# Watermark attestation (GitHub / Gitee)

Architecture only in this drop. ova-web can save coordinates and a local PAT, compute a fingerprint, and show what a publish *would* send. It does **not** call GitHub or Gitee yet.

## Who talks (author PC)

“Fingerprint HTTP” is **two different HTTP paths**. Do not mix them.

| Hop | Where | Today |
|---|---|---|
| A. Local console | Browser on the **author’s machine** → `http://127.0.0.1:17849/` (`ova-web` in Unity, or `preview.ps1`) | **Live.** Computes fingerprint, stores PAT, returns `wouldPost`. |
| B. Registry publish | Same machine → **GitHub or Gitee HTTPS** Contents API | **Not wired** (`POST /api/attest/publish` → 501). |
| C. VRChat / OSC / viewers | Watcher clients, Expression params, in-world Udon | **Never this feature.** Uploaded avatars have no author C#. |

Hop A is loopback control. Hop B is outbound attribution. Viewers do not poll either hop to draw the avatar.

## What is published

A public JSON blob with:

- `algo`: `ova-fp-v1`
- `fingerprint`: SHA-256 hex of seed + name length + watermark on/off + amplitude

That is an identity handle for later attribution (“this bake used these settings”). It is **not** a decrypt key, not a vertex-lock key, not a mesh content hash, and not a PAT. Two avatars with the same four fields collide.

## Wire notes (when Hop B is implemented)

**GitHub** ([Create or update file contents](https://docs.github.com/en/rest/repos/contents)): `PUT /repos/{owner}/{repo}/contents/{path}`. Body: `message`, Base64 `content`, `branch`; `sha` required if the file already exists. Classic PAT: `repo`. Fine-grained: repository **Contents: write**. Header: `Authorization: Bearer <token>` (or `token`). Prefer a **public** fingerprint repo if the point is takedown evidence.

**Gitee v5**: create `POST /v5/repos/{owner}/{repo}/contents/{path}`, update `PUT` (swagger UI: `https://gitee.com/api/v5/swagger`). Many clients send `access_token` in the **JSON body or query**, not a GitHub-style Authorization header. Do not copy GitHub auth blindly. Content is still Base64. Token still never goes into the public blob.

Never: PAT in gist JSON, decrypt keys, OSC bits, `_BitKey` / `DexProtect` / `_IsLocked`.

## What this can grow into (ceiling)

Remote verify in OVA is a **registry**, not a runtime license.

| Stage | What it is | Ceiling |
|---|---|---|
| Now | Local `ova-fp-v1` + 501 stub | Settings identity only |
| Next | Hop B: public JSON on GitHub/Gitee | Humans (you, takedown) can look up “this bake was registered” |
| Later | Optional `handle` / username in a **new** algo (`ova-fp-v2`) | Still baked at author PC; collision drops if handle is unique |
| Later | Bake-time mesh/watermark digest in the same public JSON | Stronger attribution; still not courtroom-proof after retopo; still not a viewer gate |
| Out of OVA | Wearer OSC companion (ShellProtector class) | Wearer-PC license theater; Quest/OSC-off fail; rip skips your server |
| Impossible | Avatar or watchers must HTTP before the body draws | No author C# on avatars; watchers already hold plaintext |

Do not grow an in-avatar DRM story. Grow a **lookup**: fingerprint (+ later handle/digest) on a public gist/repo, ova-web “试发布” becomes a real PUT, a static page or gist search for “does this hex match my registry.”

## What never leaves the machine

- GitHub / Gitee tokens (`Library/OVA/secrets.json`, gitignored with `Library/`)
- Settings crypto keys, if those are added later
- Anything that would unlock a mesh or texture

`GET /api/attest/status` returns `hasGithubSecret` / `hasGiteeSecret` booleans. It never returns the token.

## Settings vs secrets

| File | Contains |
|---|---|
| `Library/OVA/settings.json` | `attest.provider` (`off` / `github` / `gitee`), owner, repo, branch, path |
| `Library/OVA/secrets.json` | `githubToken`, `giteeToken` |

ova-web **Save** writes settings only. **写入本机令牌** POSTs `/api/attest/secret`.

## HTTP (localhost)

| Method | Path | Notes |
|---|---|---|
| GET | `/api/attest/status` | metadata + secret flags + `wired: false` |
| GET | `/api/attest/fingerprint` | current `ova-fp-v1` hex |
| POST | `/api/attest/secret` | `{ "provider": "github"\|"gitee", "token": "..." }`. Empty token clears. |
| POST | `/api/attest/publish` | **501** `publish-not-wired` plus `wouldPost` |

Next wire (not this commit): GitHub Contents API `PUT /repos/{owner}/{repo}/contents/{path}` and Gitee `POST /api/v5/repos/{owner}/{repo}/contents/{path}` with a public fingerprint JSON. The PAT stays in `secrets.json` and is only used as an `Authorization` header on that request.

## Fingerprint payload

UTF-8, then SHA-256, lowercase hex:

```
ova-fp-v1
seed={int}
nameLength={int}
watermark.enabled={0|1}
watermark.amplitude={single round-trip, invariant culture}
```
