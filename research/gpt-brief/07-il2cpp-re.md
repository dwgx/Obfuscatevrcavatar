# vrchat-il2cpp-re — 本机库，不是 OVA

路径：`D:\Project\vrchat-il2cpp-re`

## 它实际是什么

Unity 6 IL2CPP + Beebyte **客户端** 逆向管线：从 dump 恢复类型名、给 GameAssembly 里被改成 `ÌÍÎÏ` 的符号改回可读、出 `output/src/` 和 IDA map。

主业是 **观看者客户端的元数据改名**，不是「给别人内存做头像混淆」，也不是 NDMF 插件。

同形字字母表和 OVA/AvatarObfuscator 用的是同一类视觉 trick，但 **作用域完全不同**：

| | Beebyte in GameAssembly | OVA on avatar clone |
|---|---|---|
| 改谁 | 客户端方法/类型名 | 头像 Hierarchy/参数/资源名 |
| 谁看见 | 逆向的人 | 拆包后的 Unity 工程 |
| 防盗？ | 不防；是 VRChat 自己的混淆 | 劳动层 |

## 蓝队该吸收的（手册）

文件：`output/p2_research/avatar_ripping_defense_playbook.md`（防御参考，刻意省略现成盗窃工具）。

已吸收进 OVA 文档的：

- §0 第一定律（明文几何）
- 拆包向量分类：cache 提取、post-decrypt harvest、GPU、heap hook、bundle key、MITM…
- **纠正**：头像 bundle 密码在 `UnityPlayer` `EncryptionKey*`，不是 Photon / libsodium / `vrc_fast_crypto`
- 作者侧 §5：Shader lock 低、32bit mesh obfuscation 中（钥可被同实例嗅）、水印只归因
- 残余：本机 owner 关 EAC 的 local rip、GPU 抓帧、离线 cache — 作者插件补偿不了

**不要吸收进 GPT 方案的：**

- 具体 RVA / hook 点（那是客户端完整性研究，写成菜谱就变红队）
- 「在 OVA 里做 prologue hash / 注入检测」——头像没有作者 C#
- 「改观看者 GameAssembly 把 Mesh 变乱」——不是 VPM 包

## 对想法的用法

用手册当 **否决器**：凡是需要改观看者进程、藏平台钥、防 GPU 的，写进「不该做」。  
用手册当 **归因器**：水印、上传溯源是平台/作者仅剩的诚实工具。
