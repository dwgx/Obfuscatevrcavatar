# 蓝队能做到什么（2026-09-03 研究）

一手：Lumina 文、Kanna/AntiRip README、ShellProtector README、AjisaiFlow VPM 说明、DexProtect 商店页、本机 `vrchat-il2cpp-re` 防御手册 §0–§6、OVA 现码、辉夜 `Packages/vpm-manifest.json`。  
不迁 SaoMoLa 扫钥/驱动/热替换。不装进辉夜。

OSC / 顶点锁弱点与引用（2026-09-04）：[VENDOR-OSC-LOCKS.md](VENDOR-OSC-LOCKS.md)。

## 第一定律（作者层改不掉）

观看者要画出这个模，进程/GPU 里必须出现明文几何（手册 `avatar_ripping_defense_playbook.md` §0）。  
SaoMoLa 能扫到「别人模」的 **观看者本机 cache 钥**——那把钥是客户端解自己磁盘的，不在 NDMF 插件里。

| 做不到 | 为什么 |
|---|---|
| 让别人内存里没有钥、没有明文网格 | 引擎要画；解完之后抓 Mesh 甚至不需要 cache 钥 |
| GPU 抓帧 | 在游戏之下 |
| 改观看者 GameAssembly / UnityPlayer 怎么存平台钥 | 头像内容到不了那层 |
| 100% 防盗 / 永远扫不到 key | 没有作者层公开方法。DWR 是 VRChat 的；观看者内存里仍有钥。论坛「神奇办法」是攻击侧扫内存，不是 NDMF 能写进 bundle 的 |

作者层没有「永远扫不到平台 cache 钥」的公开方法。

## 做得到：按「拆开再用有多痛」

| 级 | 层 | 工具 | 挡什么 | 挡不住 | 辉夜现在 |
|---|---|---|---|---|---|
| L0 | 社交 | 私人实例 + 头像 Private 上传旗 | 克隆/台座/未同房下载 | GPU 加载后仍明文 | Owner 自己开 |
| L1 | 精神攻击 | AAO Merge Skinned Mesh + TTT 合图集/UV | 开源拆包后衣服还是一件一件好改 | 内存/GPU 明文；cache 钥；**不是锁** | **工程里还没有 AAO/TTT**（MA + NDMF + lilToon 2.3.4 + VF） |
| L2 | 构建改名 | **OVA v0.2** 或 AvatarObfuscator | Hierarchy / blendshape / 参数名（有保留表） | 几何仍在；**0 synced bit**；bake 未证 | 封存，未装 OVA |
| L3 | 贴图加密 | ShellProtector（VPM；官方写 lilToon **到 2.3.4**，辉夜同版 **未装未测**）或 Ajisai XOR（lilToon 2.3.2+） | 盘里 MainTex 不是原图 | 网格仍可抓；关 Shader；Shell 无 OSC 时约 **11–32 bool**；Ajisai 解锁 **1 Float = 8 bit** | 辉夜 256/256 → **先核 bit** |
| L4 | 顶点锁 | Kanna / GTAvaCrypt 类 | 盘里 Basis 是乱的 | **无官方 lilToon**；**+32 bit**；关 Shader=隐身；同实例可观察 BitKey；热替换不免疫 | **双禁** |
| L5 | OSC / 多层锁 | DexProtect；Ajisai 宣称 8 层（水印+mesh lock+decoy） | 无 key 二次上传难看 | 闭源；Ajisai 文档版号与 VPM 0.50.0 可能不一致；关自定义动画仍痛 | 未评估装辉夜 |
| L6 | 菜单密码 | 各种 lock | 挡「原样二次上传」 | Lumina：资源提出来密码也在动画里 | 低优先 |
| L7 | 水印 | 顶点微扰 / UV / 贴图 LSB / 隐藏 morph | **归因**（二次上传还能认出来） | 不防盗；重拓扑可能冲掉 | OVA **v0.2 已打 keyed basis 微扰**；上传后是否还在未验证。贴图不要 LSB |

