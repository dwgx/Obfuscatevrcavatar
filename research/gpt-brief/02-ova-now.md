# OVA 现在是什么

## 身份

| | |
|---|---|
| 盘 | `D:\Project\Obfuscatevrcavatar` |
| 短名 | OVA |
| VPM id | `dev.ova.protection` **0.2.0**（工作树；本地 listing zip 落后） |
| 控制台 | ova-web `http://127.0.0.1:17849/` |
| Unity | 2022.3 LTS（抛掷与辉夜同号 2022.3.22f1） |
| Git | 私有 `dwgx/Obfuscatevrcavatar`，`main` `8a3d32c` 干净初提交 |
| 不是 | 辉夜改模、SaoMoLa、ripper |

## 仓库形状（源码仓，不是 Unity 工程）

```
Packages/dev.ova.protection/     真正跑的 NDMF 插件（Runtime + Editor）
host/ova-web/wwwroot/            UI 真源；shipping 拷进包内 Editor/Web/wwwroot
scripts/compile-check.ps1        csc，引用辉夜 HintPath，不装进辉夜
scripts/pack-vpm.ps1             打本地 VCC listing zip + 写 source.json
docs/                            威胁模型、能力、名字层、厂商锁
```

辉夜 `D:\Project\Unity\kaguya` 与抛掷 `D:\Project\Unity\test` **在仓外**。不要把它们推进 GitHub。

## 构建 / 安装

1. 写 C# 在 `Packages/dev.ova.protection/`。
2. `compile-check.ps1` 语法检查 → `Temp/compile/*.dll`。
3. 人把包装进 **test**（UPM Add from disk 或 VCC User Packages）。禁止装辉夜（封存）。
4. 头像根加组件 `OVA Protection`。菜单 **OVA → 打开 ova-web**。设置进 `Library/OVA/settings.json`。
5. **人**点 VRChat SDK Build & Publish。代理不点。
6. 作者用自己的 `.vrca` / NDMF clone 当红队（见 `docs/RED-BLUE.md`）。不迁 SaoMoLa 代码。

## NDMF 在克隆上做什么（源场景不动）

`OvaProtectionPlugin`，`BuildPhase.Optimizing`，`AfterPlugin`：

Modular Avatar → VRCFury → Avatar Optimizer → TexTransTool →（可选 FaceEmo blink）→ **OVA**

OVA 内部顺序：

1. Resolving：读 JSON，立刻 `DestroyImmediate` 组件（`IEditorOnly`，不进 bundle）
2. 水印：克隆网格，basis 带种子微扰（默认 ±1e-5）。SMR + MeshFilter。
3. 名字：Hierarchy / blendshape / 层 / 状态 → 同形字 `ÌÍÎÏ`（与 AvatarObfuscator / VRChat 内部同一字母表；默认种子 `5145514`，`0`=每次随机）
4. 参数：Animator + VRC Expression + 菜单 + 行为里恰好等于旧名的字符串。PhysBone **前缀 + prefix_*** 整组留。
5. 克隆资源名：Mesh / AnimationClip / mat / tex（Texture2D=`CopySerialized`）/ audio；动画 object-reference 与 PlayAudio 跟着走。

贴图加密：`crypto.textureMode = compose`（叠 ShellProtector / TTT / Ajisai）。**不**自写 lilToon 顶点锁。

## 保留表（这就是产品的一半）

- Hierarchy：根、名为 `Armature` 的子物体、Humanoid 骨及祖先、眼/颌、可选 MMD Body、`preserveNameSubstrings`
- 口型：`vrc.*` 以及 `v_aa` / descriptor 槽位名硬留
- 参数：VRChat 保留名、`Go/`、`VrcDcc/`、自动提示 `FT`/`GoGo`/`OSC`…、点选 `pinPreserve`、`extraPreserve` 子串
- `FT`：**词边界**（`FT_Blink` 留，`blink_left` / `Gift` 不留）。两字母 token 才走边界；更长仍子串。
- Humanoid fallback：不因第二个叫 `Chest` 的碰撞体把 `Dynamics` 整支留下。
- 参数子串 **不**看物体名 CSV。物体名 CSV 不改参数。

## ova-web

- 英文 catalog + zh-CN；Normal / Expert
- 参数点选 → `pinPreserve`
- AI pack zip（`ova-params-v1` 往返）
- 概览「将改 n / total」来自 **场景探针**，不是 NDMF 虚拟控制器（已知和 bake `renamed=2` 对不齐）
- Attest：本机算指纹 + 存 PAT；`POST /api/attest/publish` → **501**（Owner 停 Hop B）

## 明确不做

- 100% 防盗 / 观看者内存里没明文
- 扫别人 cache 钥、内核、热替换
- 官方没有的 lilToon 顶点锁
- 头像里跑自定义 C# / 观看者必须 HTTP 才能看见身体
- 把 OVA 当 SaoMoLa 的对面去「藏平台钥」
