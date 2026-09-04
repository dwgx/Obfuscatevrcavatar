# Library refresh — 2026-09-03 (OVA window)

Primary sources fetched this session. Not blogs. Do not vendor their code.

## Pipeline we sit on

| Piece | Current public | Kaguya (read-only) | OVA |
|---|---|---|---|
| NDMF | **1.14.8** (2026-08-29) | MA 1.18.5 locks `nadena.dev.ndmf >=1.14.7 <2.0.0-a` | `package.json` still `>=1.6.0` |
| AnimatorServicesContext | Tracks Transform renames while active; VirtualClip paths are snapshot/virtual ([docs](https://ndmf.nadena.dev/api/nadena.dev.ndmf.animator.AnimatorServicesContext.html)) | used by MA/FaceEmo | v0.1 `WithRequiredExtension` |
| VRC Avatars SDK | — | **3.10.4** | `>=3.5.0` |
| lilToon | Shell README claims **1.3.8–2.3.4** | **2.3.4** | no vertex lock |
| AAO / TTT | still the merge/atlas layer | **not installed** | AfterPlugin only |

## Name-layer peers

| Tool | Distro | Latest verified | Does | Do not copy |
|---|---|---|---|---|
| [AvatarObfuscator](https://github.com/cocokoishi/AvatarObfuscator) | unitypackage (not VPM) | **v0.4.9** 2026-08-25 | NDMF clone-only homoglyph `ÌÍÎÏ`; hierarchy, blendshapes, animator+VRC params, cloned mat/tex/audio; seed `5145514` / `0`; Preserve MMD; substring exclusions default `FT,eye`. Passes: CollectState, Hierarchy, BlendShapes, Parameters, AnimationClips, SharedAssets, FinalizeAssets. `AutoMergeSkinnedMeshPass` / `RemapUVTexturePass` are tiny leftover stubs; README says use TTT. | 20k-line pass files |
| [Esska AV3Obfuscator](https://github.com/Ess-Ka/EsskaAV3Obfuscator) | VPM listing `ess-ka.github.io/EsskaPackageListing` | README V2.0; listing **archived** in prior OVA note | Hierarchy, controllers/layers/states, optional expressions/menus/params/meshes/blendshapes/mat/tex/audio. **Destructive copy** into `Assets/Obfuscated` (not NDMF clone-only). Preserve MMD. | scene mutation model |
| Kanna Protecc [AntiRip](https://github.com/PlagueVRC/AntiRip) | GitHub zip into Assets | README still current | Vertex scramble + 32 synced bits + whitelist shaders (Poiyomi, UTS, Sunao, XSToon, GTAvaToon). **No lilToon.** Obfuscates objects/params/layers with Auto Detect for GoGo/FT. Write Keys. Not hotswap-immune (their README). | shaders, Write Keys, 32-bit key |

## Texture / mesh lock peers (compose, do not reimplement)

| Tool | Latest verified | Notes for OVA |
|---|---|---|
| [ShellProtector](https://github.com/Shell4026/ShellProtector) | **2.6.3** 2026-07-14 + OSC 1.6.2 | VPM `shell4026.github.io/VCC/`. MainTex XXTEA/ChaCha8. lilToon **through 2.3.4**. Multiplex OSC ~11–13 bits. Fallback 16×16. **BC7 unsupported.** Not on Kaguya. |
| [AjisaiFlow Anti-Ripping](https://github.com/lighfu/vpm/releases/tag/anti-ripping-v0.50.0) | **0.50.0** 2026-07-16, 64 MB zip | VPM `net.ajisaiflow.anti-ripping`. Claims mesh/texture encrypt + blendshape decoy + obfuscation. Product copy vs zip not re-audited this session. |
| DexProtect | Gumroad (closed) | OSC unlock, hierarchy rename, mesh scramble, **no custom shader**. Needs ~19 spare bits. |
| [KSUnityTools Obfuscator](https://github.com/kawaiistudio/KSUnityTools) | git UPM | Vertex scramble + shader encrypt + GUID names. Not NDMF-first. |
| [GTAvaCrypt](https://github.com/rygo6/GTAvaCrypt) | ancestor of Kanna | Poiyomi inject. Shell fork existed for lilToon/UTS; **not official lilToon for Kanna**. |

## Red-team classes (local, names only)

SaoMoLa / il2cpp-re playbook: cache extract, post-decrypt harvest, GPU capture, heap mesh, bundle-key intercept. Author NDMF cannot touch viewer `GameAssembly` / `UnityPlayer` keys. Fingerprints claimed locally: `_BitKey0..31`, `DexProtect`, `_IsLocked`. L1 merge+atlas is not on that list.

## OVA v0.1 gaps vs AO 0.4.9 (code read this window)

Implemented: Optimizing after MA/VF/AAO/TTT; homoglyph GO names; cloned blendshapes; VirtualClip `blendShape.*` rewrite; Humanoid+Armature+eye/jaw+optional Body; `seed=0` random.

Dead / unfinished in C#: `preserveNameSubstrings` never copied into `OvaBuildState`; `PathRenames` never filled. No parameter pass, no cloned mat/tex/audio names, no inspector, no NDMF ErrorReport, no bake test in a throwaway project.

Unverified: whether `RewriteBlendShapeCurves` keys (`Rel` before rename) still match `VirtualClip` bindings after `ObjectPathRemapper` sees Transform renames. Must prove on a throwaway bake, not Kaguya.

## Kaguya (constraints, not a testbed)

Freeze file: do not install OVA / AAO / TTT / Shell into Kaguya this window. Stack: MA 1.18.5, VF 1.1345.0, lilToon 2.3.4, FaceEmo 1.7.0 (package present, not hung), GoGo All. Bake was 256/256 historically → no 32-bit vertex lock, Shell bits need a diet first. Preserve names if we ever obfuscate params: `Go/`, `VRCEmote`, FaceEmo, FT/OSC, `VrcDcc/`.
