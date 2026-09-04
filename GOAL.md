# GOAL — OVA

Locked 2026-09-03 (identity) and 2026-09-03 evening (v0.2 product: ova-web + engine).

## Identity

| | |
|---|---|
| Disk | `D:\Project\Obfuscatevrcavatar` |
| Short | OVA |
| VPM id | `dev.ova.protection` **0.3.0-pre.1** (first public pre) |
| Local console | **ova-web** (localhost UI; Unity inspector is fallback only) |
| UI | English catalog + zh-CN; Normal / Expert; AI pack |
| Unity | 2022.3 LTS (same as VRChat avatars) |

## Mission

Author-layer protection that is **painful to reuse after a rip**, not a promise that memory is empty.

Watchers must hold plaintext geometry to draw the avatar (il2cpp-re playbook §0). SaoMoLa proves cache-key / harvest / GPU classes exist. OVA does **not** copy that code. Success = unpacked Unity project is labor, plus watermark for attribution.

## What “strong” means here (analysis, not marketing)

| Do | Why it is strong | Do not |
|---|---|---|
| L1 compose AAO+TTT | Split clothes/textures is what makes rips cheap | Reimplement atlas |
| L2 full name layer (hierarchy, blendshapes, animator/VRC params, cloned mat/tex/audio, layer/state names) | Matches AvatarObfuscator 0.4.9 labor; VPM + ova-web is our edge | Paste their 20k lines |
| L7 keyed vertex watermark after merge | Honest attribution; playbook §5 | LSB on textures (VRC compresses them away) |
| L3 **compose** ShellProtector 2.6.3 (lilToon through 2.3.4) | They already maintain lilToon decrypt shaders | Unofficial lilToon vertex lock (hedgehog, 32 bit, no official Kanna list) |
| ova-web + JSON settings | Nested features, param picker, GitHub/Gitee fingerprint attest | Putting decrypt keys on a public gist |
| AES for **our** settings/watermark key at rest | Toolchain crypto is allowed | AssetBundle / viewer cache / GameAssembly keys |

Kaguya stay-out: freeze still forbids installing OVA there. Test on a **throwaway** 2022.3 avatar. GoGo / FT / OSC / `VrcDcc/` auto-preserved when param obfuscation is on.

## v0.2 (this window)

- [x] v0.1 name pass skeleton
- [x] JSON `OvaSettings` shared by NDMF and ova-web
- [x] Parameter pass with auto-detect preserve + Animator param curves + synced-layer behaviours
- [x] ova-web param picker (`pinPreserve` exact) + substring chips
- [x] Name-substring wired to Hierarchy/blendshape; param extraPreserve separate; PhysBone prefixes kept
- [x] ova-web overview live counts + pin chips
- [x] Cloned mat/tex/audio names
- [x] Animator layer/state name soup
- [x] Watermark pass (tiny keyed basis offset; upload survival unverified)
- [x] ova-web localhost nested UI (`PinProjectRoot` + reload Stop)
- [x] ova-web console UI (Material Web controls, custom line SVG, no Material Symbols)
- [x] ova-web EN catalog + zh-CN, Normal/Expert, AI pack zip (`ova-params-v1` round-trip)
- [x] GitHub/Gitee attest architecture (fingerprint, secrets.json, stub publish)
- [x] compile-check includes new Editor C#
- [x] Owner throwaway bake (not Kaguya) — NDMF ManualProcess on `test` Robot; human SDK Publish still open

## v0.3 tracks (2026-09-04)

Working titles. Red-team classes in, attack code out. Lost 10GB dump is not recovered.

- [x] **labor-clipref** — animation object-reference + PlayAudio clips follow cloned mat/tex/audio
- [x] **labor-meshname** — Mesh / AnimationClip / remaining tex names on the clone (2026-09-04)
- [ ] **attest-publish** — **parked 2026-09-04** (Owner: in-VRC remote verify is not a thing; do not wire Hop B). Local fingerprint UI may stay.
- [x] **throwaway-bake** — NDMF clone smoke on `D:\Project\Unity\test`. Human SDK Publish still the upload check.
- [x] **no-lock-fingerprint** — package has no `_BitKey` / `DexProtect` / `_IsLocked` strings (grep)

Details: [docs/BLUE-NEXT.md](docs/BLUE-NEXT.md).

## Explicit non-goals

- Perfect secrecy / “source code means nobody can harvest the drawn mesh”
- Shipping a ripper
- Putting OVA on Kaguya during freeze
- Kernel / physmem / cache decrypt
- Official-looking lilToon vertex unlock shader
