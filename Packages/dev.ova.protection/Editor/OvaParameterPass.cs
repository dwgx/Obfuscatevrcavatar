#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace Ova.Editor
{
    internal static class OvaParameterPass
    {
        public static void Run(BuildContext ctx)
        {
            var state = ctx.GetState<OvaBuildState>();
            if (!state.Enabled || state.Names == null || state.Settings == null)
                return;
            if (state.Settings.parameters == null || !state.Settings.parameters.obfuscate)
            {
                Debug.Log("[OVA] parameter pass skipped");
                return;
            }

            state.PhysBonePrefixes.Clear();
            var contacts = new HashSet<string>(StringComparer.Ordinal);
            OvaNameKeep.CollectDynamics(ctx.AvatarRootObject, state.PhysBonePrefixes, contacts);

            var preserve = OvaPreserve.Build(state.Settings, ctx.AvatarRootObject, state.PhysBonePrefixes);
            state.PreserveParameters.Clear();
            foreach (var p in preserve)
                state.PreserveParameters.Add(p);

            var map = state.ParameterRenames;
            map.Clear();
            var asc = ctx.Extension<AnimatorServicesContext>();
            Collect(asc, ctx.AvatarRootObject, state, map);
            foreach (var c in contacts)
                Consider(c, state, map);

            foreach (var ctrl in asc.ControllerContext.GetAllControllers())
                OvaAnimatorRewrite.MapParameters(ctrl, map);

            RewriteExpressionAssets(ctx, map);
            OvaSerializedRename.ReplaceOnBehaviours(ctx.AvatarRootObject, map);

            state.RenamedParameters = map.Count;
            Debug.Log("[OVA] parameter pass: renamed=" + map.Count + " preserved-hints=" + preserve.Count);
        }

        static void Collect(AnimatorServicesContext asc, GameObject root, OvaBuildState state, Dictionary<string, string> map)
        {
            foreach (var ctrl in asc.ControllerContext.GetAllControllers())
            {
                if (ctrl == null) continue;
                foreach (var kv in ctrl.Parameters)
                    Consider(kv.Key, state, map);
            }

            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            if (descriptor != null && descriptor.expressionParameters != null && descriptor.expressionParameters.parameters != null)
            {
                var list = descriptor.expressionParameters.parameters;
                for (int i = 0; i < list.Length; i++)
                    Consider(list[i].name, state, map);
            }
        }

        static void Consider(string name, OvaBuildState state, Dictionary<string, string> map)
        {
            if (string.IsNullOrEmpty(name) || map.ContainsKey(name)) return;
            if (OvaPreserve.ShouldKeep(name, state.PreserveParameters, state.Settings, state.PhysBonePrefixes))
                return;
            state.Names.Reserve(name);
            map[name] = state.Names.Next();
        }

        static void RewriteExpressionAssets(BuildContext ctx, Dictionary<string, string> map)
        {
            var descriptor = ctx.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null) return;

            if (descriptor.expressionParameters != null)
            {
                var src = descriptor.expressionParameters;
                var clone = Object.Instantiate(src);
                clone.name = src.name;
                if (clone.parameters != null)
                {
                    for (int i = 0; i < clone.parameters.Length; i++)
                    {
                        string nn;
                        if (map.TryGetValue(clone.parameters[i].name, out nn))
                            clone.parameters[i].name = nn;
                    }
                }

                ctx.AssetSaver.SaveAsset(clone);
                ObjectRegistry.RegisterReplacedObject(src, clone);
                descriptor.expressionParameters = clone;
            }

            if (descriptor.expressionsMenu != null)
            {
                var seen = new Dictionary<VRCExpressionsMenu, VRCExpressionsMenu>();
                descriptor.expressionsMenu = CloneMenu(ctx, descriptor.expressionsMenu, map, seen);
            }
        }

        static VRCExpressionsMenu CloneMenu(
            BuildContext ctx,
            VRCExpressionsMenu src,
            Dictionary<string, string> map,
            Dictionary<VRCExpressionsMenu, VRCExpressionsMenu> seen)
        {
            if (src == null) return null;
            VRCExpressionsMenu existing;
            if (seen.TryGetValue(src, out existing))
                return existing;
            var clone = Object.Instantiate(src);
            clone.name = src.name;
            seen[src] = clone;
            if (clone.controls != null)
            {
                for (int i = 0; i < clone.controls.Count; i++)
                {
                    var c = clone.controls[i];
                    if (c.parameter != null && !string.IsNullOrEmpty(c.parameter.name))
                    {
                        string nn;
                        if (map.TryGetValue(c.parameter.name, out nn))
                            c.parameter.name = nn;
                    }

                    if (c.subParameters != null)
                    {
                        for (int s = 0; s < c.subParameters.Length; s++)
                        {
                            if (c.subParameters[s] == null || string.IsNullOrEmpty(c.subParameters[s].name))
                                continue;
                            string nn;
                            if (map.TryGetValue(c.subParameters[s].name, out nn))
                                c.subParameters[s].name = nn;
                        }
                    }

                    if (c.subMenu != null)
                        c.subMenu = CloneMenu(ctx, c.subMenu, map, seen);
                }
            }

            ctx.AssetSaver.SaveAsset(clone);
            ObjectRegistry.RegisterReplacedObject(src, clone);
            return clone;
        }
    }
}
#endif
