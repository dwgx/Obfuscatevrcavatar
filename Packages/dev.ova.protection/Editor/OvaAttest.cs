#if UNITY_EDITOR
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace Ova.Editor
{
    /// <summary>
    /// GitHub / Gitee watermark fingerprint registry. Tokens never leave Library/OVA/secrets.json.
    /// Publish HTTP is intentionally unwired in this skeleton.
    /// </summary>
    internal static class OvaAttest
    {
        public const string SecretsRelative = "Library/OVA/secrets.json";

        public static bool TryHandle(HttpListenerRequest req, HttpListenerResponse res, string path, string method)
        {
            if (path == "/api/attest/status" && method == "GET")
            {
                var settings = LoadSettings();
                var secrets = LoadSecrets();
                var a = settings.attest;
                WriteJson(res, JsonUtility.ToJson(new StatusDto
                {
                    ok = true,
                    provider = a.provider,
                    owner = a.owner,
                    repo = a.repo,
                    branch = a.branch,
                    path = a.path,
                    hasGithubSecret = !string.IsNullOrEmpty(secrets.githubToken),
                    hasGiteeSecret = !string.IsNullOrEmpty(secrets.giteeToken),
                    wired = false,
                    algo = OvaFingerprint.Algo,
                    note = "publish-not-wired"
                }));
                return true;
            }

            if (path == "/api/attest/fingerprint" && method == "GET")
            {
                var settings = LoadSettings();
                WriteJson(res, JsonUtility.ToJson(new FingerprintDto
                {
                    ok = true,
                    algo = OvaFingerprint.Algo,
                    fingerprint = OvaFingerprint.Compute(settings),
                    payloadKind = "seed+nameLength+watermark"
                }));
                return true;
            }

            if (path == "/api/attest/secret" && method == "POST")
            {
                var body = ReadBody(req);
                var parsed = JsonUtility.FromJson<SecretDto>(body);
                if (parsed == null || !IsProvider(parsed.provider) || parsed.provider == "off")
                {
                    res.StatusCode = 400;
                    WriteJson(res, "{\"error\":\"provider must be github or gitee\"}");
                    return true;
                }

                var secrets = LoadSecrets();
                var token = parsed.token ?? "";
                if (parsed.provider == "github") secrets.githubToken = token;
                else secrets.giteeToken = token;
                SaveSecrets(secrets);
                WriteJson(res, "{\"ok\":true,\"stored\":" + (token.Length > 0 ? "true" : "false") + "}");
                return true;
            }

            if (path == "/api/attest/publish" && method == "POST")
            {
                var settings = LoadSettings();
                var a = settings.attest;
                var fp = OvaFingerprint.Compute(settings);
                res.StatusCode = 501;
                WriteJson(res, JsonUtility.ToJson(new PublishDto
                {
                    ok = false,
                    error = "publish-not-wired",
                    message = "Architecture only. Next wire PUTs a public fingerprint JSON. Never the PAT, never a decrypt key.",
                    wouldPost = new WouldPostDto
                    {
                        provider = a.provider,
                        owner = a.owner,
                        repo = a.repo,
                        branch = a.branch,
                        path = a.path,
                        algo = OvaFingerprint.Algo,
                        fingerprint = fp
                    }
                }));
                return true;
            }

            return false;
        }

        static OvaSettings LoadSettings()
        {
            var pointer = OvaSettingsStore.Resolve(OvaSettingsStore.ActivePointer);
            var relative = OvaSettingsStore.DefaultRelativePath;
            if (File.Exists(pointer))
            {
                var active = JsonUtility.FromJson<OvaSettingsStore.ActiveFile>(File.ReadAllText(pointer));
                if (active != null && !string.IsNullOrEmpty(active.settingsPath))
                    relative = active.settingsPath;
            }

            return OvaSettingsStore.LoadOrDefault(relative, null);
        }

        static OvaAttestSecrets LoadSecrets()
        {
            var path = OvaSettingsStore.Resolve(SecretsRelative);
            if (!File.Exists(path)) return new OvaAttestSecrets();
            var parsed = JsonUtility.FromJson<OvaAttestSecrets>(File.ReadAllText(path));
            return parsed ?? new OvaAttestSecrets();
        }

        static void SaveSecrets(OvaAttestSecrets secrets)
        {
            var path = OvaSettingsStore.Resolve(SecretsRelative);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonUtility.ToJson(secrets ?? new OvaAttestSecrets(), true));
        }

        static bool IsProvider(string provider)
        {
            return provider == "off" || provider == "github" || provider == "gitee";
        }

        static string ReadBody(HttpListenerRequest req)
        {
            using (var reader = new StreamReader(req.InputStream, req.ContentEncoding ?? Encoding.UTF8))
                return reader.ReadToEnd();
        }

        static void WriteJson(HttpListenerResponse res, string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json ?? "{}");
            res.ContentType = "application/json; charset=utf-8";
            res.ContentLength64 = bytes.Length;
            res.Headers.Add("Cache-Control", "no-store");
            res.Headers.Add("X-Content-Type-Options", "nosniff");
            res.OutputStream.Write(bytes, 0, bytes.Length);
            res.OutputStream.Close();
        }

        [Serializable]
        class OvaAttestSecrets
        {
            public string githubToken = "";
            public string giteeToken = "";
        }

        [Serializable]
        class SecretDto
        {
            public string provider = "";
            public string token = "";
        }

        [Serializable]
        class StatusDto
        {
            public bool ok;
            public string provider;
            public string owner;
            public string repo;
            public string branch;
            public string path;
            public bool hasGithubSecret;
            public bool hasGiteeSecret;
            public bool wired;
            public string algo;
            public string note;
        }

        [Serializable]
        class FingerprintDto
        {
            public bool ok;
            public string algo;
            public string fingerprint;
            public string payloadKind;
        }

        [Serializable]
        class PublishDto
        {
            public bool ok;
            public string error;
            public string message;
            public WouldPostDto wouldPost;
        }

        [Serializable]
        class WouldPostDto
        {
            public string provider;
            public string owner;
            public string repo;
            public string branch;
            public string path;
            public string algo;
            public string fingerprint;
        }
    }
}
#endif
