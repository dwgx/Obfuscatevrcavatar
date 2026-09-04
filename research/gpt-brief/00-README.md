# GPT 研究包 — OVA 蓝队

给 **GPT 5.6 Sol Ultra Pro**（或任何推理模型）用的只读包。  
路径：`D:\Project\Obfuscatevrcavatar\.agent\gpt-pack\`  
不进 git（`.agent/` 已 ignore）。不要把 SaoMoLa / 驱动 / 扫钥源码塞进这个目录。

## 怎么丢给 GPT

1. 先贴 `01-PROMPT.md` 当系统/首条指令。
2. 再上传本文件夹其余 `.md` / `.json` / `.jsonl`。
3. 模型只准输出：**方案想法、对比、风险、和 OVA 现码的差距**。  
   不准输出：提取步骤、解密 PoC、驱动利用、热替换别人、扫 key。
4. 把模型的「想法清单」原样带回 Owner 的 OVA 产品窗（Grok / 实现代理）。不要让 GPT 直接改这个仓库。

## 文件

| 文件 | 作用 |
|---|---|
| `01-PROMPT.md` | 首条指令 + 停线 + 要交的作业 |
| `02-ova-now.md` | 产品是什么、怎么构建、现码能做什么 |
| `03-theory.md` | 第一定律、三把钥、Path A/B、L0–L7 |
| `04-pipeline.md` | NDMF 顺序、保留表、ova-web |
| `05-evidence.md` | 2026-09-04 抛掷/SDK 证据与开放项 |
| `06-peers.md` | 别人的工具（GitHub / VPM / 商店） |
| `07-il2cpp-re.md` | 本机 `vrchat-il2cpp-re` 是什么（防御手册，不是头像插件） |
| `08-saomola-classes.md` | 红队 **类别** 证明存在；禁止抄代码 |
| `09-questions.md` | 请 GPT 逐条想的问题 |
| `sources.json` | 一手 URL |
| `claims.jsonl` | 可核对的断言（status 词） |
| `manifest.json` | 清单 |

## 停线（复制进 GPT）

- 蓝队。不写 ripper、不写扫钥、不写内核驱动、不写热替换别人。
- 不声称 100% 防盗。观看者要画模，进程/GPU 必须有明文几何。
- 不把 OSC / 32bit 同步钥说成「防 SaoMoLa」。那是作者钥，检测器会指纹。
- 不建议官方名单没有的 lilToon 顶点锁。
- 不建议把 OVA 装进封存中的辉夜 `D:\Project\Unity\kaguya`。
- 人点 VRChat SDK Publish。代理不点。
