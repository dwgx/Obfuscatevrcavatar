# Vendor OSC / vertex locks — limits (evidence)

Blue-team notes. Not a ripper cookbook. Do not vendor DexProtect, Kanna, or
Ajisai into this repo. OVA still does **not** emit `_BitKey*` / `DexProtect` /
`_IsLocked`.

Fetched 2026-09-04. Grades: **1p** = vendor README or store; **playbook** =
`D:\Project\vrchat-il2cpp-re\output\p2_research\avatar_ripping_defense_playbook.md`;
**detector** = SaoMoLa public README protection table (classes only).

## Two paths (do not mix)

| Path | What the attacker has | OSC / shader lock |
|---|---|---|
| **A — disk Unity project** | Cache `.vrca` → AssetRipper-style unpack | **Often broken.** Mesh on disk is scrambled; no key in the project. Vendors advertise this. Local detector agrees after *platform* decrypt. |
| **B — already drawn** | Live `Mesh` / GPU | **Not stopped.** Viewer must hold plaintext to draw (playbook §0, `vrchat-il2cpp-re/output/p2_research/avatar_ripping_defense_playbook.md`). Post-decrypt harvest is **key-agnostic**. |

Theorem: **viewers see a correct body** and **unpack is garbage** cannot both be
true unless a restore key (synced bits / OSC / Shader uniforms) exists **on the
viewer**. That restore is the **author key**. NDMF cannot hide the platform
cache key in `GameAssembly`.

Owner wish “even an agent cannot restore”: Path A can be made painful. Path B
cannot be made impossible at the avatar layer.

## Author-key fingerprint

A **fingerprint** is a stable string or param name **in the uploaded bundle**
that a detector greps after (or without) platform decrypt. It is not the VRChat
cache AES key.

| Fingerprint | Vendor | Detector claim (**detector**) |
|---|---|---|
| `_BitKey0` … `_BitKey31` | AvaCrypt / Kanna-class 32 synced bits | “decrypt then mesh still garbage” |
| `DexProtect` | DexProtect | “decrypt then invisible” |
| `_IsLocked` | Kanna Protecc | “decrypt then still encrypted” |

OVA `no-lock-fingerprint`: grep clean 2026-09-04. Avoiding these strings does
**not** hide the platform key. It only avoids the cheap “this is a lock pack”
classifier.

## Kanna Protecc / AntiRip (**1p**)

Source: [PlagueVRC/AntiRip README](https://github.com/PlagueVRC/AntiRip) and
[FAQ](https://raw.githubusercontent.com/PlagueVRC/AntiRip/master/Readme/FAQ.md)
(fetched 2026-09-04).

They say they:

- Randomize vertices on disk; custom shader + **32 bit** Avatar 3.0 params un-randomize in game.
- Obfuscate objects / params / layers as extra labor.
- Need **shaders, animations, and all** fully shown or the wearer is **invisible**.
- Support Poiyomi, UTS, Sunao, XSToon, GTAvaToon. **No lilToon.**
- Need **Write Keys** into LocalAvatarData; delete old unencrypted uploads.
- Quest: **not supported** (custom shader). Quest twin without lock **negates** PC lock.
- FAQ: typical hotswap does not bypass; **specialized hotswap** “would only get the avatar working normally **in game; not in unity**.”
- FAQ: they **cannot** take a listing off a ripper store; rippers may still upload the **unusable** copy.
- How secure: “not foolproof”; dedicated mod hotswap is admitted.

Lumina ([论 VRChat 如何反制盗模](https://share.lumina.moe/posts/vrchat-anti-ripper/)):
AntiRip-class shader patch; VRChat users **disable shaders** for low trust →
**hedgehog**. Lumina calls that trade-off not worth it vs merge+atlas.

## DexProtect (**1p**)

Source: [Jinxxy product page](https://jinxxy.com/Dextro/DexProtect) (fetched
2026-09-04). Gumroad listing is the same product.

They say they:

- Editor scramble at upload + **OSC app on the wearer PC** to unlock.
- **No custom shader** required; optional UV/texture scramble.
- Need **19 bits** of Expression Parameter space; SDK 3.5.2+; Unity 2022.3.22f1.
- Visible only if viewers have **Custom Animations** enabled.
- Invisible for a few seconds to new observers; menu preview is fallback or empty.
- Fallback avatar is **unprotected** (no gestures/visemes).
- Keys live under `Documents/DexProtect` named by avatar ID; **keys can be shared**.
- Public uploads “only work for users who have both the OSC program and key.”
- Their own limit: “No system is perfect… **no guarantee** that your avatar will never be ripped… unlikely to get the original with all functionality, **but not impossible**.”

Marketing “highest level of security” is vendor copy, not a measurement.

## AjisaiFlow Anti-Ripping (weaker grade)

VPM `net.ajisaiflow.anti-ripping` **0.50.0** (2026-07-16) claims mesh/texture
encrypt + blendshape decoy + obfuscation, lilToon 2.3.2+. **Product copy vs zip
was not re-audited this session.** Treat as compose-only until a throwaway bake
proves Path A.

## SaoMoLa detector vs platform decrypt (**detector**)

Public README table: after **their cache decrypt**, AvaCrypt/DexProtect/Kanna
packs are still scrambled / invisible / encrypted. Unprotected packs “fully
usable.”

That is evidence that vendor locks **work on Path A** (Unity project from
cache). It is **not** evidence that Path B (live Mesh / GPU) fails. The same
README lists cache-key extract and (experimental) hotswap as separate features.

Do not copy SaoMoLa scripts into OVA. Classes only.

## What OVA should not copy

- Synced unlock bits / OSC wearer-PC unlock as “anti-SaoMoLa.”
- LilToon vertex-lock shaders (Kanna has no official lilToon).
- Fingerprint param names.
- Claiming Path B is closed.

Compose if Owner names it: ShellProtector (textures, lilToon through 2.3.4) or
Ajisai — still Path A labor, still Path B open.

## What is still worth doing here

Ranked, still avatar-layer, no new constitution:

1. Finish **human SDK upload + author-unpack** of `obft3st` (Path A for **OVA**,
   not Dex). Prove names/atlas/decoy on a real `.vrca`.
2. Keep **AAO + TTT** as the Lumina “精神攻击” layer (Path A labor without a key).
3. Optional **ShellProtector on throwaway only** if Owner wants Path A textures
   with official lilToon decrypt — compose, do not reimplement.
4. Watermark survival after VRC compress (Path B **attribution**, not a lock).
5. L0: Private avatar + private instance so the pack never enters a stranger’s
   cache.

Not this repo: kernel anti-scan, hiding `UnityPlayer` EncryptionKey, in-avatar
HTTP, Quest twin that undoes PC lock.
