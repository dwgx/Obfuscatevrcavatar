# 名字层 + 构建管线（克隆上）

NDMF `dev.ova.protection` **0.2.0** 在 **Optimizing**、Modular Avatar / VRCFury / AAO / TTT **之后**跑。`AfterPlugin` 在对方没装时是空约束。

主 UI：**ova-web**（`OVA → 打开 ova-web`，`http://127.0.0.1:17849/`）。设置写入 `Library/OVA/settings.json`。JSON 不存在时才用组件上的后备 `OvaSettings`。

## 顺序

1. **Resolving**：读 JSON / 嵌入设置，立刻 `DestroyImmediate` 组件。
2. **水印**：克隆网格，对 Basis 做带种子的微扰（归因，不是锁）。
3. **物体名 / blendshape / 层 / 状态名**：同形字 `ÌÍÎÏ`。源场景不动。
4. **参数名**：Animator + VRC Expression + 菜单 + 行为/Contact 上**恰好等于**旧参数名的字符串。PhysBone **前缀**以及 `前缀_*` 整组留（`Hair` 和 `Hair_IsGrabbed` 一起留）。自动保留 GoGo / FT / OSC / `VrcDcc/` / VRChat 保留参数。ova-web **点选**写入 `parameters.pinPreserve`（精确名）。**参数子串**只看 `parameters.extraPreserve`，不看物体名子串。Animator clip 上 `type=Animator` 的参数曲线一并改名。SyncedLayer 覆盖动画/行为也会走。
5. **克隆资源名**：Instantiate 后的 **Mesh / AnimationClip / mat / tex / audio**。动画 **object-reference** 曲线和状态机行为上的 AudioClip 数组一并指到克隆。源场景不动。SyncedLayer 覆盖运动上的 BlendTree 参数名一并改。

物体名 / blendshape 的 `preserveNameSubstrings` 只留 **Hierarchy / shapekey**。`FT` 按词边界匹配（`FT_Blink` 留，`blink_left` / `Gift` 不留）。Humanoid / Armature / 眼 / 下颌 / 可选 MMD `Body` 仍硬留。

blendshape 曲线键用 NDMF `ObjectPathRemapper.GetVirtualPathForObject`，避免改 Hierarchy 之后对不上虚拟 clip 路径。

## 必留（Hierarchy）

头像根、名为 `Armature` 的子物体、**所有 Humanoid 骨骼及其祖先**、眼/下颌 Transform、可选 MMD `Body`、以及 `preserveNameSubstrings` 命中的物体名。口型 `vrc.*` **以及** SDK 常见 `v_sil` / `v_aa` / `v_ee` 等 viseme token **不改**（`VRCAvatarDescriptor.visemeBlendShapes` 槽位名也硬留）。Humanoid fallback 只补 `GetBoneTransform` 没命中的骨，避免第二个叫 `Chest` 的碰撞体把 `Dynamics` 整支留下。同子串也可留 blendshape。

## 种子

默认 `5145514`。**`0` = 每次随机**。

## 不做什么

- 不合网格、不合图集（用 AAO + TTT）。
- 不写 lilToon 顶点锁。贴图加密：`crypto.textureMode = compose`（ShellProtector 2.6.3 官方写到 lilToon 2.3.4）。
- 不装进封存中的辉夜。
- 不从 SaoMoLa 迁提取 / 扫 key / 驱动。

算法约束对照公开的 [AvatarObfuscator](https://github.com/cocokoishi/AvatarObfuscator) 0.4.9（MIT）。OVA 自己写 pass，不粘贴对方文件。

## 试

空的 2022.3 头像工程：Add from disk → 本仓库 `Packages/dev.ova.protection/package.json`。头像根加 **OVA Protection**。打开 ova-web 存一份设置。人点 SDK。日志应有 `[OVA] name pass:` / `parameter pass` / `watermark pass`。
