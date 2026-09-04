#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Ova.Editor
{
    /// <summary>
    /// Hierarchy / blendshape keep rules, plus PhysBone prefix / contact names.
    /// Parameter keep lives in <see cref="OvaPreserve"/> (does not use name substrings).
    /// </summary>
    internal static class OvaNameKeep
    {
        public static readonly string[] MmdShapeHints =
        {
            "あ", "い", "う", "え", "お", "まばたき", "笑い", "怒り", "困る",
            "あっかんべー", "じと目", "なごみ", "びっくり", "喜び", "悲哀",
            "口角上げ", "口角下げ", "瞳小", "瞳大", "Blink", "Blink_L", "Blink_R",
            "vrc.v_sil", "vrc.v_pp", "vrc.v_ff", "vrc.v_th", "vrc.v_dd", "vrc.v_kk",
            "vrc.v_ch", "vrc.v_ss", "vrc.v_nn", "vrc.v_rr", "vrc.v_aa", "vrc.v_e",
            "vrc.v_ih", "vrc.v_oh", "vrc.v_ou"
        };

        public static bool MatchesSubstring(string name, string csv)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(csv)) return false;
            var parts = csv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                var t = parts[i].Trim();
                if (t.Length > 0 && MatchesPreserveToken(name, t))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Prefix tokens ending in '/' stay prefix matches.
        /// Two-letter tokens (FT) require a non-letter/digit boundary so
        /// <c>blink_left</c> / <c>Gift</c> are not kept.
        /// Longer tokens stay substring (eye, VRCEmote, GoGo).
        /// </summary>
        public static bool MatchesPreserveToken(string name, string token)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(token)) return false;
            if (token[token.Length - 1] == '/')
            {
                if (name.StartsWith(token, StringComparison.OrdinalIgnoreCase))
                    return true;
                var inner = token.TrimEnd('/');
                return inner.Length > 0 &&
                       name.IndexOf("/" + inner, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (token.Length <= 2)
                return HasTokenBoundary(name, token);
            return name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool HasTokenBoundary(string name, string token)
        {
            int start = 0;
            while (start <= name.Length - token.Length)
            {
                int at = name.IndexOf(token, start, StringComparison.OrdinalIgnoreCase);
                if (at < 0) return false;
                bool leftOk = at == 0 || !IsNameTokenChar(name[at - 1]);
                int end = at + token.Length;
                bool rightOk = end >= name.Length || !IsNameTokenChar(name[end]);
                if (leftOk && rightOk) return true;
                start = at + 1;
            }

            return false;
        }

        static bool IsNameTokenChar(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
        }

        public static bool KeepObjectName(string name, OvaSettings settings)
        {
            return settings != null && MatchesSubstring(name, settings.preserveNameSubstrings);
        }

        static readonly string[] VisemeTokens =
        {
            "sil", "pp", "ff", "th", "dd", "kk", "ch", "ss", "nn", "rr",
            "aa", "e", "ee", "ih", "oh", "ou"
        };

        public static bool IsVisemeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;
            if (name.StartsWith("vrc.", StringComparison.Ordinal)) return true;
            if (name.StartsWith("v_", StringComparison.Ordinal) && IsVisemeToken(name.Substring(2)))
                return true;
            return IsVisemeToken(name);
        }

        static bool IsVisemeToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            for (int i = 0; i < VisemeTokens.Length; i++)
            {
                if (string.Equals(token, VisemeTokens[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool IsReservedShape(string name, bool preserveMmd)
        {
            if (string.IsNullOrEmpty(name)) return true;
            if (IsVisemeName(name)) return true;
            if (!preserveMmd) return false;
            for (int i = 0; i < MmdShapeHints.Length; i++)
            {
                if (name == MmdShapeHints[i]) return true;
            }

            return false;
        }

        public static bool KeepBlendShape(string name, OvaSettings settings, bool onVisemeMesh)
        {
            return KeepBlendShape(name, settings, onVisemeMesh, null);
        }

        public static bool KeepBlendShape(string name, OvaSettings settings, bool onVisemeMesh, HashSet<string> visemeSlots)
        {
            if (visemeSlots != null && visemeSlots.Contains(name)) return true;
            if (IsReservedShape(name, settings != null && settings.preserveMmd)) return true;
            if (onVisemeMesh && IsVisemeName(name)) return true;
            return KeepObjectName(name, settings);
        }

        public static HashSet<string> VisemeSlotNames(VRCAvatarDescriptor descriptor)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (descriptor == null || descriptor.VisemeBlendShapes == null) return set;
            var slots = descriptor.VisemeBlendShapes;
            for (int i = 0; i < slots.Length; i++)
            {
                if (!string.IsNullOrEmpty(slots[i]))
                    set.Add(slots[i]);
            }

            return set;
        }

        public static HashSet<Transform> BuildStructural(Transform root, OvaSettings settings)
        {
            var preserved = new HashSet<Transform>();
            if (root == null) return preserved;
            preserved.Add(root);

            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == "Armature")
                    preserved.Add(child);
            }

            var animators = root.GetComponentsInChildren<Animator>(true);
            for (int a = 0; a < animators.Length; a++)
            {
                var animator = animators[a];
                if (animator.avatar == null || !animator.avatar.isHuman)
                    continue;

                var animatorBoneNames = new HashSet<string>(StringComparer.Ordinal);
                for (var bone = HumanBodyBones.Hips; bone < HumanBodyBones.LastBone; bone++)
                {
                    Transform t = null;
                    try { t = animator.GetBoneTransform(bone); }
                    catch { t = null; }
                    if (t == null) continue;
                    animatorBoneNames.Add(t.name);
                    var cur = t;
                    while (cur != null && cur != root)
                    {
                        preserved.Add(cur);
                        cur = cur.parent;
                    }
                }

                AddHumanDescriptionFallback(animator, root, preserved, animatorBoneNames);
            }

            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            if (descriptor != null)
            {
                if (descriptor.lipSyncJawBone != null)
                    preserved.Add(descriptor.lipSyncJawBone);
                var eye = descriptor.customEyeLookSettings;
                if (eye.leftEye != null) preserved.Add(eye.leftEye);
                if (eye.rightEye != null) preserved.Add(eye.rightEye);
            }

            if (settings != null && settings.preserveMmd)
            {
                var body = MmdBodyAnchor(root);
                if (body != null)
                    preserved.Add(body);
            }

            return preserved;
        }

        public static bool KeepTransform(Transform t, Transform root, HashSet<Transform> structural, OvaSettings settings)
        {
            if (t == null || t == root) return true;
            if (structural != null && structural.Contains(t)) return true;
            return KeepObjectName(t.name, settings);
        }

        public static void CollectDynamics(GameObject root, HashSet<string> physPrefixes, HashSet<string> contactParams)
        {
            if (root == null) return;
            var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                var mb = behaviours[i];
                if (mb == null) continue;
                var typeName = mb.GetType().Name;
                if (typeName == "VRCPhysBone")
                {
                    var p = ReadStringMember(mb, "parameter");
                    if (!string.IsNullOrEmpty(p))
                        physPrefixes.Add(p);
                }
                else if (typeName == "VRCContactReceiver")
                {
                    var p = ReadStringMember(mb, "parameter");
                    if (!string.IsNullOrEmpty(p))
                        contactParams.Add(p);
                }
            }
        }

        public static bool IsPhysBoneName(string name, HashSet<string> prefixes)
        {
            if (string.IsNullOrEmpty(name) || prefixes == null || prefixes.Count == 0)
                return false;
            foreach (var prefix in prefixes)
            {
                if (string.IsNullOrEmpty(prefix)) continue;
                if (string.Equals(name, prefix, StringComparison.Ordinal))
                    return true;
                if (name.StartsWith(prefix + "_", StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        static string ReadStringMember(object obj, string member)
        {
            var t = obj.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var field = t.GetField(member, flags);
            if (field != null && field.FieldType == typeof(string))
                return field.GetValue(obj) as string;
            var prop = t.GetProperty(member, flags);
            if (prop != null && prop.PropertyType == typeof(string) && prop.GetIndexParameters().Length == 0)
                return prop.GetValue(obj, null) as string;
            return null;
        }

        static void AddHumanDescriptionFallback(
            Animator animator,
            Transform root,
            HashSet<Transform> preserved,
            HashSet<string> animatorBoneNames)
        {
            if (animator.avatar == null) return;
            var human = animator.avatar.humanDescription.human;
            if (human == null || human.Length == 0) return;
            var candidates = animator.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < human.Length; i++)
            {
                var mapped = human[i].boneName;
                if (string.IsNullOrEmpty(mapped)) continue;
                // GetBoneTransform already pinned the real Chest / Hips / … .
                // A second object with the same name (collider "Chest") must not
                // keep its whole Dynamics branch readable.
                if (animatorBoneNames != null && animatorBoneNames.Contains(mapped))
                    continue;
                for (int c = 0; c < candidates.Length; c++)
                {
                    if (candidates[c].name != mapped) continue;
                    var cur = candidates[c];
                    while (cur != null && cur != root)
                    {
                        preserved.Add(cur);
                        cur = cur.parent;
                    }
                }
            }
        }

        /// <summary>
        /// MMD worlds look up a GameObject named Body. That is the viseme
        /// SkinnedMeshRenderer on the clone, not every object that used to be
        /// named Body, and not Dynamics.
        /// </summary>
        public static Transform MmdBodyAnchor(Transform root)
        {
            if (root == null) return null;
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null || descriptor.VisemeSkinnedMesh == null)
                return null;
            return descriptor.VisemeSkinnedMesh.transform;
        }
    }
}
#endif
