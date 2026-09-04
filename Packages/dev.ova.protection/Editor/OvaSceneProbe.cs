#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace Ova.Editor
{
    internal static class OvaSceneProbe
    {
        public static string ToJson(OvaSettings settings)
        {
            settings = OvaSettingsStore.Normalize(settings);
            var dto = new Dto { ok = true, unity = true };
            var avatar = FindAvatar();
            if (avatar == null)
            {
                dto.hasAvatar = false;
                dto.note = "场景里没有 OVA Protection / VRC Avatar Descriptor。列表为空。";
                dto.items = Array.Empty<Row>();
                dto.physbonePrefixes = Array.Empty<string>();
                return JsonUtility.ToJson(dto);
            }

            dto.hasAvatar = true;
            dto.avatar = avatar.name;
            var prefixes = new HashSet<string>(StringComparer.Ordinal);
            var contacts = new HashSet<string>(StringComparer.Ordinal);
            OvaNameKeep.CollectDynamics(avatar, prefixes, contacts);

            var acc = new Dictionary<string, Acc>(StringComparer.Ordinal);
            CollectAnimator(acc, avatar);
            CollectExpression(acc, avatar);
            foreach (var c in contacts)
                Get(acc, c).contact = true;
            foreach (var prefix in prefixes)
            {
                if (string.IsNullOrEmpty(prefix)) continue;
                Get(acc, prefix).physbone = true;
            }

            foreach (var kv in acc)
            {
                if (OvaNameKeep.IsPhysBoneName(kv.Key, prefixes))
                    kv.Value.physbone = true;
            }

            var names = new List<string>(acc.Keys);
            names.Sort(StringComparer.Ordinal);
            var rows = new Row[names.Count];
            var rename = 0;
            for (int i = 0; i < names.Count; i++)
            {
                var n = names[i];
                var a = acc[n];
                string keep;
                string reason;
                OvaPreserve.Classify(n, settings, prefixes, out keep, out reason);
                if (keep == "none") rename++;
                rows[i] = new Row
                {
                    name = n,
                    source = SourceOf(a),
                    keep = keep,
                    reason = reason
                };
            }

            dto.items = rows;
            dto.count = rows.Length;
            dto.rename = rename;
            dto.note = avatar.name;
            CountNames(avatar, settings, dto);
            var prefixList = new List<string>(prefixes);
            prefixList.Sort(StringComparer.Ordinal);
            dto.physbonePrefixes = prefixList.ToArray();
            return JsonUtility.ToJson(dto);
        }

        static string SourceOf(Acc a)
        {
            if (a.animator && a.expression) return "both";
            if (a.expression) return "expression";
            if (a.animator) return "animator";
            if (a.physbone) return "physbone";
            if (a.contact) return "contact";
            return "other";
        }

        static void CountNames(GameObject avatar, OvaSettings settings, Dto dto)
        {
            var root = avatar.transform;
            var structural = OvaNameKeep.BuildStructural(root, settings);
            var transforms = root.GetComponentsInChildren<Transform>(true);
            dto.transforms = transforms.Length;
            var tRename = 0;
            for (int i = 0; i < transforms.Length; i++)
            {
                if (!OvaNameKeep.KeepTransform(transforms[i], root, structural, settings))
                    tRename++;
            }

            dto.transformsRename = tRename;

            Mesh visemeMesh = null;
            var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
            if (descriptor != null && descriptor.VisemeSkinnedMesh != null)
                visemeMesh = descriptor.VisemeSkinnedMesh.sharedMesh;
            var visemeSlots = OvaNameKeep.VisemeSlotNames(descriptor);

            var smrs = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var shapes = 0;
            var sRename = 0;
            for (int i = 0; i < smrs.Length; i++)
            {
                var mesh = smrs[i] != null ? smrs[i].sharedMesh : null;
                if (mesh == null) continue;
                for (int s = 0; s < mesh.blendShapeCount; s++)
                {
                    shapes++;
                    if (!OvaNameKeep.KeepBlendShape(mesh.GetBlendShapeName(s), settings, mesh == visemeMesh, visemeSlots))
                        sRename++;
                }
            }

            dto.blendshapes = shapes;
            dto.blendshapesRename = sRename;
            dto.animatorLayers = CountLayers(avatar);
        }

        static int CountLayers(GameObject root)
        {
            var n = 0;
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            if (descriptor != null)
            {
                n += CountLayerArray(descriptor.baseAnimationLayers);
                n += CountLayerArray(descriptor.specialAnimationLayers);
            }

            return n;
        }

        static int CountLayerArray(VRCAvatarDescriptor.CustomAnimLayer[] layers)
        {
            if (layers == null) return 0;
            var n = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                var ctrl = Unwrap(layers[i].animatorController);
                if (ctrl != null && ctrl.layers != null)
                    n += ctrl.layers.Length;
            }

            return n;
        }

        static GameObject FindAvatar()
        {
            var markers = Object.FindObjectsOfType<OvaProtection>(true);
            for (int i = 0; i < markers.Length; i++)
            {
                if (markers[i] != null) return markers[i].gameObject;
            }

            var descriptors = Object.FindObjectsOfType<VRCAvatarDescriptor>(true);
            return descriptors.Length > 0 ? descriptors[0].gameObject : null;
        }

        static void CollectExpression(Dictionary<string, Acc> acc, GameObject root)
        {
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null || descriptor.expressionParameters == null) return;
            var list = descriptor.expressionParameters.parameters;
            if (list == null) return;
            for (int i = 0; i < list.Length; i++)
            {
                var n = list[i].name;
                if (string.IsNullOrEmpty(n)) continue;
                Get(acc, n).expression = true;
            }
        }

        static void CollectAnimator(Dictionary<string, Acc> acc, GameObject root)
        {
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            if (descriptor != null)
            {
                AddLayers(acc, descriptor.baseAnimationLayers);
                AddLayers(acc, descriptor.specialAnimationLayers);
            }

            var animators = root.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                if (animators[i] != null)
                    AddController(acc, animators[i].runtimeAnimatorController);
            }
        }

        static void AddLayers(Dictionary<string, Acc> acc, VRCAvatarDescriptor.CustomAnimLayer[] layers)
        {
            if (layers == null) return;
            for (int i = 0; i < layers.Length; i++)
                AddController(acc, layers[i].animatorController);
        }

        static void AddController(Dictionary<string, Acc> acc, RuntimeAnimatorController rac)
        {
            var ctrl = Unwrap(rac);
            if (ctrl == null || ctrl.parameters == null) return;
            var list = ctrl.parameters;
            for (int i = 0; i < list.Length; i++)
            {
                var n = list[i].name;
                if (string.IsNullOrEmpty(n)) continue;
                Get(acc, n).animator = true;
            }
        }

        static AnimatorController Unwrap(RuntimeAnimatorController rac)
        {
            while (rac is AnimatorOverrideController ovr)
                rac = ovr.runtimeAnimatorController;
            return rac as AnimatorController;
        }

        static Acc Get(Dictionary<string, Acc> acc, string name)
        {
            Acc a;
            if (!acc.TryGetValue(name, out a))
            {
                a = new Acc();
                acc[name] = a;
            }

            return a;
        }

        class Acc
        {
            public bool animator;
            public bool expression;
            public bool physbone;
            public bool contact;
        }

        [Serializable]
        class Dto
        {
            public bool ok;
            public bool unity;
            public bool hasAvatar;
            public string avatar = "";
            public string note = "";
            public int count;
            public int rename;
            public int transforms;
            public int transformsRename;
            public int blendshapes;
            public int blendshapesRename;
            public int animatorLayers;
            public string[] physbonePrefixes = Array.Empty<string>();
            public Row[] items = Array.Empty<Row>();
        }

        [Serializable]
        class Row
        {
            public string name = "";
            public string source = "";
            public string keep = "";
            public string reason = "";
        }
    }
}
#endif
