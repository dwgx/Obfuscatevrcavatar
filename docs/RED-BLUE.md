# Author unpack as red/blue

OVA does not put a decrypt key in the uploaded avatar. The author can
AssetRipper **their own** `.vrca` the same way a watcher can after cache
harvest. That unpack **is** the red team. The NDMF clone from
`AvatarProcessor.ManualProcessAvatar` is the same class of evidence
without an upload (Editor-only components already destroyed).

Do not confuse this with license servers, OSC locks, or in-VRC HTTP.
Those are not in the bundle. Post-decrypt harvest does not need an
author key ([THREAT-MODEL.md](THREAT-MODEL.md) three keys).

## Loop

1. Bake (NDMF clone or human SDK upload → unpack).
2. List what is still **cheap** (readable shop names, split meshes, un-atlased textures).
3. Patch OVA (or compose AAO/TTT/Shell) for that class.
4. Bake again. Cheap list should shrink. Geometry stays plaintext — that is not a bug.

Agent does not click SDK Publish. Agent does not copy SaoMoLa.

## Cheap paths (Robot throwaway, 2026-09-04)

| After unpack, still cheap? | Class | OVA / compose |
|---|---|---|
| Hierarchy shop names | L2 names | name pass (Armature/Humanoid/viseme kept on purpose) |
| Blendshape shop names | L2 | name pass; viseme tokens kept so lip sync works |
| `FT` infix (`blink_left`, `Gift`) | L2 keep-list | **closed** on Robot bake (token-boundary) |
| Mesh / clip / texture **asset** `.name` | L2 assets | **closed** on Robot bake (`OvaSharedAssetPass`) |
| Separate clothes meshes / per-piece textures | L1 | AAO merge **closed** on Robot (2 SMR → 1). TTT atlas **closed** on the two Body maps (clone mats share one souped 2048 tex). Not split-clothes proof |
| Material slots readable | L2 | cloned mat names (already) |
| Animation still drives soup shapes | L2 | VirtualClip blendShape rewrite |
| Synced-layer BlendTree param | L2 | `MapParameters` + `RewriteMotion` in code; **unproven** — Robot has no override |
| Vertex positions | L7 watermark only | keyed ±1e-5; not a lock |
| `_BitKey` / `DexProtect` / `_IsLocked` | detector bait | must stay absent |
| Author decrypt key in bundle | L3/L4 | **do not add** |

## Concepts to iterate (not a fifth constitution)

- **Labor, not secrecy.** Source in this repo does not make a rip free; readable names do.
- **Keep lists are the product.** Over-keep (`FT` inside `left`) is a red-team gift.
- **Clone-only.** Source scene stays the author's working names.
- **Compose for L1/L3.** OVA does not reimplement atlas or lilToon decrypt.
- **Watermark is attribution.** Unpack will still show a mesh. Survival after VRC compress is a later upload test.
- **No in-avatar key.** Own unpack without a key is the intended threat model.

Next: human SDK Publish on RobotBake (Owner). Agent does not click Publish. Do not send `execute_code` to Kaguya `:8080`.
