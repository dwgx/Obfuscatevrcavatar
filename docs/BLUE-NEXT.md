# Blue-team next tracks (OVA)

Working titles. Owner may rename. Not a fifth constitution.

Red-team input is **classes only**: cache extract, post-decrypt harvest, GPU capture,
AssetRipper-style Unity projects, known lock fingerprints (`_BitKey0..31`,
`DexProtect`, `_IsLocked`). SaoMoLa / il2cpp-re attack code stays out of this
repo. The 10GB local package dump is gone; do not hunt it. Use the playbook,
public peers, and this tree.

## Tracks

| Id | Goal | Why (red-team cheap path) | Status |
|---|---|---|---|
| **labor-clipref** | Clone + soup **animation object-reference** curves (Material / Texture2D / AudioClip) and PlayAudio clip arrays | Unpack tools keep swap animations pointing at plaintext asset names if we only rename renderer slots | **landed** |
| **labor-meshname** | Clone + soup Mesh / AnimationClip **asset** names | Rip still shows shop mesh/clip filenames after clip-ref | **this slice** (2026-09-04) |
| **attest-publish** | GitHub/Gitee PUT/POST of public `ova-fp-v1` JSON | Attribution without synced decrypt keys (those are what scanners look for) | **parked 2026-09-04** (Owner) |
| **throwaway-bake** | `D:\Project\Unity\test`, human clicks SDK | Paper ≠ verified. Never Kaguya | **NDMF ManualProcess verified**. Human SDK Publish still open. Loop: [RED-BLUE.md](RED-BLUE.md) |
| **no-lock-fingerprint** | Never emit AvaCrypt/Kanna/DexProtect param names | Local detector claims those three strings | grep clean 2026-09-04 |

Compose, do not rewrite: AAO merge, TTT atlas, ShellProtector / Ajisai. No lilToon vertex lock.
Citations for why OSC locks do not close live Mesh/GPU: [VENDOR-OSC-LOCKS.md](VENDOR-OSC-LOCKS.md).

## This slice (labor-meshname + keep-list)

Mesh / clip / texture **asset** names on the clone. `FT` is a token, not an infix.
Synced-layer BlendTree blend params on overrides are rewritten in
`OvaAnimatorRewrite.MapParameters` (needs an avatar that actually has one).

Still open: upload watermark survival, AAO+TTT compose on `test`, human SDK
Publish then author-unpack.
