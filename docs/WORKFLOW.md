# Workflow (public)

One mission per window. English on git. Chinese with the Owner.

## Do

- Product lives in `Packages/dev.ova.protection/`. Canonical UI is `host/ova-web/wwwroot/`; copy into the package when shipping.
- Verify with commands that ran: `scripts/compile-check.ps1`, ova-web preview, a throwaway bake on `D:\Project\Unity\test` — never Kaguya.
- Human clicks VRChat SDK Build & Publish.
- Commits name the change. No AI attribution. Control-plane files stay untracked: `CLAUDE.md`, `AGENTS.md`, `.agent/`.

## Do not

- Claim 100% anti-rip. Path B (live Mesh / GPU) stays open.
- Emit `_BitKey` / `DexProtect` / `_IsLocked`. Do not vendor Dex, Kanna, or Ajisai.
- Copy SaoMoLa harvest / decrypt / key-scan / drivers.
- Install this package into a frozen Kaguya tree.
- Use a GitHub PAT as a remote VPM zip host. Local `scripts/pack-vpm.ps1` or a GitHub Release asset is enough.

## Version

Git tags are the public chronology. `v0.3.0-pre.1` is the first public pre. A local `source.json` is generated; it is not the git snapshot.
