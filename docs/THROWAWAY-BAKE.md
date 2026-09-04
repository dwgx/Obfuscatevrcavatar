# Throwaway bake plan (not Kaguya)

Owner will create an empty Unity 2022.3 avatar project. Agent does not click SDK Publish. Do not install OVA into `D:\Project\Unity\kaguya`.

Paper sandbox below is **code-trace**, not a bake. Three hedge subagents were started 2026-09-03 to cross-check; this file is the human checklist.

## 0. Create the project

1. Unity Hub → 2022.3.22f1 (same as Kaguya) → 3D Core or VRChat Avatars template.
2. Folder suggestion: `D:\Project\Unity\ova-throwaway` (throwaway, not Kaguya).  
   **2026-09-04:** Owner already has `D:\Project\Unity\test` (VCC: SDK 3.10.4, MA 1.18.7, NDMF 1.14.8, lilToon 2.3.4). Use that. Do not wait for Unity MCP (DCC window still owns it).
3. VRChat SDK Avatars via VCC (same 3.10.x as Kaguya is fine).
4. VCC/UPM: NDMF (MA pulls it). Optional later: AAO + TTT + GoGo.
5. Unity Package Manager → Add package from disk →  
   `D:\Project\Obfuscatevrcavatar\Packages\dev.ova.protection\package.json`
6. Make a tiny SDK avatar: Humanoid Armature, Body with `vrc.v_aa` + `Blink` + `あ`, one extra mesh `Clothes_Skirt`, one Bool `MyToggle`, GoGo **or** a dummy param `Go/Locomotion`.
7. Avatar root: Add Component **OVA Protection**.
8. Menu **OVA → 打开 ova-web**. Browser `http://127.0.0.1:17849/`. Save. If bind fails, note the exception (URL ACL).

## 1. Console must-see (NDMF Preview or SDK Build)

| Log | Meaning |
|---|---|
| `[OVA] name pass:` | hierarchy/blendshape/layer ran |
| `[OVA] parameter pass: renamed=` | params ran; `renamed=0` with GoGo-only avatar may still be OK |
| `[OVA] watermark pass: meshes=` | basis offset ran |
| `[OVA] asset name pass:` | cloned mat/tex names |
| `[OVA] no OvaProtection` | component missing or JSON all-off |

Source scene Hierarchy must **stay readable**. Soup names exist only on the **build clone**.

## 2. Pass / fail (clone, not scene)

| Check | Pass |
|---|---|
| Humanoid | Upload/SDK does not complain missing `Hips` path |
| Armature | Child still named `Armature` |
| Body | Still `Body` if Preserve MMD on |
| Visemes | `vrc.*` unchanged |
| `Go/Locomotion`, `VRCEmote` | Still readable on clone |
| `MyToggle` | Soup `ÌÍÎÏ…` |
| `Clothes_Skirt` | Soup |
| Face Blink / あ | Readable if MMD on |
| `Breast_small` | Soup |
| ova-web Save | `Library/OVA/settings.json` updates; `/api/health` 能开不算 Save 成功 |
| Clip with `blendShape.Breast_small` | Still drives the soup-named shape (NDMF 主路径：ASC 关掉才 RewritePaths) |
| Clip that animates an **Animator parameter** (`MyToggle` as float curve) | **Expect break until we rewrite those property names** |
| FX synced-layer motion override | **Expect missed param/shape rewrite** if the avatar has one |
| Dummy param `Gift` or `Softness` | Likely **left readable** (`FT` substring over-preserve) |
| PhysBone field `Hair` vs param `Hair_IsGrabbed` | **Prefix not rewritten** — grab/pose may break; needs Owner policy |
| Expression ParameterDriver nested `.name` | Was `NextVisible` miss; now walks nested strings — bake to confirm |

Toggle **parameters.obfuscate off** in ova-web, bake again: `MyToggle` readable. Toggle watermark off: vertex positions match (spot-check one mesh).

## 3. Later overlays (same throwaway, still not Kaguya)

