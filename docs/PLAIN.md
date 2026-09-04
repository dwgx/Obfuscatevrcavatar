# 白话介绍

先读这一页。技术细节在 [CAPABILITY.md](CAPABILITY.md)、[COMBO.md](COMBO.md)、[OVA-WEB.md](OVA-WEB.md)。

## 做什么

OVA 是给你**自己的** VRChat 头像用的蓝队 NDMF 插件。上传时它只动 **克隆**：改名、加诱饵 blendshape、打一点点顶点水印。源场景还是可读的。

目标就一句：**拆包二次利用变贵**。不是让别人看不见、也不是让别人显卡里没有网格。

## 不做什么

- **不**把网格从别人 GPU 里藏起来。别人要看见你的身体，画的时候就必须有明文。画完就能抓。这叫 B 路，OVA 挡不住。
- **不**声称 100% 防盗。官方也说了，公开世界里没有这回事。私人实例仍然是唯一接近的办法。
- **不**做顶点锁、OSC 解锁、lilToon 破解、`_BitKey` / `DexProtect` / `_IsLocked`。
- **不**扫别人的 key，**不**装进封存中的辉夜，**不**代替你点 VRChat SDK Publish。
- 水印现在只是编辑器里打过（`editor-only-unverified`）。上传以后还能不能认，这一轮**没证**，不要把它当成「已经可归因」。

## 怎么点

1. 头像工程装 VPM 包 `dev.ova.protection`（不要装辉夜）。
2. 头像根加组件 **OVA Protection**。
3. Unity 菜单 **OVA → 打开 ova-web**，浏览器 `http://127.0.0.1:17849/`。
4. 概览上有两块，别看混了：
   - **场景估计**：当前场景探出来的数。
   - **上次克隆实测**：NDMF 烤过才有。没烤过会写「还没烤过 / 报告过期」，那不是 0/8。
5. 保护页勾你要改的层，保存。NDMF 预览或构建时才会动克隆。
6. SDK Publish 你自己点。Agent 不点。

没有 Unity 时可以预览控制台（没有实测报告）：

```powershell
powershell -NoProfile -File host/ova-web/preview.ps1
```

## 怎么叠 AAO / TTT

OVA **不**自己合网格、不合图集。拆包还好改，通常是因为衣服还是一件一件、贴图还是一张一张。

推荐顺序：

1. Modular Avatar / VRCFury 先把衣服做出最终形态。
2. **Avatar Optimizer** 合 Skinned Mesh。
3. **TexTransTool** 合图集和 UV。
4. **OVA** 最后改名 / 水印（已经排在这几家后面）。
5. 贴图加密要用 ShellProtector 或 Ajisai 再另叠。顶点锁默认不要。

叠完仍然不是 100%。B 路还在。私人实例仍是上限。

---

## English

OVA is a blue-team NDMF plugin for **your own** Unity 2022.3 VRChat avatars. It makes unpack-and-reuse expensive on the **upload clone**. It does **not** hide the mesh in someone else's GPU. Path B (live Mesh / GPU harvest) stays open. Private instances are still the only near-guarantee. Do not claim 100% anti-rip.

Open **ova-web** at `http://127.0.0.1:17849/` (Unity: **OVA → 打开 ova-web**). Overview splits **Estimated from scene** vs **Last NDMF clone**. A 404 last-build report means not baked yet — not scene `0/8`. Watermark status stays `editor-only-unverified`.

Stack AAO merge + TTT atlas **before** OVA. Do not install into Kaguya. Do not SDK Publish from an agent.
