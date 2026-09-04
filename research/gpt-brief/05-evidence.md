# 证据与开放项（2026-09-04 独立审查）

完整英文审查：仓库 `.agent/REVIEW-2026-09-04-full.md`。ruling **blocked**（弧没关）。下面是给 GPT 的浓缩。不要把 HANDOFF 辩护当证据。

## 已重跑且成立

| 断言 | 证据 | status |
|---|---|---|
| compile-check 绿 | `scripts/compile-check.ps1` exit 0 | verified |
| 包内无锁指纹 | grep `_BitKey` `DexProtect` `_IsLocked` 于 `*.cs` 无匹配 | verified |
| wwwroot 内容 host=包=test | SHA256 9 个内容文件一致；test 多 Unity `.meta` | verified |
| test 包不是 junction | `LinkType` 空；UPM embedded copy | verified |
| 未装辉夜 | 无 `kaguya/Packages/dev.ova.protection` | verified |
| AfterPlugin MA/VF/AAO/TTT | `OvaProtectionPlugin.cs` | verified |
| NDMF clone visemeMiss=0 | `Temp/bake-w2.txt` `bake-w3.txt` | verified |
| AAO 2 SMR → 1 | 同上 + Editor.log「蒙皮网格数量: 2 → 1」 | verified |
| TTT 两张 Body 图 → 一张共用 2048 | bake dump cloneTex 同名同尺寸 | verified |
| decoy 8：23→31 | bake-w3 + `[OVA] decoy blendshapes added=8` | verified |
| FT 词边界：blink_left / Ear-Left 被汤 | dump `blink_left=False` `Ear-Left=False` | verified |
| SDK **预处理**跑过 OVA | Editor.log watermark→decoy→name→params→assets，栈在 `VRCSdkControlPanelAvatarBuilder` | verified |
| TTT Relocate 19 | Editor.log Atlas Relocate Result | verified |
| 厂商 1p URL 能打开 | AntiRip README/FAQ、Jinxxy DexProtect、Lumina | verified |

## 开放 / 失败（不要标 done）

| 断言 | 事实 | status |
|---|---|---|
| 人点 SDK **上传** + 作者拆包 | `emailOtp` Bad Request；无 `Building Avatar` / `.vrca` / `obft3st` | **open** |
| 水印过 VRC 压缩 | 仅 Editor 微扰日志 | **open** |
| listing zip = 工作树 | zip SHA 对得上 `source.json`，但 zip **没有** `HasTokenBoundary`，缺 `i18n.js`/`pack.js`。GitHub 初提交 **故意不收** `Releases/*.zip` | **open**（repair=重跑 pack-vpm） |
| MMD Body 硬留在 compose 克隆上 | dump `BodyGo=False`；合完后 viseme SMR 物体名是汤 | **gap** |
| ova-web「参数将改 0/8」 | bake / SDK 预处理 `renamed=2`。探针看场景，pass 看 NDMF 虚拟控制器 | **gap** |
| decoy 是整模一次 | 代码按 **每个** SMR 加 N 个；合完 1 网格所以 Robot 看起来像一次 | **gap** |
| SyncedLayer BlendTree | 代码有 `RewriteMotion`；Robot `clipCurves=0` | **unproven** |
| 分件衣服 | Robot 本来就近乎一件 Body | **unproven** |
| attest Hop B | 501，Owner parked | **parked** |

## Robot 不能当辉夜

- 没有真衣服分件、没有 GoGo 真包、没有面捕一整套、没有 256 bit 菜单。
- 证明的是：**顺序、口型槽、合网格/图、decoy、改名在 dummy 上跑得动**。
- 没证明：MMD 世界找 `Body`、PhysBone 抓取、SyncedLayer override、上传存活。

## 给想法的约束

新切片必须能在 **test Robot 或下一只抛掷模** 上证伪，且不要求装辉夜、不要求抄 SaoMoLa、不要求代理点 Publish。
