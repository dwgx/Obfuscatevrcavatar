# lilToon × Kanna-style vertex lock (v0.3 spike)

一手：Kanna / AntiRip README 的 Shader 白名单是 Poiyomi、UTS、Sunao、XSToon、GTAvaToon。**没有 lilToon。**  
辉夜用 lilToon 2.3.4。关 Shader 的人看到的是刺猬 / 兜底网格，不是「加密失败变正常模」。

结论：**OVA 不做 lilToon 顶点锁。** 不要写一套非官方 lilToon 解锁变体。lilToon **2.x 已删掉** 自带 `AvatarEncryption`。贴图层：ShellProtector 官方写支持到 **2.3.4**（与辉夜同号，**未在辉夜实测**）；Ajisai 宣称 2.3.2+ 全贴图+shader mesh。顶点层等有官方/可维护的 lilToon 方案再开。

32 synced bit 也会撞辉夜曾 256/256 的 bake。官方流程还有 **Write Keys**（密钥进 LocalAvatarData）以及删掉未加密的旧上传；OVA 不做这一层。私人实例仍是唯一接近 100% 的办法。
