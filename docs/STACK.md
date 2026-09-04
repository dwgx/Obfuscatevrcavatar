# 工具对照（蓝队）

研究用。OVA 自己实现名字层；其余用现成 VCC 包。不把 SaoMoLa 的拆包/扫 key/热替换代码迁进来。

| | AvatarObfuscator 0.4.9 | Kanna Protecc / AntiRip | OVA v0.2 | SaoMoLa（本机红队，只记类别） |
|---|---|---|---|---|
| 装法 | unitypackage | GitHub | VPM `dev.ova.protection` + ova-web | 不装进头像工程 |
| 何时跑 | NDMF 构建克隆 | NDMF + 自定义 Shader | NDMF Optimizing，MA/VF/AAO/TTT 之后 | 客户端/缓存/进程 |
| 改名 | 同形字：物体、blendshape、animator/VRC 参数、克隆的 mat/tex/audio。源场景不动 | 不以此为主 | 同形字 `ÌÍÎÏ`：物体、blendshape、层/状态、参数（保留表）、克隆 mat/tex/audio。源场景不动。**bake 未证** | 可读名会降低拆包成本 |
| Humanoid / MMD | 保留骨骼路径、可选头网格 | n/a | 根、Armature、Humanoid 祖先、眼/颌、可选 Body | — |
| 参数 | 可改 VRC / animator 参数名 | 32 bit 密钥参数；上传后 **Write Keys**，删掉未加密的旧上传 | 改；硬留 VRChat / `Go/` / `VrcDcc/` / FT 子串 / 点选 pin / PhysBone 前缀整组 | 短 key 可搜 |
| 顶点 | 不锁 | Basis 打乱 + 白名单 Shader | 不做锁；keyed 微扰水印（上传存活未证） | 内存里仍是明文顶点 |
| lilToon | 不管 Shader | **官方名单没有** | 不做顶点锁，见 [LILTOON.md](LILTOON.md) | — |
| 合网格/图集 | 让你用 TTT | 不管 | 让你用 AAO+TTT，见 [COMBO.md](COMBO.md) | 分开的网格/贴图更好抽 |
| 热替换 | 不防 | README：不免疫 | 不防；指纹验证发布仍 501 | 已证明这类攻击存在 |
| 固定种子 | 默认 `5145514` 可复现 | 密钥在 LocalAvatarData | 默认同样 `5145514`；**`seed=0` 每次随机**。见 [NAME-PASS.md](NAME-PASS.md) | 可复现 = 两次构建同名 |

威胁类别与防御顺序：[THREAT-MODEL.md](THREAT-MODEL.md)。来源笔记：[../research/2026-09-03-sources.md](../research/2026-09-03-sources.md)。
