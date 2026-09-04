# 管线与配置

## 谁在哪一层跑

| 层 | 谁跑 | OVA |
|---|---|---|
| **A 上传克隆** | Unity Editor NDMF。改名、克隆 mesh。EditorOnly 不进 bundle | **只做这一层** |
| **B 观看端** | 客户端已有的 Animator / shader / 同步参数 / 本机 OSC。头像不能带自定义 C# | 不做。要让别人看见正确身体 ⇒ 别人 GPU 必须有明文或等价 uniform |
| **C 进程/内核** | GameAssembly 缓存钥、UnityPlayer bundle 钥、别人内存 | 碰不到 |

## 推荐叠法（别人的工具）

MA / VRCFury 先出最终衣服 → AAO 合网格 → TTT 合图集/UV → **OVA 水印再改名** → 可选 Shell/Ajisai。顶点锁最后且默认关。

OVA `AfterPlugin` 已声明 MA、VF、AAO、TTT。对方没装时是空约束。

TTT 在 **batchmode** 必须先 `TTTInitializeCaller.Initialize()`，否则 ColorFill NRE。普通 Editor `delayCall` 通常已经 init。

## 设置

- JSON：`Library/OVA/settings.json`（工程内，gitignore）
- PAT：`Library/OVA/secrets.json`（永不进 git / 永不进 gist）
- 组件上的 `OvaSettings` 只是 JSON 不存在时的后备
- 默认种子 `5145514`（AvatarObfuscator 同款）。`0` = 每次随机
- `preserveNameSubstrings` 默认 `"Go/,FT,eye,VRCEmote,VrcDcc/"` — **只** Hierarchy/blendshape
- 参数：`parameters.extraPreserve` + `pinPreserve` + 自动提示 + VRC 保留 + PhysBone 前缀

## 水印（L7）

- 算法：对每个顶点用 `seed` 和 index 做 hash，±amplitude 偏 basis（默认 1e-5）
- 归因，不是加密。AssetRipper 仍看得到网格
- 贴图 **不要 LSB**（VRC 压缩会碾）
- 上传后是否还在：**未证**（人点 SDK 之后才能证）

## Attest（停）

Hop A：本机 ova-web 算 `ova-fp-v1` = SHA-256(seed + nameLength + watermark on/off + amplitude)  
Hop B：GitHub/Gitee Contents API — **501，Owner 停**（「进 VRC 远程验证」不是头像能做的事）  
Hop C：观看者 HTTP — **永远不是这个功能**

指纹会碰撞（同样四个字段）。不是网格内容哈希，不是解密钥。

## 抛掷工程

- `D:\Project\Unity\test`，场景 `Assets/OVA/RobotBake.unity`
- SDK Robot Avatar + `OVA_AAO_Merge` + `OVA_TTT_Atlas`
- 驱动 `Assets/OVA/Editor/OvaThrowawayBake.cs`（不进 VPM；`dataPath` 必须含 `/test/`）
- Unity MCP：仅当 8080 属于 test。禁止写入用户全局 `mcp.json`。Kaguya 若占用 8080 必须 abort。