1. Add AAO Merge Skinned Mesh + TTT atlas → bake → clothes should not be separate extractable pieces. OVA still last (AfterPlugin).
2. Optional ShellProtector 2.6.3 on **one** MainTex (lilToon 2.3.4). Bit budget: dummy avatar only.
3. Owner may point local red-team at **this** upload, not Kaguya. Agent does not copy SaoMoLa.

## 4. Paper sandbox (defaults: all layers on, seed 5145514)

Fake tree: `Root` / `Armature/Hips/.../LeftEye` / `Body` / `Body_b` / `Clothes_Skirt` / dummy `Go/Locomotion` + `VRCEmote` + `MyToggle` + `FT_Blink` + PhysBone `Hair_Spring`.

| Thing | Predicted clone | Confidence |
|---|---|---|
| Root, Armature, Humanoid bones, eyes | readable | certain-from-code (`BuildPreserveSet`) |
| `Body` | readable (name fallback if &lt;4 MMD shapes) | certain-from-code |
| `Clothes_Skirt` | soup | certain-from-code |
| `vrc.v_aa`, `Blink`, `あ` | readable | certain-from-code |
| `Breast_small` | soup | certain-from-code |
| `Go/Locomotion`, `VRCEmote`, `FT_Blink` | readable (`Go/`, reserved, `FT`) | certain-from-code |
| `MyToggle` | soup | certain-from-code |
| `Hair_Spring` if only on PhysBone, not in Animator | **stays readable** (not collected) | certain-from-code (gap) |
| `GoGo` GameObject name | soup（参数仍可读） | certain-from-code；真包是否按物体名找 = bake |
| Blendshape curves still animate | VirtualClip 在 ASC 期间仍是虚拟 path；ShapeKey 主路径 | code-safe；bake 仍要看一眼 |
| Animator 参数曲线 / SyncedLayer override | 当前 pass 不改 | **likely-broken**（有这类 clip 才爆） |
| Watermark then shape-clone | basis offset should survive second Instantiate | certain-from-code; magnitude needs eye |
| ova-web Save | `JsonUtility` 后台可用；原先请求里摸 `Application.dataPath` 会炸。已主线程 `PinProjectRoot` | 仍要点一次 Save 看 json |

## 5. Do not

- Install into Kaguya.
- Claim 100% anti-rip if bake looks ugly.
- Point SDK Publish from the agent.

## 6. MCP-ready (2026-09-04)

Playbook: `.agent/MCP-READY.md`. `test` has `Packages/dev.ova.protection` **0.2.0** (VPM copy, not a junction — product edits must be copied into that folder).
Dummy: SDK Robot Avatar PC sample saved as `Assets/OVA/RobotBake.unity`.
F: is `F:\UnityVRChat` — do not harvest Kaguya/Rurune clothes into this throwaway.

This home Cursor chat has **no** `unityMCP` namespace. Bake used CoplayDev HTTP at `http://127.0.0.1:8080/mcp` via `Temp/mcp-call.py`. Do not put Unity MCP in user-global `mcp.json`. Do not `install-vrc-dcc-tools.ps1` on `test`.

## 7. Bake evidence (2026-09-04, SDK Robot Avatar PC)

NDMF `AvatarProcessor.ManualProcessAvatar` on root `Tutorial_Robot_Avatar_Dynamics_Demo_v1` (instance 43744). Source Hierarchy stayed `Armature` / `Body` / `Dynamics`. Agent did not SDK Publish.

