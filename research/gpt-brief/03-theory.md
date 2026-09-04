# 理论（必须先信这个再想方案）

## 第一定律

来源：`D:\Project\vrchat-il2cpp-re\output\p2_research\avatar_ripping_defense_playbook.md` §0。

> 要渲染一个头像的客户端，必须在自己的内存里持有明文的几何、贴图、骨骼。  
> 所有网络密码、CDN 签名、元数据加密、内容混淆，都在那一瞬间 **之前**。

推论：

- 没有任何「作者层客户端密码学」能让 **控制那台机器的人** 提取变成不可能。
- 防御不是「阻止解密」。防御是：别把包送给不信任的人、让进程内篡改可检测可封、提高再工具化成本、做归因/下架。
- GPU 抓帧（RenderDoc 一类）在游戏之下，作者层 **近乎不可防**。手册诚实写 Low。

Lumina（https://share.lumina.moe/posts/vrchat-anti-ripper/）：AssetBundle 不是安全边界；合网格+合 UV+合图集是「精神攻击」；Shader 锁在低信任关 Shader 时变成刺猬，他认为得不偿失。

## Path A vs Path B（不要混）

| | 攻击者手里有什么 | OSC / Shader 锁 |
|---|---|---|
| **A 磁盘 Unity 工程** | cache `.vrca` → 平台解密 → AssetRipper | 常常「解开 cache 后网格仍乱 / 看不见」。厂商广告的是这条。检测器同意。 |
| **B 已经画出来** | 活 `Mesh` / GPU | **不停**。观看者必须持有明文才能画。解完后的 harvest **不需要作者钥**。 |

定理：**「别人看见正确身体」和「别人拿不到正确身体」不能被 NDMF 同时满足**，除非还原钥（同步 bit / OSC / Shader uniform）存在于 **观看者** 上。那把还原钥就是作者钥，不是机密信道。

Owner 愿望「连 agent 都不能还原」：Path A 可以做得很痛。Path B 在头像层做不到不可能。

## 三把钥

| 钥 | 谁控制 | OVA 源码能不能藏住 |
|---|---|---|
| 观看者解自己磁盘 cache 的平台 AES-GCM | VRChat / 引擎 | 不能。扫的是别人机器上的客户端 |
| UnityPlayer AssetBundle 引擎缝（`EncryptionKey*`，手册纠正：不是 Photon/libsodium/`vrc_fast_crypto`） | 引擎 | 不能。NDMF/Udon/Shader 到不了 |
| 作者运行时解锁（Kanna 32bit、Dex OSC、Shell OSC） | 我们 | **可以不放**。放了：观看者为了画仍要用它；检测器会指纹；解完 harvest 不需要它 |

`vrc_fast_crypto` / PhotonEncryptor / libsodium **不是** 头像 bundle 密码。想方案时不要把它们当成 OVA 能拧的旋钮。

## 作者层能做的（L0–L7）

| 级 | 层 | 工具 | 挡什么 | 挡不住 |
|---|---|---|---|---|
| L0 | 社交 | Private 头像 + 私人实例 | 没同房就没有 cache | GPU；同房后仍明文 |
| L1 | 精神攻击 | AAO Merge Skinned Mesh + TTT atlas/UV | 拆包后衣服还是一件一件 | 内存/GPU；不是锁 |
| L2 | 构建改名 | OVA 或 AvatarObfuscator 或 Esska | 店名/参数/克隆资源名 | 几何仍在；0 synced bit |
| L3 | 贴图加密 | ShellProtector（lilToon→2.3.4）或 Ajisai | 盘里 MainTex 不是原图 | 网格仍可抓；关 Shader/OSC |
| L4 | 顶点锁 | Kanna / GTAvaCrypt 类 | 盘里 Basis 乱 | 无官方 lilToon；+32 bit；刺猬；同实例可观察 BitKey |
| L5 | OSC / 多层 | DexProtect；Ajisai 宣称多层 | 无 key 二次上传难看 | 闭源；关自定义动画；key 可分享 |
| L6 | 菜单密码 | 各种 lock | 挡原样二次上传 | 密码往往在动画里 |
| L7 | 水印 | 顶点微扰 / 拓扑指纹 | **归因** | 不防盗；重拓扑/GPU 可冲 |

OVA v0.2 做 **L2 + L7**，L1 **compose**，L3 compose 预留，L4 默认关，L5 不抄，attest 是 L7 的登记而不是锁。

## 平台侧（作者插件做不到，但要想清楚）

手册：本地一切反拆在 EAC 内核完整性上叠了五顶帽子，bypass 后一起掉。  
**活下来的**是服务端：EAC session 门、上传溯源（「你上传的包指纹像你刚下过的别人的包」）。  
这是 VRChat 的活，不是 VPM 插件的活。不要在 OVA 里假装能做服务端门。

VRChat 2024.2.3p2 一类「下载门」（同实例 + 验证客户端）减少 **没看见的人** 的 cache，不是 GPU 密码。

## 检测 ≠ 破解

SaoMoLa 公开 README：解开 **平台 cache** 之后，AvaCrypt/Dex/Kanna 包仍乱码/不可见/加密。无保护包「完全可用」。

这证明：厂商锁在 **Path A** 有效。  
这不证明：Path B 失败。同一份 README 把 cache 钥提取和热替换写成 **另一些功能**。

蓝队不要为了躲三串指纹去抄扫描器。L1 劳动层本来就不在那张检测表里。
