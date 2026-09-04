import { zipStore, downloadBytes } from "./zip.js";

const CONSTRAINTS = `# OVA_CONSTRAINTS_v1

You are helping configure ova-web (dev.ova.protection), a VRChat NDMF
name/watermark plugin. Success = unpacked Unity project is labor, plus a
keyed vertex watermark. Not 100% anti-rip.

Hard rules:
- Reply with ONE fenced block: ova-params-v1 containing JSON only.
- Only known OvaSettings keys. Unknown keys are dropped.
- Never emit _BitKey, DexProtect, _IsLocked, or any runtime unlock key.
- Do not turn attest.provider on unless the human already has a repo.
- preserveNameSubstrings: FT is a token (FT_Blink stays, Gift / blink_left rename).
- Viseme names (vrc.* and v_aa / v_sil / …) stay readable. That is required.
- Humanoid / Armature / optional MMD Body stay readable.
- textureMode compose means ShellProtector / TTT / Ajisai — do not invent OVA texture crypto.
- ui.locale is en | zh-CN. ui.mode is normal | expert.
- Do not ask for a Unity project dump. If context is large, ask the human
  to send ova-pack-v1.zip (this archive) instead of pasting the scene.

JSON shape matches 02-settings.json in this zip.
`;

export function parseParams(text) {
  if (!text || !String(text).trim()) return null;
  const raw = String(text).trim();
  const fence = raw.match(/```(?:ova-params-v1|json)\s*([\s\S]*?)```/i);
  const blob = fence ? fence[1] : raw;
  const start = blob.indexOf("{");
  const end = blob.lastIndexOf("}");
  if (start < 0 || end <= start) return null;
  try {
    const obj = JSON.parse(blob.slice(start, end + 1));
    if (!obj || typeof obj !== "object" || Array.isArray(obj)) return null;
    const keys = [
      "seed", "nameLength", "names", "watermark", "parameters", "assets",
      "crypto", "attest", "ui", "preserveNameSubstrings", "preserveMmd",
      "autoDetectPreserve", "version"
    ];
    if (!keys.some((k) => Object.prototype.hasOwnProperty.call(obj, k))) return null;
    return obj;
  } catch {
    return null;
  }
}

export function buildBrief({ settings, scene, fingerprint, projectNote }) {
  const s = settings || {};
  const sc = scene || {};
  const names = s.names || {};
  const lines = [
    "# OVA_BRIEF_v1",
    "",
    "Plugin: OVA / dev.ova.protection (NDMF, clone-only).",
    "Ask: suggest OvaSettings. Reply with ```ova-params-v1 JSON.",
    "",
    "## Project note",
    projectNote && String(projectNote).trim() ? String(projectNote).trim() : "(none)",
    "",
    "## Scene probe (not a Unity dump)",
    `- unity: ${!!sc.unity}`,
    `- avatar: ${sc.hasAvatar ? (sc.avatar || sc.note || "yes") : "none"}`,
    `- transforms: ${sc.transformsRename || 0} / ${sc.transforms || 0} will rename`,
    `- blendshapes: ${sc.blendshapesRename || 0} / ${sc.blendshapes || 0} will rename`,
    `- animator layers: ${sc.animatorLayers || 0}`,
    `- expression/animator items: ${(sc.items && sc.items.length) || 0}`,
    "",
    "## Fingerprint",
    fingerprint || "(none)",
    "",
    "## Current settings",
    "```json",
    JSON.stringify(s, null, 2),
    "```",
    "",
    "## Layers on",
    `- hierarchy: ${!!names.obfuscateHierarchy}`,
    `- blendshapes: ${!!names.obfuscateBlendShapes}`,
    `- decoy blendshapes: ${Number(names.decoyBlendShapeCount) || 0}`,
    `- animator layers: ${!!names.obfuscateAnimatorLayers}`,
    `- states: ${!!names.obfuscateStates}`,
    `- parameters: ${!!(s.parameters && s.parameters.obfuscate)}`,
    `- cloned assets: ${!!(s.assets && s.assets.obfuscateClonedNames)}`,
    `- watermark amp: ${(s.watermark && s.watermark.enabled) ? s.watermark.amplitude : "off"}`,
    `- textureMode: ${(s.crypto && s.crypto.textureMode) || "compose"}`,
    `- attest: ${(s.attest && s.attest.provider) || "off"}`,
    "",
    "Read 05-CONSTRAINTS.md before editing JSON."
  ];
  return lines.join("\n");
}

export function buildZipFiles(ctx) {
  const settings = ctx.settings || {};
  const scene = Object.assign({}, ctx.scene || {});
  if (Array.isArray(scene.items) && scene.items.length > 80) {
    scene.items = scene.items.slice(0, 80);
    scene.itemsTruncated = true;
  }
  return [
    { name: "00-README.txt", body: "English pack for an LLM. Reply with one ```ova-params-v1 JSON block. Human pastes it into ova-web → Apply.\n" },
    { name: "01-BRIEF.md", body: buildBrief(ctx) },
    { name: "02-settings.json", body: JSON.stringify(settings, null, 2) + "\n" },
    { name: "03-scene.json", body: JSON.stringify(scene, null, 2) + "\n" },
    { name: "04-fingerprint.txt", body: (ctx.fingerprint || "") + "\n" },
    { name: "05-CONSTRAINTS.md", body: CONSTRAINTS }
  ];
}

export async function copyBrief(ctx) {
  await navigator.clipboard.writeText(buildBrief(ctx));
}

export function downloadPack(ctx) {
  const bytes = zipStore(buildZipFiles(ctx));
  const slug = (ctx.scene && ctx.scene.avatar ? ctx.scene.avatar : "ova")
    .replace(/[^\w.-]+/g, "_")
    .slice(0, 48);
  downloadBytes(`ova-pack-v1-${slug}.zip`, bytes, "application/zip");
}