| Check | First bake | After viseme + humanoid-fallback fix |
|---|---|---|
| Console | watermark meshes=1 amp=1e-5; name pass; parameter renamed=2 preserved-hints=48; asset clones=4 clipCurves=0 behaviourAudio=1 | same, name pass `blendshapes=6` (was 21) |
| `OvaProtection` | on source, gone on clone (`IEditorOnly`) | same |
| Watermark | clone v0 delta ≈ 1e-5/axis; different mesh asset | same |
| Visemes | **fail**: mesh souped `v_aa`…; descriptor still `v_sil`… (lip sync by name would miss) | **pass**: 15/15 slots match mesh (`miss=0`), `v_aa` index 19 both sides |
| Armature / Body / Humanoid | kept | kept |
| `Dynamics` folder | kept (duplicate Humanoid name `Chest` on a collider pulled ancestors) | souped |
| `Ear-Left` | kept | still kept (`FT` substring vs `left`) |
| `blink_left` / `lowerlid_left` | kept | still kept (same `FT` in `left`) |
| Mesh / texture **asset** names | `Body`, `puff`, `Tutorial_Robot_…_BASE` readable | same — **labor-meshname** still open |
| Material names | souped | souped |

Open product forks (ask Owner, do not guess): `preserveNameSubstrings` includes `FT` with case-insensitive IndexOf, so any `*left` hierarchy/shape stays plaintext. Known Gift over-preserve is the same class.

## 8. Iteration bake (2026-09-04, same Robot, after labor-meshname + FT token)

Author-unpack loop: [RED-BLUE.md](RED-BLUE.md). NDMF clone is the red team (no author key).

| Check | Result |
|---|---|
| Console | watermark meshes=1; name pass blendshapes=**8**; parameter renamed=2; asset clones=**8** meshes=1 clipNames=6 clipCurves=0 behaviourAudio=1 |
| Visemes | still 15/15 `miss=0`, `v_aa` kept |
| `blink_left` / `Ear-Left` | **souped** (`FT` token-boundary; `Gift` would too) |
| Mesh asset | clone `Ï…` / source still `Body` |
| Textures | clone soup (`CopySerialized` Texture2D); source files untouched |
| GameObject `Body` / Armature | kept (MMD / Humanoid) |
| Source Hierarchy | `Armature Body Dynamics` |
| Watermark | v0 delta ≈ 1e-5 |

Still cheap after this dummy unpack: Robot is one Body (merge is order proof, not split clothes). Humanoid/viseme/Body **names** kept on purpose. TTT now shares one atlas tex on the two clone mats (Wave 2c). Upload watermark survival untested.

## 9. Wave 2 compose bake (2026-09-04, batchmode on `test`)

Packages (test only, not Kaguya): AAO **1.9.18**, TTT **1.0.2**, tex-trans-core **0.2.0**, mathematics **1.3.2**.
Identity: `D:/Project/Unity/test/Assets`. Driver: `Assets/OVA/Editor/OvaThrowawayBake.cs` (`Wave2And3`).
First passes (`ova-batch-w2.log` / `w2b`): TTT `delayCall` init skipped → `ColorFill` NRE.
Fix: call `TTTInitializeCaller.Initialize()` before NDMF. Log `ova-batch-w2c.log`. Dump `Temp/bake-w2.txt`. Kaguya stayed on `D:\Project\Unity\kaguya`. No SDK Publish.

Scene children: `OVA_AAO_Merge` + `OVA_TTT_Atlas`. `tttInit=UStdHolder ColorFill=True`. `tttColorFillNre=False`.

| Check | Result |
|---|---|
| visemeMiss | **0** |
| SMRs | orig **2** → clone **1** (AAO merge) |
| TTT atlas | orig `BASE` + `BODY` (two 2048 tex) → clone both mats share one souped 2048 tex |
| OVA after AAO/TTT | watermark → name (`decoy=0`) → params → assets |
| Source Hierarchy | `Armature Body Dynamics OVA_AAO_Merge OVA_TTT_Atlas` readable |

## 10. Wave 3 decoy bake (same batch, `decoyBlendShapeCount=8`)

Dump: `Temp/bake-w3.txt`. `Library/OVA/settings.json` left at 8.

| Check | Result |
|---|---|
| visemeMiss | **0** |
| shapes | Wave 2 **23** → Wave 3 **31** (+8) |
| Console | `[OVA] decoy blendshapes added=8` and `decoy=8` |
| Keep-list | decoys are souped names, not viseme / `FT` tokens |
