# VCC 和 unitypackage

## VCC（VRChat Creator Companion）

VCC 管的是 **VPM 包**：每个包一个 `package.json`（名字、版本、Unity 版本、`vpmDependencies`）。

1. 做一个 **listing**（本仓库根目录 `source.json`）。发布后是一个 HTTPS JSON；本地开发可以用文件路径。
2. VCC → Settings → Packages → Add Repository → 贴 listing URL。
3. 打开你的 **头像 Unity 工程** → Manage Project → 安装 `OVA Avatar Protection`。
4. 包出现在工程 `Packages/dev.ova.protection/`，和 Modular Avatar 同一层，**不是** `Assets/` 垃圾堆。

依赖写在 `vpmDependencies` 里（NDMF）。VCC 会去拉。不要把 NDMF 再当 unitypackage 装第二份。

## unitypackage

Unity 菜单 `Assets → Import Package → Custom Package`。  
AvatarObfuscator v0.4.9 就是这种：解压进 `Assets/dev.cocokoishi.avatar-obfuscator/`。

优点：不会用 VCC 的人也能装。  
缺点：版本和 NDMF 要对你自己负责；和 VPM 包容易装双份。

OVA **先做 VPM**。以后 `scripts/pack-unitypackage.ps1` 再打一份给 Booth/QQ。

## 在本机试装（test 工程，不要辉夜）

抛掷工程现在是 **`D:\Project\Unity\test`**（2022.3.22f1，SDK Avatars 3.10.4，MA 1.18.7，NDMF 1.14.8，lilToon 2.3.4）。还没装 OVA。不要用 Unity MCP 去点（改模窗还占着）。

**做法 0 — VCC User Packages（推荐，不用 zip）：**  
VCC → 设置 → Packages → **User Packages** → Add → 选文件夹  
`D:\Project\Obfuscatevrcavatar\Packages\dev.ova.protection`  
回到 `test` 的「管理包」，应出现 **OVA Avatar Protection 0.2.0** → 安装。

**做法 A — UPM Add from disk（Unity 里，人点）：**  
Package Manager → Add package from disk →  
`D:\Project\Obfuscatevrcavatar\Packages\dev.ova.protection\package.json`

**做法 B — VCC Community listing（本地 JSON）：**  
已跑 `scripts\pack-vpm.ps1`，listing 是 0.2.0 zip。VCC → 设置 → Packages → Add Repository，贴：

```
file:///D:/Project/Obfuscatevrcavatar/source.json
```

有的 VCC 不吃 `file://`；那时用做法 0 或 A。装进 **test**，不要辉夜。

装完：头像根加 **OVA Protection** → 菜单 **OVA → 打开 ova-web**。人点 SDK。代理不点 Publish。

## Private GitHub（源码可以私有；VCC 官方 listing 默认是公开站）

可以。源码放 **private** GitHub 没问题。官方 VCC listing 模板（[template-package-listing](https://github.com/vrchat-community/template-package-listing)）写的是：建仓可选 Private，但 Pages 发布的 `https://<user>.github.io/<repo>/index.json` 是给社区用的 **公开 HTTPS**。那条路等于把 zip 地址暴露出去。

VCC **2.1+** 加仓库：[Community Repositories](https://vcc.docs.vrchat.com/guides/community-repositories/)

1. Settings → Packages → Add Repository，贴 listing 的 HTTPS URL（不是 git clone URL，不是 `package.json`）。
2. 私有 listing：URL 框旁边 **齿轮** → 自定义 Header。常见是 `Authorization` = `Bearer <PAT>`。
3. 确认弹窗 → I Understand, Add Repository。工程 Manage Project 里才能看到包。
4. CLI 等价：`vpm add repo <path-or-url>`（本地 json **可以**，官方写了绝对路径）。

不要把 PAT 写进仓库里的 `source.json`。Token 只活在本机 VCC `settings.json` 的 `userRepos[].headers`。

**坑（VCC 维护者在 issue 里写明的）：** 齿轮 Header 往往只够把 **listing JSON** 拉下来。GitHub **private Release zip** 的浏览器下载链通常不认同一套 Header；要走 `api.github.com/repos/.../releases/assets/<id>` 且 `Accept: application/octet-stream`。把 PAT 配进 VCC 去下 GitHub 私有资产，等于 token 长期躺在本机配置里，**不适合当「给别人装的私有商店」**。结论：private GitHub 当源码仓可以；当远程 VPM 源，官方社区不推荐。

Owner 自己用，按便宜程度：

| 做法 | 何时 |
|---|---|
| UPM Add from disk → `Packages/dev.ova.protection/package.json` | 最稳，现在就能装 |
| clone 私有仓后 `pack-vpm.ps1`，VCC 加本地 `source.json` 或 `vpm add repo` | 要 VCC 版本列表、仍不上网 |
| 公开 GitHub Pages listing（模板仓） | 以后真的要给别人装 |
| VCC 齿轮 + PAT + GitHub private zip | 不作为默认；要自建带鉴权的 listing/zip 代理 |

建仓时不要把辉夜工程、SaoMoLa、SDK cookie 推进去。zip 仍用现有 `scripts/pack-vpm.ps1`（须带 `zipSHA256`）。

3. 头像根加组件 `OVA Protection`。NDMF 构建日志应有 `[OVA] name pass:`（v0.1）。
4. 人点 SDK。Agent 不点 Publish。

离线语法检查（不装进辉夜）：`powershell -File scripts\compile-check.ps1`。引用 Kaguya 工程里 Unity 生成的 NDMF Editor csproj HintPath（`unity-4.8-api` + `Managed\UnityEngine\UnityEditor.dll`）。产出在 `Temp/compile/`，不要拷进 `kaguya/Assets`。
