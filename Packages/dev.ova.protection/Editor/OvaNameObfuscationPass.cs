#if UNITY_EDITOR
using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace Ova.Editor
{
    /// <summary>
    /// Clone-only name pass. Humanoid + ancestors + Armature + root stay.
    /// Blendshape curve keys use ObjectPathRemapper virtual paths.
    /// </summary>
    internal static class OvaNameObfuscationPass
    {
        public static void Run(BuildContext ctx)
        {
            var state = ctx.GetState<OvaBuildState>();
            if (!state.Enabled || state.Names == null || state.Settings == null)
                return;

            var names = state.Settings.names;
            if (names == null) return;

            if (names.obfuscateBlendShapes)
                RenameBlendShapes(ctx, state);

            if (names.obfuscateHierarchy)
                RenameHierarchy(ctx, state);

            AnchorMmdBody(ctx, state);

            if (names.obfuscateBlendShapes && state.BlendShapeRenames.Count > 0)
                RewriteBlendShapeCurves(ctx, state);

            if (names.obfuscateAnimatorLayers || names.obfuscateStates)
            {
                var asc = ctx.Extension<AnimatorServicesContext>();
                foreach (var ctrl in asc.ControllerContext.GetAllControllers())
                    OvaAnimatorRewrite.ObfuscateStructure(ctrl, state.Names, state.Settings, state);
            }

            Debug.Log("[OVA] name pass: blendshapes=" + state.BlendShapeRenames.Count +
                      " hierarchy=" + names.obfuscateHierarchy +
                      " layers=" + state.RenamedLayers +
                      " mmd=" + state.Settings.preserveMmd +
                      " decoy=" + OvaSettingsStore.ClampDecoyCount(names.decoyBlendShapeCount));
        }

        static string VirtualRel(BuildContext ctx, Transform t)
        {
            var asc = ctx.Extension<AnimatorServicesContext>();
            return asc.ObjectPathRemapper.GetVirtualPathForObject(t) ?? "";
        }

        static void RenameHierarchy(BuildContext ctx, OvaBuildState state)
        {
            var root = ctx.AvatarRootTransform;
            var preserve = OvaNameKeep.BuildStructural(root, state.Settings);
            var all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (OvaNameKeep.KeepTransform(t, root, preserve, state.Settings))
                    continue;
                state.Names.Reserve(t.name);
                t.name = state.Names.Next();
            }
        }

        static void AnchorMmdBody(BuildContext ctx, OvaBuildState state)
        {
            if (state.Settings == null || !state.Settings.preserveMmd) return;
            var t = OvaNameKeep.MmdBodyAnchor(ctx.AvatarRootTransform);
            if (t == null) return;
            t.name = "Body";
            Debug.Log("[OVA] mmd body anchor=" + t.name);
        }

        static void RenameBlendShapes(BuildContext ctx, OvaBuildState state)
        {
            var smrs = ctx.AvatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Mesh visemeMesh = null;
            SkinnedMeshRenderer visemeSmr = null;
            var descriptor = ctx.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
            if (descriptor != null && descriptor.VisemeSkinnedMesh != null)
            {
                visemeSmr = descriptor.VisemeSkinnedMesh;
                visemeMesh = visemeSmr.sharedMesh;
            }
            var visemeSlots = OvaNameKeep.VisemeSlotNames(descriptor);
            var slotRemap = new Dictionary<string, string>();
            var remaining = 0;
            if (state.Settings.names != null)
                remaining = OvaSettingsStore.ClampDecoyCount(state.Settings.names.decoyBlendShapeCount);
            var decoyAdded = 0;
            var carrier = PickDecoyCarrier(smrs, visemeSmr);

            for (int i = 0; i < smrs.Length; i++)
            {
                var smr = smrs[i];
                var src = smr.sharedMesh;
                if (src == null) continue;
                var add = 0;
                if (smr == carrier && remaining > 0)
                    add = remaining;
                if (src.blendShapeCount == 0 && add == 0) continue;

                var path = VirtualRel(ctx, smr.transform);
                var newNames = new string[src.blendShapeCount];
                var changed = false;
                var onVisemeMesh = src == visemeMesh;
                for (int s = 0; s < src.blendShapeCount; s++)
                {
                    var oldName = src.GetBlendShapeName(s);
                    if (OvaNameKeep.KeepBlendShape(oldName, state.Settings, onVisemeMesh, visemeSlots))
                    {
                        newNames[s] = oldName;
                        continue;
                    }
                    var nn = state.Names.Next();
                    newNames[s] = nn;
                    state.BlendShapeRenames[ShapeKey(path, oldName)] = nn;
                    if (onVisemeMesh && visemeSlots.Contains(oldName))
                        slotRemap[oldName] = nn;
                    changed = true;
                }
                if (!changed && add == 0) continue;

                var soup = state.Names.Next();
                var clone = CloneMeshWithShapes(src, newNames);
                if (add > 0)
                {
                    decoyAdded += AppendDecoyShapes(clone, state, add);
                    remaining = 0;
                }
                clone.name = soup;
                ctx.AssetSaver.SaveAsset(clone);
                clone.name = soup;
                ObjectRegistry.RegisterReplacedObject(src, clone);
                smr.sharedMesh = clone;
            }

            RemapDescriptorVisemes(descriptor, slotRemap);
            state.DecoyAdded = decoyAdded;
            Debug.Log("[OVA] decoy blendshapes added=" + decoyAdded);
        }

        static SkinnedMeshRenderer PickDecoyCarrier(SkinnedMeshRenderer[] smrs, SkinnedMeshRenderer visemeSmr)
        {
            if (visemeSmr != null && visemeSmr.sharedMesh != null)
                return visemeSmr;
            if (smrs == null) return null;
            for (int i = 0; i < smrs.Length; i++)
            {
                if (smrs[i] != null && smrs[i].sharedMesh != null)
                    return smrs[i];
            }

            return null;
        }

        static void RemapDescriptorVisemes(VRCAvatarDescriptor descriptor, Dictionary<string, string> slotRemap)
        {
            if (descriptor == null || slotRemap == null || slotRemap.Count == 0) return;
            var slots = descriptor.VisemeBlendShapes;
            if (slots == null) return;
            for (int i = 0; i < slots.Length; i++)
            {
                string nn;
                if (slots[i] != null && slotRemap.TryGetValue(slots[i], out nn))
                    slots[i] = nn;
            }

            descriptor.VisemeBlendShapes = slots;
        }

        static string ShapeKey(string path, string shape)
        {
            return path + "\n" + shape;
        }

        static Mesh CloneMeshWithShapes(Mesh src, string[] newNames)
        {
            var copy = Object.Instantiate(src);
            copy.name = src.name;
            copy.ClearBlendShapes();
            int verts = src.vertexCount;
            var dv = new Vector3[verts];
            var dn = new Vector3[verts];
            var dt = new Vector3[verts];
            for (int i = 0; i < src.blendShapeCount; i++)
            {
                int frames = src.GetBlendShapeFrameCount(i);
                for (int f = 0; f < frames; f++)
                {
                    var weight = src.GetBlendShapeFrameWeight(i, f);
                    src.GetBlendShapeFrameVertices(i, f, dv, dn, dt);
                    copy.AddBlendShapeFrame(newNames[i], weight, dv, dn, dt);
                }
            }
            return copy;
        }

        static int AppendDecoyShapes(Mesh copy, OvaBuildState state, int count)
        {
            if (copy == null || state == null || state.Names == null || count <= 0) return 0;
            int verts = copy.vertexCount;
            var dv = new Vector3[verts];
            var dn = new Vector3[verts];
            var dt = new Vector3[verts];
            if (verts > 0)
                dv[0] = new Vector3(1e-8f, 0f, 0f);
            for (int i = 0; i < count; i++)
            {
                var nn = state.Names.Next();
                copy.AddBlendShapeFrame(nn, 100f, dv, dn, dt);
            }
            return count;
        }

        static void RewriteBlendShapeCurves(BuildContext ctx, OvaBuildState state)
        {
            var asc = ctx.Extension<AnimatorServicesContext>();
            foreach (var ctrl in asc.ControllerContext.GetAllControllers())
            {
                if (ctrl == null) continue;
                foreach (var node in ctrl.AllReachableNodes())
                {
                    var clip = node as VirtualClip;
                    if (clip == null || clip.IsMarkerClip) continue;
                    RewriteVirtualClipShapes(clip, state);
                }
            }
        }

        static void RewriteVirtualClipShapes(VirtualClip clip, OvaBuildState state)
        {
            var bindings = new List<EditorCurveBinding>(clip.GetFloatCurveBindings());
            for (int i = 0; i < bindings.Count; i++)
            {
                var b = bindings[i];
                if (b.propertyName == null || !b.propertyName.StartsWith("blendShape."))
                    continue;
                var oldShape = b.propertyName.Substring("blendShape.".Length);
                string nn;
                if (!state.BlendShapeRenames.TryGetValue(ShapeKey(b.path ?? "", oldShape), out nn))
                    continue;
                var curve = clip.GetFloatCurve(b);
                if (curve == null) continue;
                clip.SetFloatCurve(b, null);
                var nb = b;
                nb.propertyName = "blendShape." + nn;
                clip.SetFloatCurve(nb, curve);
            }
        }
    }
}
#endif