顺序（COMBO）：MA/VF 出最终衣服 → AAO/TTT → OVA 水印微扰 → OVA 改名/参数 → 可选 Shell/Ajisai。顶点锁最后且默认关。水印上传存活未证。

## 红队「保护检测」≠ 破解

SaoMoLa 文档只声称认出三种指纹，且 **解密 cache 不等于解开内容**：

| 指纹 | 含义 |
|---|---|
| AvaCrypt `_BitKey0`…`31` | 有顶点锁痕迹；解开 cache 后网格仍可是乱的 |
| 字面量 `DexProtect` | 有整模锁痕迹；文档写解开后仍不可用 |
| Kanna `_IsLocked` | 有锁标记；不是拿到钥 |

没有声称能打败合网格+合图集。蓝队不要为了躲这三串去抄扫描器；L1 劳动层本来就不在检测名单里。

## OVA 自己现在

已做：Optimizing、MA/VF/AAO/TTT 之后；水印微扰；同形字物体/blendshape/层/状态名；参数名（自动保留，`FT` 词边界）；克隆 Mesh / clip / mat / tex / audio 名。源场景不动。配置走 **ova-web** JSON。作者自拆包闭环：[RED-BLUE.md](RED-BLUE.md)。

未接线：GitHub/Gitee **真发布**（骨架已落：指纹 + `secrets.json` + 501 stub）；自研贴图加密（`textureMode=ova`）；设置文件 DPAPI。本地 listing 已是 **0.2.0**（`scripts/pack-vpm.ps1`，`source.json` 与 zip sha 一致）。

不做：顶点锁、图集、扫钥、注入观看者、装辉夜。

## 水印（L7，归因，不防盗）

懒人二次上传 / AssetRipper 路径：优先 **带密钥的顶点微扰**（AAO 合完再打，一个身体网格带着）。其次作者侧拓扑指纹登记（对照二次上传，不必进包）。UV 指纹必须在 TTT 之后。贴图 **不要 LSB**（VRC 压缩会碾）。隐藏 blendshape 当附属。不要把身份放进同步 Expression 参数。

GPU 抓帧和认真重拓扑仍然冲得掉——只为归因。

## A / B / C（头像契约）

| 层 | 谁跑 | OVA |
|---|---|---|
| **A** 上传克隆 | Unity Editor NDMF。改名、克隆 mesh。`IEditorOnly` 进不了 bundle | **只做这一层** |
| **B** 观看端 | 客户端已有的 Animator / shader / 同步参数 / 本机 OSC。头像 **不能**带自定义 C# | 不做。要让别人看见正确身体 ⇒ 别人 GPU 必须有明文或等价 uniform |
| **C** 进程/内核 | GameAssembly 缓存钥、UnityPlayer bundle 钥、别人内存 | 碰不到 |

冲突钉死：**「别人看见正确身体」和「别人拿不到正确身体」不能同时被 NDMF 满足。** 同步 bit 是把钥交给每个要画你的人，不是机密信道。

## 有源码时还剩什么

- L1/L2 公开算法也不怕：价值是 **劳动**，不是秘密。
- L3/L4/L5：观看者 Shader/OSC **必须**用钥还原。源码帮人复现解锁器；不帮人从「没在看的进程」里拿走平台 cache 钥。解完后的网格抓取 **不需要**作者钥。
- 因此「有插件源码也无法解、也无法抓到 avatar 解密 key」**不能**作为 OVA 承诺。能承诺的是：不自己再塞一把 32 bit 同步钥给扫描器当作者钥；解开后的工程难改；二次上传带水印。

## 抛掷工程（不是辉夜）上的验收

1. 空 2022.3 + OVA：日志 `[OVA] name pass:`，厂商 GoGo 名仍可读在源场景、克隆上不可读。
2. 再加 AAO+TTT：拆开后不是分件衣服。
3. 可选 ShellProtector：无 OSC 时贴图噪声/16×16 fallback。
4. Owner 用本机红队打 **自己的** 抛掷模：看解开后好不好改。Agent 不迁红队代码。
