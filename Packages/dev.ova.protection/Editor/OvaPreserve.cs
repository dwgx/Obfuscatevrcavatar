using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Ova.Editor
{
    /// <summary>
    /// Animator / VRC parameter names that stay plaintext.
    /// Hierarchy substrings are <see cref="OvaSettings.preserveNameSubstrings"/> and do not apply here.
    /// </summary>
    internal static class OvaPreserve
    {
        static readonly string[] VrcReserved =
        {
            "IsLocal", "PreviewMode", "Viseme", "Voice", "GestureLeft", "GestureRight",
            "GestureLeftWeight", "GestureRightWeight", "AngularY", "VelocityX", "VelocityY",
            "VelocityZ", "VelocityMagnitude", "Upright", "Grounded", "Seated", "AFK",
            "TrackingType", "VRMode", "MuteSelf", "InStation", "Earmuffs", "IsOnFriendsList",
            "AvatarVersion", "ScaleModified", "ScaleFactor", "ScaleFactorInverse",
            "EyeHeightAsMeters", "EyeHeightAsPercent", "VRCEmote", "VRCFaceBlendH",
            "VRCFaceBlendV", "VRCViseme", "One", "Two"
        };

        static readonly string[] AutoHints =
        {
            "Go/", "GoGo", "FT", "VRCFT", "vrcft", "Tracking/", "OSC", "VrcDcc/",
            "FaceEmo", "VRCFaceTracking", "IsLocal"
        };

        public static HashSet<string> Build(OvaSettings settings, GameObject avatarRoot, HashSet<string> physPrefixes)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < VrcReserved.Length; i++)
                set.Add(VrcReserved[i]);

            AddSubstrings(set, settings.parameters != null ? settings.parameters.extraPreserve : null);
            AddExact(set, settings.parameters != null ? settings.parameters.pinPreserve : null);

            if (physPrefixes != null)
            {
                foreach (var p in physPrefixes)
                {
                    if (!string.IsNullOrEmpty(p))
                        set.Add(p);
                }
            }

            if (settings.autoDetectPreserve)
            {
                for (int i = 0; i < AutoHints.Length; i++)
                    AddSubstringToken(set, AutoHints[i]);
                ScanAnimatorNames(set, avatarRoot, settings, physPrefixes);
            }

            return set;
        }

        public static bool ShouldKeep(string name, HashSet<string> exact, OvaSettings settings, HashSet<string> physPrefixes)
        {
            if (string.IsNullOrEmpty(name)) return true;
            if (exact != null && exact.Contains(name)) return true;
            if (HasExact(name, settings.parameters != null ? settings.parameters.pinPreserve : null)) return true;
            if (IsReservedPrefix(name)) return true;
            if (OvaNameKeep.IsPhysBoneName(name, physPrefixes)) return true;
            if (ContainsAny(name, settings.parameters != null ? settings.parameters.extraPreserve : null)) return true;
            if (!settings.autoDetectPreserve) return false;
            for (int i = 0; i < AutoHints.Length; i++)
            {
                if (OvaNameKeep.MatchesPreserveToken(name, AutoHints[i]))
                    return true;
            }

            return false;
        }

        public static void Classify(string name, OvaSettings settings, HashSet<string> physPrefixes, out string keep, out string reason)
        {
            keep = "none";
            reason = "";
            if (string.IsNullOrEmpty(name))
            {
                keep = "reserved";
                reason = "empty";
                return;
            }

            if (IsVrcReserved(name))
            {
                keep = "reserved";
                reason = "vrc";
                return;
            }

            if (IsReservedPrefix(name))
            {
                keep = "reserved";
                reason = "prefix";
                return;
            }

            if (HasExact(name, settings.parameters != null ? settings.parameters.pinPreserve : null))
            {
                keep = "pin";
                reason = "picker";
                return;
            }

            if (OvaNameKeep.IsPhysBoneName(name, physPrefixes))
            {
                keep = "physbone";
                reason = "physbone";
                return;
            }

            if (ContainsAny(name, settings.parameters != null ? settings.parameters.extraPreserve : null))
            {
                keep = "extra";
                reason = "substring";
                return;
            }

            if (settings.autoDetectPreserve)
            {
                for (int i = 0; i < AutoHints.Length; i++)
                {
                    if (OvaNameKeep.MatchesPreserveToken(name, AutoHints[i]))
                    {
                        keep = "auto";
                        reason = AutoHints[i];
                        return;
                    }
                }
            }
        }

        static bool IsVrcReserved(string name)
        {
            for (int i = 0; i < VrcReserved.Length; i++)
            {
                if (string.Equals(name, VrcReserved[i], StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        static bool HasExact(string name, string csv)
        {
            if (string.IsNullOrEmpty(csv)) return false;
            var parts = csv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.Equals(parts[i].Trim(), name, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        static void AddExact(HashSet<string> set, string csv)
        {
            if (string.IsNullOrEmpty(csv)) return;
            var parts = csv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                var t = parts[i].Trim();
                if (t.Length > 0) set.Add(t);
            }
        }

        static bool IsReservedPrefix(string name)
        {
            return name.StartsWith("Go/", StringComparison.Ordinal)
                   || name.StartsWith("VrcDcc/", StringComparison.Ordinal);
        }

        static void AddSubstrings(HashSet<string> set, string csv)
        {
            if (string.IsNullOrEmpty(csv)) return;
            var parts = csv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                var t = parts[i].Trim();
                if (t.Length > 0) set.Add(t);
            }
        }

        static void AddSubstringToken(HashSet<string> set, string token)
        {
            if (!string.IsNullOrEmpty(token)) set.Add(token);
        }

        static bool ContainsAny(string name, string csv)
        {
            if (string.IsNullOrEmpty(csv)) return false;
            var parts = csv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                var t = parts[i].Trim();
                if (t.Length > 0 && name.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        static void ScanAnimatorNames(HashSet<string> set, GameObject root, OvaSettings settings, HashSet<string> physPrefixes)
        {
            if (root == null) return;
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            if (descriptor != null && descriptor.expressionParameters != null)
            {
                var list = descriptor.expressionParameters.parameters;
                if (list != null)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        var n = list[i].name;
                        if (ShouldKeep(n, set, settings, physPrefixes))
                            set.Add(n);
                    }
                }
            }
        }
    }
}
