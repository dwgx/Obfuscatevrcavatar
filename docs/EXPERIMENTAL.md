# Experimental / not-yet (author layer)

Not a promise. NDMF clone + ova-web only. No custom C# in the uploaded avatar.
Synced Expression parameters are visible to anyone who can draw the avatar.
SaoMoLa / il2cpp-re: classes only. Unity MCP is not required for this list.

Throwaway on disk: `D:\Project\Unity\test` (do not use Kaguya).
2026-09-04 lock: Unity `2022.3.22f1`, SDK Base/Avatars `3.10.4`, MA `1.18.7`,
NDMF `1.14.8`, lilToon `2.3.4`, Av3Emulator `3.4.13`. OVA **is** installed (`dev.ova.protection` 0.2.0 in `test`). Author-unpack loop: [RED-BLUE.md](RED-BLUE.md).
`Packages/manifest.json` is Unity modules only; VPM lives in `vpm-manifest.json`.

Local listing is **0.2.0** (`source.json` + `Releases/dev.ova.protection-0.2.0.zip`).
A 2026-09-04 market scan still said 0.1.0 — that claim is stale.

## Verify (identity), not a lock

| Idea | Channel | Do | Don't |
|---|---|---|---|
| GitHub/Gitee fingerprint JSON | HTTPS from **author PC** at bake/publish | **OVA attest** — public `ova-fp-v1` only | PAT / decrypt key on gist |
| OSC password | Wearer machine → avatar params | Compose **ShellProtector** | Reimplement OSC in OVA |
| Menu / "password roulette" | Expression menu ints | Optional later: labor + hide states | Call it verify or anti-rip |
| In-world "verify then grant mesh" | Avatar runtime | Impossible: upload has no author C# / GitHub | Fake a remote unlock |
| Per-avatar username + **VRC switch → remote HTTP** | Avatar bundle | **No.** Username can be **baked** (fingerprint / watermark seed material). Switch-time HTTP is not in the avatar | Treat OSC companion as if it were NDMF |

**Username baked at NDMF:** feasible as a string in settings → `ova-fp-v1` (or a later algo). Static. Does not re-check when the wearer changes avatars in VRChat.

**Switch → remote verify inside the uploaded avatar:** not feasible. VRChat does not run author C# or Udon HTTP on avatars. Worlds can `VRCImageDownloader`; avatars cannot.

**Wearer-PC companion (not OVA core):** VRChat OSC, if enabled, sends `/avatar/change` with `avtr_…` on local load ([docs](https://docs.vrchat.com/docs/osc-avatar-parameters)). A **desktop** app on that PC can HTTP your server, then OSC-drive an unlock param. Official, local-only. Requires OSC on + the app running. Watchers still get plaintext once drawn. Rip / cache harvest does not talk to your server. Same class as ShellProtector OSC, not attest.

**Recommendation:** verify = attest HTTP. OSC = vendor compose. Roulette = ungrilled, default **off**.

Empty shells that are **not** verify: `POST /api/attest/publish` → 501;
`encryptSettingsAtRest` unused; `textureMode=ova` reserved unused.

## Memory (viewer), NDMF cannot paint

Playbook §0: viewer process/GPU holds plaintext Mesh. Cache AES is the
**viewer's** key. Author plugin never reaches `GameAssembly` / `UnityPlayer`.
Post-decrypt harvest does not need the author key. GPU capture sits below the game.

Three keys (do not mix): viewer cache AES; Unity AssetBundle cipher in the
engine; optional author OSC/shader unlock. `vrc_fast_crypto` / Photon /
libsodium are **not** the avatar-bundle key.

SaoMoLa READMEs prove cache-key + post-decrypt harvest classes exist. They do
**not** document parameter multiplexing, and they do **not** claim a verified
avatar GPU mesh-grab product. Detector strings: `_BitKey` / `DexProtect` /
`_IsLocked`. Stay out of SaoMoLa `scripts/hotswap`, `scripts/extraction`,
`scripts/drivers`, `drivers/`.

Experimental that stays out: kernel, physmem, cache decrypt, hotswap-other-people,
32-bit vertex lock on lilToon.

## Ranked labor (clone) — still to mine

1. ~~Mesh / AnimationClip **asset** names~~ (2026-09-04 labor-meshname)
2. Synced-layer BlendTree params on overrides — rewritten; needs an avatar that has one
3. ~~Optional decoy blendshapes~~ (2026-09-04) `decoyBlendShapeCount=8` on Robot clone; visemeMiss=0
4. Human SDK upload then author-unpack ([RED-BLUE.md](RED-BLUE.md))
5. ~~AAO + TTT on the throwaway~~ (2026-09-04) merge 2→1 SMR; TTT atlas shares one tex on the two clone mats

Roulette / menu lock stay Animator-only **if** Owner names them. Treat as UX
friction, not a secret.

## Fingerprints we will not emit

`_BitKey0..31`, `DexProtect`, `_IsLocked`. Grep clean in the OVA package 2026-09-04.
