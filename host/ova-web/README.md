# ova-web

Local console for OVA. Named by Owner 2026-09-03.

Unity Editor hosts `http://127.0.0.1:17849/` (`OvaWebServer`) and serves this `wwwroot`. Menu: **OVA → 打开 ova-web**. The Inspector button on `OVA Protection` does the same.

UI: [Material Web](https://github.com/material-components/material-web) controls, IBM Plex + Noto Sans SC, custom line SVG in `index.html` / `icons/ova-mark.svg`. No Material Symbols. Canonical tree is this folder; copy into `Packages/dev.ova.protection/Editor/Web/wwwroot` when shipping the VPM package.

Settings JSON: `Library/OVA/settings.json` (gitignored). NDMF Resolving reads it; if missing, the component’s embedded `OvaSettings` is used.

Attest (GitHub / Gitee): fingerprint registry metadata in settings, PAT in `Library/OVA/secrets.json`. Publish HTTP is not wired. See [docs/ATTEST.md](../../docs/ATTEST.md).

Scene probe: `GET /api/scene` (alias `/api/scene/parameters`) returns Animator / expression / PhysBone names plus Hierarchy and blendshape rename counts. Preview mode serves a sample avatar named `preview`. Last clone: `GET /api/last-build` (preview always 404 `{"error":"not found"}`; Unity engine lane owns a real report). Overview must not show scene counts as a bake.

Without Unity, preview:

```powershell
powershell -File host/ova-web/preview.ps1
```
