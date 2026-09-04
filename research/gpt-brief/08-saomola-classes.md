# SaoMoLa — 只记类别

路径：`D:\Project\SaoMoLa`（本机红队树，MIT 公开过）。  
**禁止**把 `scripts/hotswap`、`scripts/extraction`、`research/drivers`、`drivers/`、扫钥、cache 解密代码复制进 OVA 或本 gpt-pack。

本文件只回答：红队已经 **证明哪些攻击类存在**，好让蓝队别做梦。

## 它声称的主链（类别）

观看者本机 VRChat cache 是 AES-256-GCM → 用内核物理内存读拿到 **观看者自己的** 平台钥 → 解开本地 `.vrca` → AssetRipper 成工程。

那把钥必须存在，否则 VRChat 自己也画不出模。把它搬进 OVA **既防不住别人机器上的扫，也违反停线**。

## 保护检测表（公开 README，2026-09-04 仍在）

| 指纹 | 厂商 | 解开 **平台 cache** 之后 |
|---|---|---|
| `_BitKey0`…`_BitKey31` | AvaCrypt / Kanna 类 32 synced bits | mesh 仍乱码 |
| 字面量 `DexProtect` | DexProtect | 仍不可见 |
| `_IsLocked` | Kanna Protecc | 仍加密 |
| 无上述指纹 | 普通模 | 「完全可用」 |

含义：

- Path A 上，厂商锁 **有效**（劳动层 OVA 不在这张表里，也没声称能骗过这张表）。
- 这不是 Path B 失败的证据。
- OVA `no-lock-fingerprint`：grep 干净，避免成为「这是锁包」的廉价分类器。躲检测 ≠ 藏平台钥。

## 其它已声称的类（不要展开步骤）

- 工程还原（AssetRipper + 修常见错误）
- 热替换上传（实验性；游戏里能看 ≠ Unity 能用）
- Unity 桥接修工程

## 蓝队对照实验（Owner 自己的抛掷模 + 自己的红队软件；Agent 不迁代码）

1. 解开后 Hierarchy / blendshape 还是不是店名。
2. 网格是否分开、贴图是否还能按件抽。
3. 有没有短同步 Int 当运行时解锁钥。
4. 有没有三串指纹。
5. 水印是否还在（上传后）。

GPT 想方案时：用这些当 **验收问题**，不要用这些当 **实现说明书**。
