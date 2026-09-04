using System.Collections.Generic;
using System.Collections.Immutable;
using nadena.dev.ndmf.animator;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Ova.Editor
{
    internal static class OvaAnimatorRewrite
    {
        public static void MapParameters(VirtualAnimatorController ctrl, Dictionary<string, string> map)
        {
            if (ctrl == null || map == null || map.Count == 0) return;
            var next = ImmutableDictionary.CreateBuilder<string, AnimatorControllerParameter>();
            foreach (var kv in ctrl.Parameters)
            {
                var oldName = kv.Key;
                var p = kv.Value;
                string nn;
                if (!map.TryGetValue(oldName, out nn))
                    nn = oldName;
                var copy = new AnimatorControllerParameter
                {
                    name = nn,
                    type = p.type,
                    defaultBool = p.defaultBool,
                    defaultFloat = p.defaultFloat,
                    defaultInt = p.defaultInt
                };
                next[nn] = copy;
            }

            ctrl.Parameters = next.ToImmutable();
            foreach (var layer in ctrl.Layers)
            {
                WalkMachine(layer.StateMachine, map);
                if (layer.SyncedLayerMotionOverrides != null)
                {
                    foreach (var kv in layer.SyncedLayerMotionOverrides)
                        RewriteMotion(kv.Value, map);
                }

                if (layer.SyncedLayerBehaviourOverrides == null) continue;
                foreach (var kv in layer.SyncedLayerBehaviourOverrides)
                    RewriteBehaviours(kv.Value, map);
            }

            RewriteAnimatorParamCurves(ctrl, map);
        }

        public static void ObfuscateStructure(VirtualAnimatorController ctrl, OvaNameGenerator names, OvaSettings settings, OvaBuildState state)
        {
            if (ctrl == null || names == null || settings == null || settings.names == null) return;
            foreach (var layer in ctrl.Layers)
            {
                if (settings.names.obfuscateAnimatorLayers && layer != null && !string.IsNullOrEmpty(layer.Name))
                {
                    layer.Name = names.Next();
                    state.RenamedLayers++;
                }

                if (settings.names.obfuscateStates)
                    RenameStates(layer != null ? layer.StateMachine : null, names);
            }
        }

        static void RenameStates(VirtualStateMachine sm, OvaNameGenerator names)
        {
            if (sm == null) return;
            if (!string.IsNullOrEmpty(sm.Name) && sm.Name != "Base Layer")
                sm.Name = names.Next();
            foreach (var child in sm.States)
            {
                if (child.State != null)
                    child.State.Name = names.Next();
            }

            foreach (var child in sm.StateMachines)
            {
                if (child.StateMachine != null)
                    RenameStates(child.StateMachine, names);
            }
        }

        static void WalkMachine(VirtualStateMachine sm, Dictionary<string, string> map)
        {
            if (sm == null) return;
            RewriteTransitions(sm.AnyStateTransitions, map);
            RewriteTransitions(sm.EntryTransitions, map);
            RewriteBehaviours(sm.Behaviours, map);
            foreach (var kv in sm.StateMachineTransitions)
                RewriteTransitions(kv.Value, map);
            foreach (var child in sm.States)
            {
                var st = child.State;
                if (st == null) continue;
                st.SpeedParameter = MapOne(st.SpeedParameter, map);
                st.TimeParameter = MapOne(st.TimeParameter, map);
                st.MirrorParameter = MapOne(st.MirrorParameter, map);
                st.CycleOffsetParameter = MapOne(st.CycleOffsetParameter, map);
                RewriteTransitions(st.Transitions, map);
                RewriteBehaviours(st.Behaviours, map);
                RewriteMotion(st.Motion, map);
            }

            foreach (var child in sm.StateMachines)
                WalkMachine(child.StateMachine, map);
        }

        static void RewriteMotion(VirtualMotion motion, Dictionary<string, string> map)
        {
            var tree = motion as VirtualBlendTree;
            if (tree == null) return;
            tree.BlendParameter = MapOne(tree.BlendParameter, map) ?? tree.BlendParameter;
            tree.BlendParameterY = MapOne(tree.BlendParameterY, map) ?? tree.BlendParameterY;
            var children = tree.Children;
            if (children == null) return;
            for (int i = 0; i < children.Count; i++)
            {
                var c = children[i];
                c.DirectBlendParameter = MapOne(c.DirectBlendParameter, map) ?? c.DirectBlendParameter;
                RewriteMotion(c.Motion, map);
            }
        }

        static void RewriteTransitions<T>(ImmutableList<T> list, Dictionary<string, string> map)
            where T : VirtualTransitionBase
        {
            if (list == null) return;
            for (int t = 0; t < list.Count; t++)
            {
                var tr = list[t];
                if (tr == null) continue;
                var conds = tr.Conditions;
                if (conds == null || conds.Count == 0) continue;
                var b = conds.ToBuilder();
                for (int i = 0; i < b.Count; i++)
                {
                    var c = b[i];
                    string nn;
                    if (c.parameter != null && map.TryGetValue(c.parameter, out nn))
                    {
                        c.parameter = nn;
                        b[i] = c;
                    }
                }

                tr.Conditions = b.ToImmutable();
            }
        }

        static void RewriteBehaviours(ImmutableList<StateMachineBehaviour> behaviours, Dictionary<string, string> map)
        {
            if (behaviours == null) return;
            for (int i = 0; i < behaviours.Count; i++)
                OvaSerializedRename.ReplaceExactStrings(behaviours[i], map);
        }

        static string MapOne(string name, Dictionary<string, string> map)
        {
            if (string.IsNullOrEmpty(name)) return name;
            string nn;
            return map.TryGetValue(name, out nn) ? nn : name;
        }

        static void RewriteAnimatorParamCurves(VirtualAnimatorController ctrl, Dictionary<string, string> map)
        {
            if (ctrl == null) return;
            foreach (var node in ctrl.AllReachableNodes())
            {
                var clip = node as VirtualClip;
                if (clip == null || clip.IsMarkerClip) continue;
                var bindings = new List<EditorCurveBinding>(clip.GetFloatCurveBindings());
                for (int i = 0; i < bindings.Count; i++)
                {
                    var b = bindings[i];
                    if (b.type != typeof(Animator) || string.IsNullOrEmpty(b.propertyName))
                        continue;
                    string nn;
                    if (!map.TryGetValue(b.propertyName, out nn))
                        continue;
                    var curve = clip.GetFloatCurve(b);
                    if (curve == null) continue;
                    clip.SetFloatCurve(b, null);
                    var nb = b;
                    nb.propertyName = nn;
                    clip.SetFloatCurve(nb, curve);
                }
            }
        }
    }
}
