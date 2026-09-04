#if UNITY_EDITOR
using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ova.Editor
{
    /// <summary>
    /// Clone referenced materials / textures / audio and give the clones soup names.
    /// Disk originals are not touched. Animation PPtr curves and PlayAudio clip
    /// arrays are retargeted so swaps still drive the clones (AO 0.4.9 constraint,
    /// OVA code).
    /// </summary>
    internal static class OvaSharedAssetPass
    {
        public static void Run(BuildContext ctx)
        {
            var state = ctx.GetState<OvaBuildState>();
            if (!state.Enabled || state.Names == null || state.Settings == null)
                return;
            if (state.Settings.assets == null || !state.Settings.assets.obfuscateClonedNames)
                return;

            var map = new Dictionary<Object, Object>();
            var renderers = ctx.AvatarRootObject.GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < renderers.Length; r++)
            {
                var mats = renderers[r].sharedMaterials;
                var changed = false;
                for (int m = 0; m < mats.Length; m++)
                {
                    var mat = CloneMaterial(ctx, state, mats[m], map);
                    if (mat != mats[m])
                    {
                        mats[m] = mat as Material;
                        changed = true;
                    }
                }

                if (changed)
                    renderers[r].sharedMaterials = mats;
            }

            var sources = ctx.AvatarRootObject.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i].clip == null) continue;
                sources[i].clip = CloneNamed<AudioClip>(ctx, state, sources[i].clip, map);
            }

            var smrs = ctx.AvatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < smrs.Length; i++)
            {
                var mesh = smrs[i].sharedMesh;
                if (mesh == null) continue;
                smrs[i].sharedMesh = CloneNamed<Mesh>(ctx, state, mesh, map);
            }

            var filters = ctx.AvatarRootObject.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; i++)
            {
                var mesh = filters[i].sharedMesh;
                if (mesh == null) continue;
                filters[i].sharedMesh = CloneNamed<Mesh>(ctx, state, mesh, map);
            }

            RewriteAnimatorObjectRefs(ctx, state, map);

            state.RenamedAssets = map.Count;
            Debug.Log("[OVA] asset name pass: clones=" + map.Count +
                      " meshes=" + state.MeshNameRewrites +
                      " clipNames=" + state.ClipNameRewrites +
                      " clipCurves=" + state.ClipObjectRewrites +
                      " behaviourAudio=" + state.BehaviourAudioRewrites);
        }

        static void RewriteAnimatorObjectRefs(BuildContext ctx, OvaBuildState state, Dictionary<Object, Object> map)
        {
            var asc = ctx.Extension<AnimatorServicesContext>();
            if (asc == null || asc.ControllerContext == null) return;

            foreach (var ctrl in asc.ControllerContext.GetAllControllers())
            {
                if (ctrl == null) continue;
                foreach (var node in ctrl.AllReachableNodes())
                {
                    var clip = node as VirtualClip;
                    if (clip != null && !clip.IsMarkerClip)
                    {
                        if (!string.IsNullOrEmpty(clip.Name))
                        {
                            clip.Name = state.Names.Next();
                            state.ClipNameRewrites++;
                        }

                        RewriteClipCurves(ctx, state, clip, map);
                    }

                    var st = node as VirtualState;
                    if (st != null)
                        RewriteBehaviourAudio(ctx, state, st.Behaviours, map);

                    var sm = node as VirtualStateMachine;
                    if (sm != null)
                        RewriteBehaviourAudio(ctx, state, sm.Behaviours, map);

                    var layer = node as VirtualLayer;
                    if (layer == null || layer.SyncedLayerBehaviourOverrides == null) continue;
                    foreach (var kv in layer.SyncedLayerBehaviourOverrides)
                        RewriteBehaviourAudio(ctx, state, kv.Value, map);
                }
            }
        }

        static void RewriteClipCurves(
            BuildContext ctx,
            OvaBuildState state,
            VirtualClip clip,
            Dictionary<Object, Object> map)
        {
            var bindings = new List<EditorCurveBinding>(clip.GetObjectCurveBindings());
            for (int i = 0; i < bindings.Count; i++)
            {
                var b = bindings[i];
                var curve = clip.GetObjectCurve(b);
                if (curve == null || curve.Length == 0) continue;

                var dirty = false;
                var next = new ObjectReferenceKeyframe[curve.Length];
                for (int k = 0; k < curve.Length; k++)
                {
                    next[k] = curve[k];
                    var mapped = MapRef(ctx, state, curve[k].value, map);
                    if (mapped != curve[k].value)
                    {
                        next[k].value = mapped;
                        dirty = true;
                    }
                }

                if (!dirty) continue;
                clip.SetObjectCurve(b, next);
                state.ClipObjectRewrites++;
            }
        }

        static void RewriteBehaviourAudio(
            BuildContext ctx,
            OvaBuildState state,
            IEnumerable<StateMachineBehaviour> behaviours,
            Dictionary<Object, Object> map)
        {
            if (behaviours == null) return;
            foreach (var mb in behaviours)
            {
                if (mb == null) continue;
                var so = new SerializedObject(mb);
                var it = so.GetIterator();
                var enter = true;
                var dirty = false;
                while (it.Next(enter))
                {
                    enter = it.propertyType == SerializedPropertyType.Generic || it.isArray;
                    if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                    var clip = it.objectReferenceValue as AudioClip;
                    if (clip == null) continue;
                    var mapped = CloneNamed<AudioClip>(ctx, state, clip, map);
                    if (mapped == clip) continue;
                    it.objectReferenceValue = mapped;
                    dirty = true;
                    state.BehaviourAudioRewrites++;
                }

                if (dirty)
                    so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static Object MapRef(BuildContext ctx, OvaBuildState state, Object src, Dictionary<Object, Object> map)
        {
            if (src == null) return null;
            var mat = src as Material;
            if (mat != null) return CloneMaterial(ctx, state, mat, map);
            var tex2d = src as Texture2D;
            if (tex2d != null) return CloneNamed<Texture>(ctx, state, tex2d, map);
            var audio = src as AudioClip;
            if (audio != null) return CloneNamed<AudioClip>(ctx, state, audio, map);
            return src;
        }

        static Object CloneMaterial(BuildContext ctx, OvaBuildState state, Material src, Dictionary<Object, Object> map)
        {
            if (src == null) return null;
            Object existing;
            if (map.TryGetValue(src, out existing))
                return existing;
            var clone = Object.Instantiate(src);
            var soup = state.Names.Next();
            clone.name = soup;
            ctx.AssetSaver.SaveAsset(clone);
            clone.name = soup;
            ObjectRegistry.RegisterReplacedObject(src, clone);
            map[src] = clone;

            var texNames = clone.GetTexturePropertyNames();
            for (int i = 0; i < texNames.Length; i++)
            {
                var tex = clone.GetTexture(texNames[i]);
                if (tex == null || tex is RenderTexture) continue;
                clone.SetTexture(texNames[i], CloneNamed<Texture>(ctx, state, tex, map));
            }

            return clone;
        }

        static T CloneNamed<T>(BuildContext ctx, OvaBuildState state, T src, Dictionary<Object, Object> map)
            where T : Object
        {
            if (src == null) return null;
            Object existing;
            if (map.TryGetValue(src, out existing))
                return (T)existing;
            var tex2d = src as Texture2D;
            if (tex2d != null)
            {
                var texClone = new Texture2D(2, 2);
                EditorUtility.CopySerialized(tex2d, texClone);
                var texSoup = state.Names.Next();
                texClone.name = texSoup;
                ctx.AssetSaver.SaveAsset(texClone);
                texClone.name = texSoup;
                ObjectRegistry.RegisterReplacedObject(src, texClone);
                map[src] = texClone;
                return (T)(Object)texClone;
            }

            T clone;
            try { clone = Object.Instantiate(src); }
            catch { return src; }
            if (clone == null) return src;
            var soup = state.Names.Next();
            clone.name = soup;
            ctx.AssetSaver.SaveAsset(clone);
            clone.name = soup;
            ObjectRegistry.RegisterReplacedObject(src, clone);
            map[src] = clone;
            if (src is Mesh)
                state.MeshNameRewrites++;
            return clone;
        }
    }
}
#endif
