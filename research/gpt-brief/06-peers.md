# 别人已经做的（一手，不抄进 OVA）

对照表也见产品仓 `docs/STACK.md`、`docs/VENDOR-OSC-LOCKS.md`、`research/2026-09-03-sources.md`。

## AvatarObfuscator 0.4.9 — 劳动层近亲

- https://github.com/cocokoishi/AvatarObfuscator MIT
- unitypackage，NDMF ≥ 1.6，clone-only 同形字 `ÌÍÎÏ`
- 改：blendshape、GameObject、animator/VRC 参数、克隆 mat/tex/audio 名、部分 mesh/clip 文件名
- 不改：Shader、VRC 保留参数、Preserve MMD 时的头网格
- 参数排除默认子串 `FT,eye`（**子串**，会误伤 `Gift`；OVA 已改成 FT **词边界**）
- 默认种子 `5145514`（OVA 相同）；`0`=随机
- 自己的贴图混淆已删除，改推 TTT
- README 还建议「密码锁 + OSC」当附加成本 — 那是 L6/L5，不是 OVA 核心
- **OVA 跟约束自己写 pass，不粘贴对方 20k 行**

## Esska AV3Obfuscator — 更早的改名器

- https://github.com/Ess-Ka/EsskaAV3Obfuscator
- VCC listing（文档写 ess-ka.github.io，有 archived 记录）；wiki 仍见 `com.esska.av3obfuscator` 2.3.1
- 改层级、controller/层/状态/BlendTree、可选参数/mesh/blendshape/mat/tex/audio
- **自己承认**：偷的人仍能拿到 mesh/mat/tex，只是更难改
- 不改 Shader
- 第一次上传要把 avatar ID 写回原文件（和 Kanna Write Keys 同类流程负担）

## Kanna Protecc / PlagueVRC AntiRip — L4 顶点锁

- https://github.com/PlagueVRC/AntiRip
- FAQ：https://raw.githubusercontent.com/PlagueVRC/AntiRip/master/Readme/FAQ.md
- Basis 打乱 + 白名单 Shader + **32 synced bits**
- 白名单：Poiyomi、UTS、Sunao、XSToon、GTAvaToon。**无 lilToon**
- 别人必须开满 Shader/动画否则隐身（刺猬）
- Write Keys → LocalAvatarData；删未加密旧上传
- Quest 不支持；Quest 无锁双胞胎会否定 PC 锁
- 自己承认：不是 foolproof；特化热替换「游戏里能看、Unity 里不能」
- 前身：https://github.com/rygo6/GTAvaCrypt（同类 32bit + Poi 注入）

## DexProtect — L5 OSC 整模锁

- https://jinxxy.com/Dextro/DexProtect （Gumroad 同产品）
- Editor 打乱 + **观看者 PC 上 OSC 程序** 解锁；宣称不需要自定义 Shader
- 19 bit Expression；SDK 3.5.2+；Unity 2022.3.22f1
- 别人必须开 Custom Animations；新观察者会隐几秒
- Fallback 模 **不受保护**
- Key 在 `Documents/DexProtect` 按 avatar id；**可以分享**
- 自己写：没有系统完美；很难拿回全功能原件，但 **不是不可能**
- 营销「最高安全」当文案，不当测量

## ShellProtector — L3 贴图（lilToon 友好）

- https://github.com/Shell4026/ShellProtector
- VCC https://shell4026.github.io/VCC/
- 贴图加密 + Shader 解密；OSC 输密码 https://github.com/Shell4026/ShellProtectorOSC
- lilToon **1.3.8–2.3.4**（与辉夜同号，**未在辉夜实测**）
- 也做 blendshape 混淆、好友 fallback 小图
- XXTEA/ChaCha8，key 经 SHA-256
- BC7 不支持
- **OVA compose，不重写**

## AjisaiFlow Anti-Ripping — L3/L2 宣称多层

- VPM `net.ajisaiflow.anti-ripping` **0.50.0**
- 站点 https://ajisaiflow.net/prod/anti-ripping/ ；BOOTH 免费 α
- 宣称 NDMF 8 层：mesh/tex 加密、blendshape decoy、难读化；lilToon 2.3.2+
- 自己写：不能完全阻止提取
- 产品文案 vs zip **本仓未复审**。当 compose 候选，不当已证 Path A

## 优化器（不是锁，但是 L1）

- AAO https://github.com/anatawa12/AvatarOptimizer VPM https://vpm.anatawa12.com/vpm.json
- TTT https://github.com/ReinaS-64892/TexTransTool VPM https://vpm.rs64.net/vpm.json
- Lumina 点名这两家当精神攻击

## 已死 / 不要跟的

- lilToon 2.x **删掉** 自带 AvatarEncryption（https://lilxyzw.github.io/lilToon/ja_JP/migrate1to2.html）
- 非官方 hedgehog 顶点锁、32bit 无名单 Shader

## 对 OVA 的含义

别人已经把「改名劳动」和「锁+作者钥」两条路走完了。OVA 的差异应是：

- VPM + ova-web（嵌套功能、点选保留、中英）
- 跟 AvatarObfuscator 约束但保留表更严（FT 词边界、Chest 不误伤、viseme 槽）
- **故意不放作者钥**，所以检测器三串不出现
- 水印归因而不是锁
- 叠 AAO/TTT 而不是再写图集
