using System.IO;
using UnityEngine;

namespace Ova.Editor
{
    internal static class OvaSettingsStore
    {
        public const string DefaultRelativePath = "Library/OVA/settings.json";
        public const string ActivePointer = "Library/OVA/active.json";
        public const string LastBuildRelative = "Library/OVA/last-build-report.json";
        public const int MaxDecoyBlendShapes = 32;

        static string _projectRoot;

        /// <summary>Call from the Unity main thread before ova-web serves requests.</summary>
        public static void PinProjectRoot()
        {
            if (!string.IsNullOrEmpty(_projectRoot)) return;
            var data = Application.dataPath;
            _projectRoot = Path.GetDirectoryName(data) ?? data;
        }

        public static string ProjectRoot
        {
            get
            {
                if (string.IsNullOrEmpty(_projectRoot))
                    PinProjectRoot();
                return _projectRoot;
            }
        }

        public static string Resolve(string relativeOrAbsolute)
        {
            if (string.IsNullOrEmpty(relativeOrAbsolute))
                relativeOrAbsolute = DefaultRelativePath;
            if (Path.IsPathRooted(relativeOrAbsolute))
                return relativeOrAbsolute;
            return Path.GetFullPath(Path.Combine(ProjectRoot, relativeOrAbsolute.Replace('/', Path.DirectorySeparatorChar)));
        }

        public static OvaSettings LoadOrDefault(string relativeOrAbsolute, OvaSettings embedded)
        {
            var path = Resolve(relativeOrAbsolute);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var parsed = JsonUtility.FromJson<OvaSettings>(json);
                if (parsed != null && parsed.version >= 1)
                    return Normalize(parsed);
            }

            return Normalize(embedded != null ? Clone(embedded) : new OvaSettings());
        }

        public static OvaSettings Normalize(OvaSettings s)
        {
            if (s == null) s = new OvaSettings();
            if (s.version < 1) s.version = 1;
            if (s.names == null) s.names = new OvaNamesLayer();
            s.names.decoyBlendShapeCount = ClampDecoyCount(s.names.decoyBlendShapeCount);
            if (s.parameters == null) s.parameters = new OvaParamLayer();
            if (s.assets == null) s.assets = new OvaAssetLayer();
            if (s.watermark == null) s.watermark = new OvaWatermarkLayer();
            if (s.crypto == null) s.crypto = new OvaCryptoLayer();
            if (s.attest == null) s.attest = new OvaAttestLayer();
            if (s.ui == null) s.ui = new OvaUiLayer();
            if (string.IsNullOrEmpty(s.ui.locale)) s.ui.locale = "en";
            if (string.IsNullOrEmpty(s.ui.mode)) s.ui.mode = "expert";
            if (s.ui.projectNote == null) s.ui.projectNote = "";
            if (s.parameters.extraPreserve == null) s.parameters.extraPreserve = "";
            if (s.parameters.pinPreserve == null) s.parameters.pinPreserve = "";
            if (string.IsNullOrEmpty(s.attest.provider)) s.attest.provider = "off";
            if (string.IsNullOrEmpty(s.attest.branch)) s.attest.branch = "main";
            if (string.IsNullOrEmpty(s.attest.path)) s.attest.path = "ova-attest.json";
            return s;
        }

        public static void Save(string relativeOrAbsolute, OvaSettings settings)
        {
            var path = Resolve(relativeOrAbsolute);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonUtility.ToJson(Normalize(settings), true));
        }

        public static void WriteActivePointer(string settingsRelative)
        {
            var path = Resolve(ActivePointer);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonUtility.ToJson(new ActiveFile { settingsPath = settingsRelative ?? DefaultRelativePath }, true));
        }

        public static int ClampDecoyCount(int n)
        {
            if (n < 0) return 0;
            if (n > MaxDecoyBlendShapes) return MaxDecoyBlendShapes;
            return n;
        }

        public static OvaSettings Clone(OvaSettings src)
        {
            return JsonUtility.FromJson<OvaSettings>(JsonUtility.ToJson(src));
        }

        [System.Serializable]
        public class ActiveFile
        {
            public string settingsPath;
        }
    }
}
