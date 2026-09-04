# OVA — Obfuscate VRChat Avatar

**中文默认。** English: [README.en.md](README.en.md). 日语以后再加。

先看白话：[docs/PLAIN.md](docs/PLAIN.md)。OVA 让拆包再用变贵，**不**把别人 GPU 里的网格藏起来，也**不**声称 100% 防盗。怎么点、怎么叠 AAO/TTT 写在那一页。

源码：[github.com/dwgx/Obfuscatevrcavatar](https://github.com/dwgx/Obfuscatevrcavatar)。当前公开标签 **v0.3.0-pre.1**。工作流：[docs/WORKFLOW.md](docs/WORKFLOW.md)。

这是 **蓝队** 头像保护工程：在 **你自己的** Unity 2022.3 头像上，用 NDMF 在上传克隆上做难读化 / 可选加密，让拆包二次利用变贵。  
**不是** 拆包器、不是扫 key、不是热替换别人的模型。红队分析在 `D:\Project\SaoMoLa`，本仓库只吸收 **威胁模型**，不迁攻击链。

官方也说了：游戏最终要在客户端画出网格，**100% 防不住**。私人实例仍然是唯一接近的办法。公开世界里我们只做「比改一个模更贵」。

## 目标（v0）

| 优先级 | 做什么 | 不做什么 |
|---|---|---|
| P0 | 工作区 + VPM 包骨架 + 威胁模型 + 中文文档 | 装进辉夜（改模已封存） |
| P1 | NDMF 构建时混淆名字（v0.1 已做；同类：[AvatarObfuscator v0.4.9](https://github.com/cocokoishi/AvatarObfuscator)） | 改场景里的源资源 |
| P2 | 和 TexTransTool / Avatar Optimizer 的配合说明（合网格、合图集） | 自己再写一套图集 |
| P3 | 研究 Kanna Protecc 级顶点锁（32 bit、要特定 Shader、别人关 Shader 会刺猬） | 现在不写 lilToon 破解版 |

## 两个导包概念（必须分清）

| | **VCC / VPM** | **.unitypackage** |
|---|---|---|
| 是什么 | VRChat Creator Companion 的包清单。VCC 按 `package.json` 把版本装进工程 `Packages/` | Unity 的资源包。拖进工程变成 `Assets/...` |
| 例子 | Modular Avatar、NDMF、本仓库的 `dev.ova.protection` | [AvatarObfuscator 0.4.9](https://github.com/cocokoishi/AvatarObfuscator/releases) |
| 怎么用 | VCC → 加仓库 URL 或本地 `source.json` → 在头像工程里 Install | Unity `Assets → Import Package` |
| 依赖 | `vpmDependencies` 自动拉 NDMF | 你自己保证 NDMF ≥ 1.6 |

本仓库 **主路径是 VPM**。以后可以再打一份 `.unitypackage` 给不会 VCC 的人。细节：[docs/VCC.md](docs/VCC.md)。

## 别人已经做的（研究，不抄袭当唯一方案）

- **cocokoishi AvatarObfuscator**：NDMF，同形异义字符改名（物体 / blendshape / 动画参数 / 材质克隆）。**不加密顶点**。贴图改去 TexTransTool。
- **Kanna Protecc (AntiRip)**：打乱顶点 + Shader 用 32bit 参数还原。别人必须开满 Shader。Poiyomi 等白名单。**没有官方 lilToon**。上传后还要 Write Keys。
- **ShellProtector**：贴图加密 + OSC 输密码。支持 lilToon。
- **AjisaiFlow Anti-Ripping**：VPM + NDMF，mesh/贴图加密。
- **Esska AV3Obfuscator**：VCC 包，改参数名/菜单/blendshape。
- Lumina：[论 VRChat 如何反制盗模](https://share.lumina.moe/posts/vrchat-anti-ripper/) — AssetBundle 没有安全性；合网格+合 UV 是精神攻击。

## 停线

- 不要把本包装进 `D:\Project\Unity\kaguya` 直到改模解冻。
- 不要从 SaoMoLa 复制提取 / 驱动 / 扫 key 代码。
- 不要点 VRChat SDK Publish。
- 不要声称 100% 防盗。
- 公开 git 用英文；和 Owner 说中文。

## v0.2 现状

`Packages/dev.ova.protection` **0.2.0**：NDMF Optimizing（MA / VRCFury / AAO / TTT 之后）做 **水印微扰 + 物体/blendshape/层/状态名 + 参数名（自动保留 GoGo/FT/OSC）+ 克隆 mat/tex/audio 名**。源场景不动。主 UI 是 **ova-web**（Material 3，`OVA → 打开 ova-web`，`127.0.0.1:17849`）。GitHub / Gitee **验证**只登记水印指纹，令牌留本机；远端发布还没接线。贴图加密不自写，叠 ShellProtector / TTT。顶点锁仍不做。说明：[docs/NAME-PASS.md](docs/NAME-PASS.md)、[docs/ATTEST.md](docs/ATTEST.md)、[host/ova-web/README.md](host/ova-web/README.md)。

头像根加组件 **OVA Protection**。不要装进封存中的辉夜。

蓝队上限（做得到 / 做不到）：[docs/CAPABILITY.md](docs/CAPABILITY.md)。

下一步：抛掷工程清单 [docs/THROWAWAY-BAKE.md](docs/THROWAWAY-BAKE.md)。不要用辉夜。
