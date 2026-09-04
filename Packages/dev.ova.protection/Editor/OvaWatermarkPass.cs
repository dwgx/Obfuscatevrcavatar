#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ova.Editor
{
    /// <summary>
    /// Keyed micro-offset on mesh basis. Attribution, not encryption.
    /// Run after AAO merge when possible. Skinned and MeshFilter meshes.
    /// </summary>
    internal static class OvaWatermarkPass
    {
        public static void Run(BuildContext ctx)
        {
            var state = ctx.GetState<OvaBuildState>();
            if (!state.Enabled || state.Settings == null || state.Settings.watermark == null)
                return;
            if (!state.Settings.watermark.enabled)
                return;

            var amp = state.Settings.watermark.amplitude;
            if (amp <= 0f) amp = 0.00001f;
            var seed = state.Settings.seed;
            if (seed == 0)
                seed = unchecked((int)DateTime.UtcNow.Ticks);

            var seen = new Dictionary<Mesh, Mesh>();
            var n = 0;

            var smrs = ctx.AvatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < smrs.Length; i++)
            {
                var smr = smrs[i];
                var next = Stamp(ctx, smr.sharedMesh, seed, amp, seen);
                if (next == null || next == smr.sharedMesh) continue;
                smr.sharedMesh = next;
                n++;
            }

            var filters = ctx.AvatarRootObject.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; i++)
            {
                var mf = filters[i];
                var next = Stamp(ctx, mf.sharedMesh, seed, amp, seen);
                if (next == null || next == mf.sharedMesh) continue;
                mf.sharedMesh = next;
                n++;
            }

            state.WatermarkedMeshes = n;
            Debug.Log("[OVA] watermark pass: meshes=" + n + " amp=" + amp);
        }

        static Mesh Stamp(BuildContext ctx, Mesh src, int seed, float amp, Dictionary<Mesh, Mesh> seen)
        {
            if (src == null || src.vertexCount == 0) return src;
            Mesh existing;
            if (seen.TryGetValue(src, out existing))
                return existing;

            var clone = Object.Instantiate(src);
            clone.name = src.name;
            var verts = clone.vertices;
            for (int v = 0; v < verts.Length; v++)
            {
                var h = Hash(seed, v);
                var s = ((h & 1) == 0 ? 1f : -1f) * amp;
                verts[v].x += s;
                verts[v].y += ((h >> 1) & 1) == 0 ? amp : -amp;
                verts[v].z += ((h >> 2) & 1) == 0 ? amp : -amp;
            }

            clone.vertices = verts;
            clone.RecalculateBounds();
            ctx.AssetSaver.SaveAsset(clone);
            ObjectRegistry.RegisterReplacedObject(src, clone);
            seen[src] = clone;
            return clone;
        }

        static int Hash(int seed, int vertex)
        {
            unchecked
            {
                var x = (uint)(seed * 16777619) ^ (uint)vertex * 2166136261u;
                x ^= x >> 16;
                x *= 0x7feb352d;
                x ^= x >> 15;
                x *= 0x846ca68b;
                x ^= x >> 16;
                return (int)x;
            }
        }
    }
}
#endif
