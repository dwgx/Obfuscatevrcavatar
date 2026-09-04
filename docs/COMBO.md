# v0.2 配合：TTT + AAO（文档，不重写图集）

Lumina：[论 VRChat 如何反制盗模](https://share.lumina.moe/posts/vrchat-anti-ripper/) — AssetBundle 不是安全边界。拆了还能用，通常是因为 **网格分开、贴图没合、UV 还能对上**。

## 推荐叠法（别人的工具，OVA 不重做）

| 层 | 工具 | 作用 |
|---|---|---|
| 合网格 | Avatar Optimizer (`com.anatawa12.avatar-optimizer`) Merge Skinned Mesh | 拆包后衣服/身体不再是独立好改的网格 |
| 合图集 + UV | TexTransTool (`net.rs64.tex-trans-tool`) | 贴图不再按衣服一张一张能抽 |
| 构建时改名 | **OVA v0.2** 或 AvatarObfuscator | Hierarchy / blendshape / 参数难读；源场景仍可读。bake 未证 |
| 贴图加密 | ShellProtector（lilToon）或 AjisaiFlow | 要 OSC 或参数；和改名是另一层 |
| 顶点锁 | Kanna Protecc | **官方名单无 lilToon**。辉夜 lilToon 2.3.4 默认不要上。32 synced bit。关 Shader = 刺猬。上传后还要 Write Keys（OVA 不做）。引用：[VENDOR-OSC-LOCKS.md](VENDOR-OSC-LOCKS.md) |

顺序：MA / VRCFury 先出最终衣服 → AAO / TTT 再合 → OVA 最后改名。OVA 已 `AfterPlugin` 这几家。

## 不要

- 不要在 OVA 里再写一套 atlas。
- 不要声称叠完就 100%。私人实例仍是唯一接近的办法。
- 不要把 Kanna 32 bit 丢进已经 256/256 的辉夜。
