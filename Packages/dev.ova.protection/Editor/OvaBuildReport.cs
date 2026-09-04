#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using nadena.dev.ndmf;
using UnityEditor.PackageManager;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Ova.Editor
{
    /// <summary>
    /// Writes Library/OVA/last-build-report.json on the Unity project (not the OVA repo).
    /// </summary>
    internal static class OvaBuildReport
    {
        public static void Run(BuildContext ctx)
        {
            var state = ctx.GetState<OvaBuildState>();
            if (!state.Enabled || state.Settings == null || ctx.AvatarRootObject == null)
                return;

            OvaSettingsStore.PinProjectRoot();
            var root = ctx.AvatarRootObject;
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            var smrs = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var decoyBudget = 0;
            if (state.Settings.names != null)
                decoyBudget = OvaSettingsStore.ClampDecoyCount(state.Settings.names.decoyBlendShapeCount);

            var report = new Report
            {
                ok = true,
                schema = "ova-build-report-v1",
                atUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                packageVersion = PackageVersion(),
                avatar = root.name ?? "",
                visemeMiss = CountVisemeMiss(descriptor),
                origSmr = state.OrigSmr,
                cloneSmr = smrs.Length,
                decoyAdded = state.DecoyAdded,
                decoyBudget = decoyBudget,
                bodyGo = HasBodyGo(root.transform),
                parameterRenamed = state.RenamedParameters,
                watermarkMeshes = state.WatermarkedMeshes,
                watermarkStatus = "editor-only-unverified",
                lockFingerprints = false
            };

            var path = OvaSettingsStore.Resolve(OvaSettingsStore.LastBuildRelative);
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, JsonUtility.ToJson(report, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[OVA] last-build report: " + e.Message);
            }
        }

        static int CountVisemeMiss(VRCAvatarDescriptor descriptor)
        {
            if (descriptor == null || descriptor.VisemeBlendShapes == null)
                return 0;
            var have = new HashSet<string>(StringComparer.Ordinal);
            var mesh = descriptor.VisemeSkinnedMesh != null ? descriptor.VisemeSkinnedMesh.sharedMesh : null;
            if (mesh != null)
            {
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    var n = mesh.GetBlendShapeName(i);
                    if (!string.IsNullOrEmpty(n))
                        have.Add(n);
                }
            }

            var miss = 0;
            var slots = descriptor.VisemeBlendShapes;
            for (int i = 0; i < slots.Length; i++)
            {
                if (string.IsNullOrEmpty(slots[i])) continue;
                if (!have.Contains(slots[i]))
                    miss++;
            }

            return miss;
        }

        static bool HasBodyGo(Transform root)
        {
            if (root == null) return false;
            var all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name == "Body")
                    return true;
            }

            return false;
        }

        static string PackageVersion()
        {
            try
            {
                var info = PackageInfo.FindForAssembly(typeof(OvaBuildReport).Assembly);
                if (info != null && !string.IsNullOrEmpty(info.version))
                    return info.version;
            }
            catch
            {
            }

            try
            {
                var jsonPath = Path.Combine(OvaSettingsStore.ProjectRoot, "Packages", "dev.ova.protection", "package.json");
                if (File.Exists(jsonPath))
                {
                    var json = File.ReadAllText(jsonPath);
                    const string key = "\"version\"";
                    var at = json.IndexOf(key, StringComparison.Ordinal);
                    if (at >= 0)
                    {
                        var colon = json.IndexOf(':', at + key.Length);
                        var q1 = json.IndexOf('"', colon + 1);
                        var q2 = q1 >= 0 ? json.IndexOf('"', q1 + 1) : -1;
                        if (q1 >= 0 && q2 > q1)
                            return json.Substring(q1 + 1, q2 - q1 - 1);
                    }
                }
            }
            catch
            {
            }

            return "0.2.0";
        }

        [Serializable]
        class Report
        {
            public bool ok;
            public string schema;
            public string atUtc;
            public string packageVersion;
            public string avatar;
            public int visemeMiss;
            public int origSmr;
            public int cloneSmr;
            public int decoyAdded;
            public int decoyBudget;
            public bool bodyGo;
            public int parameterRenamed;
            public int watermarkMeshes;
            public string watermarkStatus;
            public bool lockFingerprints;
        }
    }
}
#endif
