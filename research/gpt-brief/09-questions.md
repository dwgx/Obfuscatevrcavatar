# 请逐条想（不要跳）

每条先判断：Path A 还是 B、是否撞停线、OVA 是否已有、要不要 compose 别人。

## 劳动（Path A）

1. 合网格/合图之后，MMD 世界仍要一个叫 `Body` 的物体。OVA 现在 `FindMmdBody` 留的是「≥4 个保留 shape 的 SMR」，合完后 Robot 上 `BodyGo=False`。怎么留名字又不把整支 Dynamics 留下？要不要变成「留名称 Body，但只留那一个 transform」？
2. decoy blendshape：按 mesh 还是按头像？上限 32 是每个 mesh 还是总共？decoy 要不要避开 viseme 槽的 index 位置（现在是 append）？
3. 还没汤的可读表面：Animator 状态机结构已汤；SyncedLayer override 未证。下一只夹具头像该带什么？
4. 材质槽数、shader 关键字、lilToon 属性名 — 拆包后仍可读。OVA 该不该碰 Shader 名？AvatarObfuscator 明确不碰。
5. Humanoid 必须留的路径 vs 衣服骨架上重复的骨名。Chest 修复是否够，还是要白名单「只有 Animator.GetBoneTransform 命中的那一个」？
6. 音频/视频/VRCPhysBone 组件上的 **物体引用路径** 是否还有漏网（SerializedObject 字符串 vs 真正 PPtr）？

## 归因（L7）

7. 顶点 ±1e-5 过 VRChat 网格压缩后还在吗？若不在，下一个水印该放 UV（TTT 之后）还是隐藏 quad（Kanna FAQ 建议的那种）还是拓扑哈希只存在作者登记里？
8. `ova-fp-v1` 太容易碰撞。要不要 `ova-fp-v2` = seed + handle + mesh 内容摘要？摘要放 gist 还是只放作者硬盘？
9. Owner 已停 Hop B。在不停的前提下，本机「导出取证 JSON」是否够用？

## compose（不要重写）

10. ShellProtector 在 **test** 上叠一层 MainTex：bit 预算、lilToon 2.3.4、和 OVA 改名谁先谁后？
11. Ajisai 0.50.0 值不值得在 test 上做一次 Path A 烤？还是文案未审计，先别碰？
12. Dex/Kanna 的 Path A 很强。在辉夜 lilToon + 256bit 约束下，有没有 **不换 Shader、不加 32bit** 的顶点乱序是诚实的？手册和 Lumina 的答案大概是「没有」。请论证或给出反例（必须 1p）。

## 幻想检验（请主动打脸）

13. 「有 OVA 源码也无法拿到 avatar 解密 key」——三把钥里哪一把被这句话偷换了？
14. 「观看者切模时 HTTP 验证再给网格」——头像契约哪一条禁止？
15. 「把网格在别人内存里混淆」——若指 GameAssembly 注入，为什么不是 OVA？若指 Kanna，代价是什么？
16. 「躲过 SaoMoLa 检测表 = 防盗」——检测表只认三串。L1 劳动层根本不在表上。

## 流程

17. 人点 SDK 仍卡 OTP。有哪些 **不上传** 也能证的下一刀？（例如第二个抛掷模带真裙子 + SyncedLayer）
18. listing zip 落后。这是产品 bug 还是发行纪律？GPT 不要设计新的 VPM 商店，只要说要不要让 `pack-vpm.ps1` 进每次切片的 Definition of Done。
19. 私有 GitHub 已经是源码仓。不要建议用 PAT 当远程 VPM zip 主机（VCC issue：listing header ≠ Release 资产下载）。

## 输出时

优先 **可证伪的小切片**，不要「重写一个 Kanna」。每张卡写清：在 test 上怎么失败。
