# Iteration goals — 2026-09-05

Owner: self-set goals, dispatch cursor-agent worktrees, do **not** rewrite the NDMF product identity.

Identity (GPT, pending GOAL lock): one-way **build compiler** + **local attribution**. Not a runtime unlock pack.

## This wave: `v0.3-console`

Two disjoint cursor-agent lanes. Parent (this Grok window) orchestrates and merges. No Kaguya, no SDK Publish, no SaoMoLa, no Hop B, no Shell/Ajisai, no watermark-v2.

| Lane | Worktree name | Owns | Does not own |
|---|---|---|---|
| **dash** | `ova-dash` | `host/ova-web/**`, package `Editor/Web/wwwroot/**` (copy), `docs/PLAIN.md`, `docs/OVA-WEB.md`, `README.md` intro | Editor `*.cs` except you may **read** them |
| **engine** | `ova-engine` | `Packages/dev.ova.protection/Editor/*.cs`, `Runtime/OvaSettings.cs` | `host/ova-web/**`, README |

Frozen `/api/last-build` JSON (`ova-build-report-v1`). Engine writes + GET. Dash consumes; **404 = stale / never baked**.

```json
{
  "ok": true,
  "schema": "ova-build-report-v1",
  "atUtc": "2026-09-04T00:00:00Z",
  "packageVersion": "0.2.0",
  "avatar": "Tutorial_Robot_Avatar_Dynamics_Demo_v1",
  "visemeMiss": 0,
  "origSmr": -1,
  "cloneSmr": -1,
  "decoyAdded": 8,
  "decoyBudget": 8,
  "bodyGo": true,
  "parameterRenamed": 2,
  "watermarkMeshes": 1,
  "watermarkStatus": "editor-only-unverified",
  "lockFingerprints": false
}
```

Unknown ints may be `-1` if the pass cannot see orig/clone SMR counts.

## Acceptance

- Dash: overview shows **场景估计** vs **上次克隆实测**; if 404, do not pretend bake numbers. Watermark never “可归因”. Vernacular PLAIN.md. zh-CN + en. Visual language unchanged (brass/paper). Port 17849.
- Engine: `decoyBlendShapeCount` is **avatar total** (default 8, max 32), append-only, visemeMiss stays 0. After name pass, if `preserveMmd`, exactly one Transform named `Body` = viseme SMR transform (or pinned later; no pin field this slice unless trivial). `compile-check.ps1` green. No `_BitKey` / `DexProtect` / `_IsLocked`.
- Neither lane: git push, git commit unless asked, Kaguya, Publish, OSC unlock.

## Later waves (not now)

Fixture avatar, watermark upload calibration, pack-vpm zip parity, Shell compose, Ajisai quarantine, watermark-v2, attest Hop B.
