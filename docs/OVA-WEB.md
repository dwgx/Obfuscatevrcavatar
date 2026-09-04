# ova-web console

Canonical UI: `host/ova-web/wwwroot/`. Copy into
`Packages/dev.ova.protection/Editor/Web/wwwroot` (and the Unity `test`
copy) when shipping.

Visual: warm black `#12110f`, paper `#ece7dc`, brass `#d9a441`. No purple
AI chrome. No Material Symbols. English string catalog is the source;
`zh-CN` is vernacular translation. Overview uses a two-pane dash (avatar
+ fingerprint | seed / length / textures / attest), then **two honest
blocks**: Estimated from scene (`GET /api/scene`) vs Last NDMF clone
(`GET /api/last-build`). 404 or `{"error":"not found"}` shows “not baked
yet / report stale” (zh: 还没烤过 / 报告过期) — never scene `0/8` as a
bake. Watermark status label is always `editor-only-unverified`. The main
column fills the shell; long avatar names wrap. Vernacular intro:
[PLAIN.md](PLAIN.md).

## Modes

| `ui.mode` | What you see |
|---|---|
| `normal` | Overview, protection masters, watermark amplitude, AI pack |
| `expert` | Current nested UI + extra knobs (name length, keep chips, param picker, attest, raw JSON, settings-at-rest flag) |

Persisted in `OvaSettings.ui` (`locale`, `mode`, `projectNote`). NDMF ignores `ui`.

## AI pack (always English)

Export does **not** dump the Unity project. That is too big for a chat.

Zip `ova-pack-v1.zip`:

| File | Role |
|---|---|
| `00-README.txt` | How to answer |
| `01-BRIEF.md` | Short context + live scene counts |
| `02-settings.json` | Full `OvaSettings` (no PAT) |
| `03-scene.json` | `/api/scene` probe only |
| `04-fingerprint.txt` | `ova-fp-v1` hex |
| `05-CONSTRAINTS.md` | What the model may change |

Round-trip: the model replies with a fenced block:

````
```ova-params-v1
{ ...settings json... }
```
````

Paste into ova-web → Apply. Unknown keys are ignored. Do not invent
`_BitKey` / `DexProtect` / `_IsLocked`. Attest publish stays off unless
the human already configured a provider.

## Last-build report

Frozen JSON `ova-build-report-v1` (engine lane writes it in Unity). Dash
only **consumes** `GET /api/last-build`. Preview without Unity:

`GET /api/last-build` → **404** `{"error":"not found"}`.

Do not treat `/api/scene` decoy or layer counts as the last clone.
