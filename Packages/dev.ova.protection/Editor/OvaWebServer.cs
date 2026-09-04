#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Ova.Editor
{
    [InitializeOnLoad]
    internal static class OvaWebServer
    {
        public const int Port = 17849;
        static HttpListener _listener;
        static Thread _thread;
        static string _wwwroot;
        static readonly object Gate = new object();
        static readonly ConcurrentQueue<Action> MainJobs = new ConcurrentQueue<Action>();

        static OvaWebServer()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            EditorApplication.quitting += Stop;
            EditorApplication.update += DrainMain;
        }

        public static string BaseUrl => "http://127.0.0.1:" + Port + "/";

        public static void StartAndOpen(string settingsRelative)
        {
            OvaSettingsStore.PinProjectRoot();
            OvaSettingsStore.WriteActivePointer(settingsRelative);
            var resolved = OvaSettingsStore.Resolve(settingsRelative);
            if (!File.Exists(resolved))
                OvaSettingsStore.Save(settingsRelative, new OvaSettings());
            if (!Start())
                return;
            Application.OpenURL(BaseUrl);
        }

        public static bool Start()
        {
            OvaSettingsStore.PinProjectRoot();
            lock (Gate)
            {
                if (_listener != null && _listener.IsListening) return true;
                _wwwroot = LocateWwwRoot();
                if (string.IsNullOrEmpty(_wwwroot) || !Directory.Exists(_wwwroot))
                {
                    Debug.LogError("[OVA] ova-web wwwroot missing");
                    return false;
                }

                _listener = new HttpListener();
                _listener.Prefixes.Add(BaseUrl);
                try
                {
                    _listener.Start();
                }
                catch (Exception e)
                {
                    Debug.LogError("[OVA] ova-web bind failed: " + e.Message);
                    _listener = null;
                    return false;
                }

                _thread = new Thread(Loop) { IsBackground = true, Name = "ova-web" };
                _thread.Start();
                Debug.Log("[OVA] ova-web " + BaseUrl + " root=" + _wwwroot);
                return true;
            }
        }

        public static void Stop()
        {
            lock (Gate)
            {
                if (_listener == null) return;
                try { _listener.Stop(); } catch { }
                try { _listener.Close(); } catch { }
                _listener = null;
                _thread = null;
            }
        }

        static string LocateWwwRoot()
        {
            var project = OvaSettingsStore.ProjectRoot;
            var inHost = Path.Combine(project, "host", "ova-web", "wwwroot");
            if (Directory.Exists(inHost)) return inHost;
            var inPkg = Path.Combine(project, "Packages", "dev.ova.protection", "Editor", "Web", "wwwroot");
            if (Directory.Exists(inPkg)) return inPkg;
            return inHost;
        }

        static void Loop()
        {
            while (_listener != null && _listener.IsListening)
            {
                HttpListenerContext ctx;
                try { ctx = _listener.GetContext(); }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }

                try { Handle(ctx); }
                catch (Exception e)
                {
                    Debug.LogWarning("[OVA] ova-web request: " + e.Message);
                }
            }
        }

        static void Handle(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var res = ctx.Response;
            var path = req.Url.AbsolutePath;
            var method = req.HttpMethod;

            if (path.StartsWith("/api/", StringComparison.Ordinal))
            {
                if (OvaAttest.TryHandle(req, res, path, method))
                    return;

                if (path == "/api/settings" && method == "GET")
                {
                    var settings = OvaSettingsStore.LoadOrDefault(ActiveRelative(), null);
                    WriteJson(res, JsonUtility.ToJson(settings));
                    return;
                }

                if (path == "/api/settings" && method == "PUT")
                {
                    string body;
                    using (var reader = new StreamReader(req.InputStream, req.ContentEncoding ?? Encoding.UTF8))
                        body = reader.ReadToEnd();
                    var parsed = JsonUtility.FromJson<OvaSettings>(body);
                    if (parsed == null || parsed.version < 1)
                    {
                        res.StatusCode = 400;
                        WriteJson(res, "{\"error\":\"bad settings\"}");
                        return;
                    }

                    OvaSettingsStore.Save(ActiveRelative(), parsed);
                    WriteJson(res, "{\"ok\":true}");
                    return;
                }

                if (path == "/api/health")
                {
                    WriteJson(res, "{\"ok\":true,\"name\":\"ova-web\",\"unity\":true}");
                    return;
                }

                if (path == "/api/last-build" && method == "GET")
                {
                    var report = OvaSettingsStore.Resolve(OvaSettingsStore.LastBuildRelative);
                    if (!File.Exists(report))
                    {
                        res.StatusCode = 404;
                        WriteJson(res, "{\"error\":\"not found\"}");
                        return;
                    }

                    WriteJson(res, File.ReadAllText(report));
                    return;
                }

                if ((path == "/api/scene/parameters" || path == "/api/scene") && method == "GET")
                {
                    var settings = OvaSettingsStore.LoadOrDefault(ActiveRelative(), null);
                    string json;
                    if (!TryOnMain(() => OvaSceneProbe.ToJson(settings), out json))
                    {
                        res.StatusCode = 504;
                        WriteJson(res, "{\"ok\":false,\"error\":\"timeout\",\"unity\":true}");
                        return;
                    }

                    WriteJson(res, json);
                    return;
                }

                res.StatusCode = 404;
                WriteJson(res, "{\"error\":\"not found\"}");
                return;
            }

            ServeStatic(res, path);
        }

        static string ActiveRelative()
        {
            var pointer = OvaSettingsStore.Resolve(OvaSettingsStore.ActivePointer);
            var relative = OvaSettingsStore.DefaultRelativePath;
            if (File.Exists(pointer))
            {
                var active = JsonUtility.FromJson<OvaSettingsStore.ActiveFile>(File.ReadAllText(pointer));
                if (active != null && !string.IsNullOrEmpty(active.settingsPath))
                    relative = active.settingsPath;
            }

            return relative;
        }

        static void DrainMain()
        {
            Action job;
            while (MainJobs.TryDequeue(out job))
            {
                try { job(); }
                catch (Exception e)
                {
                    Debug.LogWarning("[OVA] ova-web main: " + e.Message);
                }
            }
        }

        static bool TryOnMain(Func<string> work, out string json, int timeoutMs = 5000)
        {
            json = null;
            string local = null;
            Exception err = null;
            var done = new ManualResetEventSlim(false);
            MainJobs.Enqueue(() =>
            {
                try { local = work(); }
                catch (Exception e) { err = e; }
                finally { done.Set(); }
            });
            if (!done.Wait(timeoutMs))
                return false;
            if (err != null)
            {
                json = "{\"ok\":false,\"error\":\"probe\",\"unity\":true}";
                return true;
            }

            json = local ?? "{}";
            return true;
        }

        static void ServeStatic(HttpListenerResponse res, string urlPath)
        {
            if (urlPath == "/") urlPath = "/index.html";
            var rel = urlPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var full = Path.GetFullPath(Path.Combine(_wwwroot, rel));
            if (!full.StartsWith(Path.GetFullPath(_wwwroot), StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
            {
                res.StatusCode = 404;
                WriteBytes(res, Encoding.UTF8.GetBytes("not found"), "text/plain; charset=utf-8");
                return;
            }

            WriteBytes(res, File.ReadAllBytes(full), Mime(Path.GetExtension(full)));
        }

        static string Mime(string ext)
        {
            switch (ext.ToLowerInvariant())
            {
                case ".html": return "text/html; charset=utf-8";
                case ".css": return "text/css; charset=utf-8";
                case ".js":
                case ".mjs": return "text/javascript; charset=utf-8";
                case ".svg": return "image/svg+xml";
                case ".json": return "application/json; charset=utf-8";
                case ".woff2": return "font/woff2";
                case ".woff": return "font/woff";
                case ".png": return "image/png";
                case ".webp": return "image/webp";
                case ".ico": return "image/x-icon";
                default: return "application/octet-stream";
            }
        }

        static void WriteJson(HttpListenerResponse res, string json)
        {
            WriteBytes(res, Encoding.UTF8.GetBytes(json ?? "{}"), "application/json; charset=utf-8");
        }

        static void WriteBytes(HttpListenerResponse res, byte[] bytes, string mime)
        {
            res.ContentType = mime;
            res.ContentLength64 = bytes.Length;
            res.Headers.Add("Cache-Control", "no-store");
            res.Headers.Add("X-Content-Type-Options", "nosniff");
            res.OutputStream.Write(bytes, 0, bytes.Length);
            res.OutputStream.Close();
        }
    }
}
#endif
