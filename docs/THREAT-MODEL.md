# 威胁模型（蓝队）

一手来源：Lumina 文、Kanna README、AvatarObfuscator README、以及 **本机红队工程 SaoMoLa 的能力清单（只记类别，不写步骤）**。

## 现实

VRChat 上传的是 Unity AssetBundle。客户端要画网格，内存里最终是明文顶点。  
Lumina：加密只是让 CPU 再解一次；防不住读内存。  
Kanna：现在这一套也 **不是** 对热替换免疫。  
Owner 贴的注意：要 100% 不被窃取，请私人实例。

所以 OVA 的成功标准是：**拆开再用的成本 > 自己改一个模**，外加水印让二次上传能被认出来。

对照表：[STACK.md](STACK.md)。能做到的层级：[CAPABILITY.md](CAPABILITY.md)。OSC 锁弱点：[VENDOR-OSC-LOCKS.md](VENDOR-OSC-LOCKS.md)。

## 攻击类别（SaoMoLa 已证明存在，OVA 只防御）

| 类别 | 对防御的含义 |
|---|---|
| 缓存里的加密 bundle | 服务端/缓存加密 **不是** 作者能控制的层。作者层做网格/贴图/名字 |
| 进程内密钥 | Shader/表达式里的解密 key（Kanna 32 bit）会被当成参数同步。短 key = 可搜 |
| 热替换 | 游戏内换完仍可能「能看不能进 Unity」。水印仍然有用 |
| 开源拆包工具 | 名字可读、网格分开、贴图未合图集 = 白送 |
| 可复现混淆种子 | AvatarObfuscator / OVA 默认 `5145514` → 两次构建同名。OVA `seed=0` 则每次随机 |
| 内核 / 物理内存读 | 作者层做不到。GOAL 明确非目标。不要把这类代码迁进 OVA |
| VRChat 缓存 AES-GCM（客户端自己的钥） | 扫的是 **观看者本机** 解缓存用的钥。SaoMoLa **能**扫到「别人模」——因为你看见的模会进你的 cache。那把钥不在作者 NDMF 插件里，OVA 改不了别人 GameAssembly 怎么存它 |

## 三把钥（不要混）

`D:\Project\vrchat-il2cpp-re` 主业是 **客户端 Beebyte 类型改名**（`ÌÍÎÏ` 在 GameAssembly 元数据里），不是「给别人内存做混淆」。同仓防御手册 `output/p2_research/avatar_ripping_defense_playbook.md` 第一定律：

**要画这个模，观看者进程里必须有明文几何。** 网络/缓存/AssetBundle 密码都在那之前。作者插件改不了观看者 `GameAssembly` 里平台钥的存放。

| 钥 | 谁控制 | 有 OVA 源码能不能「藏住」 |
|---|---|---|
| 观看者解 **自己磁盘 cache** 的平台钥 | VRChat / Unity 引擎 | 不能。扫的是别人机器上的客户端，不是我们插件 |
| Unity AssetBundle 加载钥（手册里的引擎缝） | 引擎，不在头像内容里 | 不能。NDMF/Udon/Shader 到不了 `UnityPlayer` |
| 作者层运行时解锁（Kanna 32 bit 那类） | 我们 | **可以不放这把钥**（OVA v0.1 就不放）。若放了，观看者为了画出来仍要在进程/GPU 里用它；「有源码也解不开」挡得了照着插件复现，挡不住解完之后的网格抓取（手册：post-decrypt harvest **不需要**这把钥） |

「把 avatar 在别人内存里混淆」若指改观看者 VRChat 进程：那是客户端改写 / 注入，不是 VPM 保护包，OVA 不做。若指上传前打乱网格、观看者 Shader 再还原：那是 Kanna 类，辉夜 lilToon + 256 bit 默认不做；而且明文仍会出现在 `Mesh` / GPU。

il2cpp-re 能帮蓝队的是：**认清缝在哪、别把 Photon/libsodium 当成 bundle 密码、别承诺防 GPU 抓帧。** 不能把「别人内存」变成我们的画布。

## 「用红队逻辑当蓝队，能不能防被扫」

能模拟的是 **解出来之后好不好用**，不能模拟成 **让物理内存扫不到**。

SaoMoLa 主链（只记类别）：进程/物理内存里找客户端缓存钥 → 解开本地 `.vrca` → 开源拆包成工程。那把钥必须存在，否则 VRChat 自己也画不出模。把扫钥逻辑搬进 OVA **既防不住别人机器上的扫，也违反本仓停线**。

蓝队该做的对照实验（Owner 自己的抛掷头像 + 自己的红队软件，Agent 不迁代码）：

1. 解开后 Hierarchy / blendshape 是否仍是可读店名（OVA 名字层的验收）。
2. 网格是否分开、贴图是否还能按件抽（AAO+TTT，不是 OVA 重写图集）。
3. 有没有短的同步 Int 当运行时解锁钥（Kanna 那类；辉夜 256 也不该上）。没有这类钥，就没有「作者钥」可扫。
4. 已知保护品牌的指纹（检测器认 AvaCrypt / Kanna 等）——那是识别，不是解密。不要为了躲检测去抄红队扫描器。

热替换：游戏里能看 ≠ Unity 工程能用。水印是归因层（上传存活未证）。内核层：非目标。

不要在本仓库实现上述攻击。不要复制 SaoMoLa 的 `scripts/hotswap`、`scripts/extraction`、`research/drivers`、`drivers/`、缓存解密或扫 key 代码。`src/AnimeStudio` 算开源拆包类，只当存在证明。

## 防御层（从便宜到贵）

1. **AAO 合网格 + TTT 合图集 + UV 重排** — Lumina「精神攻击」。拆了也难二次改。
2. **构建时改名** — AvatarObfuscator：物体、blendshape、参数、材质克隆。源场景不动。MMD 要保留头。GoGo / 面捕参数要排除。
3. **贴图加密** — ShellProtector（lilToon 友好）或 AjisaiFlow。要 OSC 或参数。
4. **顶点锁** — Kanna：Basis 打乱 + 白名单 Shader 还原。32 bit。关 Shader = 刺猬。**lilToon 不在官方名单。** 辉夜用 lilToon 2.3.4，这一层默认 **不做**，直到有 lilToon 方案。
5. **菜单密码锁** — 挡「直接二次上传」，挡不住提资源。可当附加层。
6. **私人实例** — 唯一接近 100%。

## 辉夜约束（以后解冻再装）

- bake 曾 256/256。Kanna 32 bit 会爆。要先减肥。
- GoGo 参数名不能乱改（Kanna 有 Auto Detect 排除）。
- 不要在封存期往 `kaguya.unity` 丢 OVA。
