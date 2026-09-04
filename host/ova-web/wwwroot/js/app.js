import { t, setLocale, locale, detectLocale, applyI18n } from "./i18n.js";
import { copyBrief, downloadPack, parseParams } from "./pack.js";

const $ = (id) => document.getElementById(id);

let uiMode = "expert";
let currentPage = "overview";
let substrs = [];
let extras = [];
let pins = new Set();
let sceneItems = [];
let sceneMeta = {
  unity: false,
  hasAvatar: false,
  note: "",
  avatar: "",
  physbonePrefixes: [],
  transforms: 0,
  transformsRename: 0,
  blendshapes: 0,
  blendshapesRename: 0,
  animatorLayers: 0
};
let lastBuild = null;
let paramFilter = "all";
let savedJson = "";

function parseCsv(s) {
  return (s || "").split(/[,;]/).map((x) => x.trim()).filter(Boolean);
}

function joinCsv(list) {
  return list.join(",");
}

function toast(message, ok = true) {
  const el = $("snack");
  $("snack-text").textContent = message;
  el.hidden = false;
  el.classList.toggle("err", !ok);
  requestAnimationFrame(() => el.classList.add("show"));
  clearTimeout(toast._t);
  toast._t = setTimeout(() => {
    el.classList.remove("show");
    setTimeout(() => { el.hidden = true; }, 200);
  }, 2200);
}

function moveRailPill() {
  const pill = $("rail-pill");
  const active = document.querySelector(".dest.active");
  const rail = document.querySelector(".rail");
  if (!pill || !active || !rail) return;
  const r = rail.getBoundingClientRect();
  const a = active.querySelector(".indicator").getBoundingClientRect();
  pill.style.transform = `translateY(${a.top - r.top}px)`;
}

function keepLabel(k) {
  return t("keep." + k) || k;
}

function refreshChrome() {
  applyI18n();
  document.querySelectorAll("[data-locale]").forEach((b) => {
    b.classList.toggle("is-on", b.dataset.locale === locale());
  });
  document.querySelectorAll("[data-mode-set]").forEach((b) => {
    b.classList.toggle("is-on", b.dataset.modeSet === uiMode);
  });
  const meta = { title: t("page." + currentPage + ".title"), sub: t("page." + currentPage + ".sub") };
  $("page-title").textContent = meta.title;
  $("page-sub").textContent = meta.sub;
  moveRailPill();
}

function applyMode(next, fromUser) {
  uiMode = next === "normal" ? "normal" : "expert";
  document.body.dataset.mode = uiMode;
  if (uiMode === "normal" && (currentPage === "params" || currentPage === "attest")) {
    showPage("overview");
  } else {
    refreshChrome();
  }
  if (fromUser) markDirty();
}

function showPage(id) {
  if (uiMode === "normal" && (id === "params" || id === "attest")) id = "overview";
  currentPage = id;
  for (const page of document.querySelectorAll(".page")) {
    page.hidden = page.id !== "page-" + id;
  }
  for (const btn of document.querySelectorAll(".dest")) {
    btn.classList.toggle("active", btn.dataset.page === id);
  }
  refreshChrome();
  if (id === "overview") renderOverview();
  if (id === "params") loadScene().catch((e) => toast(String(e.message || e), false));
  if (id === "pack") syncRawJson();
}

function radioValue(name) {
  const radios = document.querySelectorAll(`md-radio[name="${name}"]`);
  for (const el of radios) {
    if (el.checked) return el.value;
  }
  return "off";
}

function setRadio(name, value) {
  document.querySelectorAll(`md-radio[name="${name}"]`).forEach((el) => {
    el.checked = el.value === value;
    const card = el.closest(".provider-card");
    if (card) card.classList.toggle("is-on", el.checked);
  });
}

function fmtAmp(n) {
  const x = Number(n);
  if (!Number.isFinite(x)) return "0.00001";
  return x.toFixed(8).replace(/0+$/, "").replace(/\.$/, "");
}

function setLamp(id, on) {
  const el = $(id);
  if (el) el.classList.toggle("on", !!on);
}

function syncTiles() {
  document.querySelectorAll(".check-tile").forEach((tile) => {
    const box = tile.querySelector("md-checkbox");
    tile.classList.toggle("is-on", !!(box && box.checked));
  });
}

function renderChips(host, values) {
  host.innerHTML = "";
  values.forEach((v) => {
    const b = document.createElement("button");
    b.type = "button";
    b.className = "chip";
    b.textContent = v + " ×";
    b.addEventListener("click", () => {
      if (host.id === "substr-chips") {
        substrs = substrs.filter((x) => x !== v);
        renderChips(host, substrs);
      } else {
        extras = extras.filter((x) => x !== v);
        renderChips(host, extras);
      }
      markDirty();
      renderParamList();
      renderOverview();
    });
    host.appendChild(b);
  });
}

function addChip(kind) {
  const field = kind === "substr" ? $("substrAdd") : $("extraAdd");
  const v = (field.value || "").trim();
  if (!v) return;
  if (kind === "substr") {
    if (!substrs.includes(v)) substrs.push(v);
    renderChips($("substr-chips"), substrs);
  } else {
    if (!extras.includes(v)) extras.push(v);
    renderChips($("extra-chips"), extras);
  }
  field.value = "";
  markDirty();
  renderParamList();
  renderOverview();
}

function setStat(id, text, on) {
  const el = $(id);
  if (!el) return;
  el.textContent = text;
  el.classList.toggle("is-on", !!on);
}

function setText(id, text) {
  const el = $(id);
  if (el) el.textContent = text;
}

function fmtUnknown(n) {
  if (typeof n !== "number" || !Number.isFinite(n) || n < 0) return t("overview.unknown");
  return String(n);
}

function lastBuildMissing(res, data) {
  if (!res || res.status === 404) return true;
  if (data && typeof data.error === "string" && data.error.toLowerCase() === "not found") return true;
  return false;
}

function renderLastBuild() {
  const empty = $("ov-bake-empty");
  const grid = $("ov-bake-grid");
  const help = document.querySelector(".bake-wm-help");
  if (!empty || !grid) return;
  if (!lastBuild) {
    empty.hidden = false;
    empty.textContent = t("overview.neverBaked");
    grid.hidden = true;
    if (help) help.hidden = true;
    return;
  }
  empty.hidden = true;
  grid.hidden = false;
  if (help) help.hidden = false;
  setText("ov-bake-avatar", lastBuild.avatar || t("overview.unknown"));
  setText("ov-bake-at", lastBuild.atUtc || t("overview.unknown"));
  setText("ov-bake-decoy", fmtUnknown(lastBuild.decoyAdded) + " / " + fmtUnknown(lastBuild.decoyBudget));
  setText("ov-bake-viseme", fmtUnknown(lastBuild.visemeMiss));
  setText("ov-bake-smr", fmtUnknown(lastBuild.origSmr) + " / " + fmtUnknown(lastBuild.cloneSmr));
  setText("ov-bake-body", lastBuild.bodyGo === true ? t("overview.yes") : lastBuild.bodyGo === false ? t("overview.no") : t("overview.unknown"));
  setText("ov-bake-params", fmtUnknown(lastBuild.parameterRenamed));
  setText("ov-bake-wm-meshes", fmtUnknown(lastBuild.watermarkMeshes));
  setText("ov-bake-wm-status", t("overview.wmStatusValue"));
  setText("ov-bake-lock", lastBuild.lockFingerprints === true ? t("overview.yes") : t("overview.no"));
}

async function loadLastBuild() {
  lastBuild = null;
  try {
    const res = await fetch("/api/last-build");
    const text = await res.text();
    let data = null;
    try { data = text ? JSON.parse(text) : null; } catch { data = null; }
    if (!lastBuildMissing(res, data) && res.ok && data && data.schema === "ova-build-report-v1") {
      lastBuild = data;
    }
  } catch {
    lastBuild = null;
  }
  renderOverview();
}

function layerStat(on, rename, total) {
  if (!on) return t("stat.off");
  if (!sceneMeta.hasAvatar && !sceneMeta.unity && !sceneItems.length && !sceneMeta.transforms) {
    return t("meta.none");
  }
  if (typeof rename === "number" && typeof total === "number" && total > 0) {
    return t("stat.willChange", { n: rename, total });
  }
  if (typeof rename === "number") return t("stat.willChangeN", { n: rename });
  return t("stat.on");
}

function mergeSettings(base, patch) {
  const out = Object.assign({}, base, patch);
  out.names = Object.assign({}, base.names || {}, patch.names || {});
  out.parameters = Object.assign({}, base.parameters || {}, patch.parameters || {});
  out.assets = Object.assign({}, base.assets || {}, patch.assets || {});
  out.watermark = Object.assign({}, base.watermark || {}, patch.watermark || {});
  out.crypto = Object.assign({}, base.crypto || {}, patch.crypto || {});
  out.attest = Object.assign({}, base.attest || {}, patch.attest || {});
  out.ui = Object.assign({}, base.ui || {}, patch.ui || {});
  return out;
}

function packCtx() {
  return {
    settings: toSettings(),
    scene: Object.assign({}, sceneMeta, { items: sceneItems }),
    fingerprint: ($("fingerprint") && $("fingerprint").textContent) || ($("ov-fp") && $("ov-fp").textContent) || "",
    projectNote: ($("projectNote") && $("projectNote").value) || ""
  };
}

function syncRawJson() {
  const el = $("rawJson");
  if (el) el.value = JSON.stringify(toSettings(), null, 2);
}

function renderOverview() {
  const s = toSettings();
  const avatar = sceneMeta.hasAvatar
    ? (sceneMeta.avatar || sceneMeta.note || t("stat.on"))
    : (sceneMeta.unity ? t("meta.noAvatar") : t("meta.preview"));
  const av = $("ov-avatar");
  av.textContent = avatar;
  av.title = avatar;
  $("ov-seed").textContent = s.seed === 0 ? t("meta.randomSeed") : String(s.seed);
  $("ov-len").textContent = String(s.nameLength ?? 12);
  $("ov-tex").textContent = s.crypto.textureMode || "compose";
  const p = s.attest.provider || "off";
  $("ov-attest").textContent = p === "off" ? t("meta.off") : p;
  const fp = ($("fingerprint") && $("fingerprint").textContent) || t("meta.none");
  $("ov-fp").textContent = fp;
  setLamp("ov-hierarchy", s.names.obfuscateHierarchy);
  setLamp("ov-blendshapes", s.names.obfuscateBlendShapes);
  setLamp("ov-layers", s.names.obfuscateAnimatorLayers);
  setLamp("ov-states", s.names.obfuscateStates);
  setLamp("ov-params", s.parameters.obfuscate);
  setLamp("ov-assets", s.assets.obfuscateClonedNames);
  setLamp("ov-watermark", s.watermark.enabled);
  setLamp("ov-attest-on", p !== "off");
  const paramRename = sceneItems.filter((item) => classifyClient(item).keep === "none").length;
  setStat("ov-hierarchy-n", layerStat(s.names.obfuscateHierarchy, sceneMeta.transformsRename, sceneMeta.transforms), s.names.obfuscateHierarchy);
  setStat("ov-blendshapes-n", layerStat(s.names.obfuscateBlendShapes, sceneMeta.blendshapesRename, sceneMeta.blendshapes), s.names.obfuscateBlendShapes);
  setStat("ov-layers-n", layerStat(s.names.obfuscateAnimatorLayers, s.names.obfuscateAnimatorLayers ? sceneMeta.animatorLayers : 0, sceneMeta.animatorLayers), s.names.obfuscateAnimatorLayers);
  setStat("ov-states-n", s.names.obfuscateStates ? t("stat.withStates") : t("stat.off"), s.names.obfuscateStates);
  setStat("ov-params-n", layerStat(s.parameters.obfuscate, paramRename, sceneItems.length), s.parameters.obfuscate);
  setStat("ov-assets-n", s.assets.obfuscateClonedNames ? t("stat.afterClone") : t("stat.off"), s.assets.obfuscateClonedNames);
  setStat("ov-watermark-n", s.watermark.enabled ? fmtAmp(s.watermark.amplitude) : t("stat.off"), s.watermark.enabled);
  setStat("ov-attest-n", p === "off" ? t("stat.off") : p, p !== "off");
  const decoyPlan = $("ov-scene-decoy");
  if (decoyPlan) {
    decoyPlan.textContent = t("overview.plannedDecoy", { n: s.names.decoyBlendShapeCount || 0 });
  }
  renderLastBuild();
  const nameMeta = $("name-meta");
  if (nameMeta) {
    nameMeta.textContent = sceneMeta.hasAvatar || sceneMeta.transforms
      ? t("stat.willChange", { n: sceneMeta.transformsRename, total: sceneMeta.transforms }) +
        " · blendshape " + t("stat.willChange", { n: sceneMeta.blendshapesRename, total: sceneMeta.blendshapes })
      : t("meta.preview");
  }
}

function snapshot() {
  return JSON.stringify(toSettings());
}

function markClean() {
  savedJson = snapshot();
  $("dirty-flag").hidden = true;
}

function markDirty() {
  const dirty = snapshot() !== savedJson;
  $("dirty-flag").hidden = !dirty;
}

function fromSettings(s) {
  const names = s.names || {};
  const parameters = s.parameters || {};
  const assets = s.assets || {};
  const watermark = s.watermark || {};
  const crypto = s.crypto || {};
  const attest = s.attest || {};
  $("seed").value = String(s.seed ?? 0);
  $("nameLength").value = String(s.nameLength ?? 12);
  substrs = parseCsv(s.preserveNameSubstrings);
  extras = parseCsv(parameters.extraPreserve);
  pins = new Set(parseCsv(parameters.pinPreserve));
  renderChips($("substr-chips"), substrs);
  renderChips($("extra-chips"), extras);
  $("preserveMmd").selected = !!s.preserveMmd;
  $("autoDetectPreserve").selected = !!s.autoDetectPreserve;
  $("obfuscateHierarchy").checked = !!names.obfuscateHierarchy;
  $("obfuscateBlendShapes").checked = !!names.obfuscateBlendShapes;
  $("obfuscateAnimatorLayers").checked = !!names.obfuscateAnimatorLayers;
  $("obfuscateStates").checked = !!names.obfuscateStates;
  const decoyEl = $("decoyBlendShapeCount");
  if (decoyEl) decoyEl.value = String(Math.max(0, Math.min(32, Number(names.decoyBlendShapeCount) || 0)));
  $("obfuscateClonedNames").selected = !!assets.obfuscateClonedNames;
  $("textureMode").value = crypto.textureMode || "compose";
  const atRest = $("encryptSettingsAtRest");
  if (atRest) atRest.selected = !!crypto.encryptSettingsAtRest;
  $("obfuscateParams").selected = !!parameters.obfuscate;
  $("watermarkEnabled").selected = !!watermark.enabled;
  $("watermarkAmp").value = fmtAmp(watermark.amplitude ?? 0.00001);
  setRadio("provider", attest.provider || "off");
  $("attestOwner").value = attest.owner || "";
  $("attestRepo").value = attest.repo || "";
  $("attestBranch").value = attest.branch || "main";
  $("attestPath").value = attest.path || "ova-attest.json";
  const note = $("projectNote");
  if (note) note.value = (s.ui && s.ui.projectNote) || "";
  const nextLocale = (s.ui && s.ui.locale) || detectLocale();
  setLocale(nextLocale);
  applyMode((s.ui && s.ui.mode) || "expert", false);
  syncTiles();
  renderOverview();
  renderParamList();
  syncRawJson();
}

function toSettings() {
  return {
    version: 1,
    seed: Number($("seed").value),
    nameLength: Number($("nameLength").value),
    preserveMmd: !!$("preserveMmd").selected,
    autoDetectPreserve: !!$("autoDetectPreserve").selected,
    preserveNameSubstrings: joinCsv(substrs),
    names: {
      obfuscateHierarchy: !!$("obfuscateHierarchy").checked,
      obfuscateBlendShapes: !!$("obfuscateBlendShapes").checked,
      obfuscateAnimatorLayers: !!$("obfuscateAnimatorLayers").checked,
      obfuscateStates: !!$("obfuscateStates").checked,
      decoyBlendShapeCount: Math.max(0, Math.min(32, Number(($("decoyBlendShapeCount") && $("decoyBlendShapeCount").value) || 0)))
    },
    parameters: {
      obfuscate: !!$("obfuscateParams").selected,
      extraPreserve: joinCsv(extras),
      pinPreserve: [...pins].sort().join(",")
    },
    assets: { obfuscateClonedNames: !!$("obfuscateClonedNames").selected },
    watermark: {
      enabled: !!$("watermarkEnabled").selected,
      amplitude: Number($("watermarkAmp").value)
    },
    crypto: {
      textureMode: $("textureMode").value || "compose",
      encryptSettingsAtRest: !!($("encryptSettingsAtRest") && $("encryptSettingsAtRest").selected)
    },
    attest: {
      provider: radioValue("provider"),
      owner: $("attestOwner").value.trim(),
      repo: $("attestRepo").value.trim(),
      branch: $("attestBranch").value.trim() || "main",
      path: $("attestPath").value.trim() || "ova-attest.json"
    },
    ui: {
      locale: locale(),
      mode: uiMode,
      projectNote: ($("projectNote") && $("projectNote").value.trim()) || ""
    }
  };
}

function matchesPreserveToken(name, token) {
  if (!name || !token) return false;
  if (token.charAt(token.length - 1) === "/") {
    const inner = token.slice(0, -1);
    const lower = name.toLowerCase();
    if (name.toLowerCase().startsWith(token.toLowerCase())) return true;
    return inner.length > 0 && lower.indexOf("/" + inner.toLowerCase()) >= 0;
  }
  if (token.length <= 2) return hasTokenBoundary(name, token);
  return name.toLowerCase().indexOf(token.toLowerCase()) >= 0;
}

function hasTokenBoundary(name, token) {
  const n = name.toLowerCase();
  const t = token.toLowerCase();
  let start = 0;
  while (start <= n.length - t.length) {
    const at = n.indexOf(t, start);
    if (at < 0) return false;
    const leftOk = at === 0 || !isNameTokenChar(n.charCodeAt(at - 1));
    const end = at + t.length;
    const rightOk = end >= n.length || !isNameTokenChar(n.charCodeAt(end));
    if (leftOk && rightOk) return true;
    start = at + 1;
  }
  return false;
}

function isNameTokenChar(code) {
  return (code >= 48 && code <= 57) || (code >= 65 && code <= 90) || (code >= 97 && code <= 122);
}

function classifyClient(item) {
  const name = item.name;
  if (item.keep === "reserved" || /^(Go\/|VrcDcc\/)/.test(name)) {
    return { keep: "reserved", reason: item.reason || "reserved" };
  }
  if (pins.has(name)) return { keep: "pin", reason: "picker" };
  const prefixes = sceneMeta.physbonePrefixes || [];
  if (item.keep === "physbone" || prefixes.some((p) => p && (name === p || name.startsWith(p + "_")))) {
    return { keep: "physbone", reason: "physbone" };
  }
  if (extras.some((t) => t && name.toLowerCase().includes(t.toLowerCase()))) {
    return { keep: "extra", reason: "substring" };
  }
  if ($("autoDetectPreserve").selected) {
    const hints = ["Go/", "GoGo", "FT", "VRCFT", "vrcft", "Tracking/", "OSC", "VrcDcc/", "FaceEmo", "VRCFaceTracking"];
    const hit = hints.find((h) => matchesPreserveToken(name, h));
    if (hit) return { keep: "auto", reason: hit };
  }
  return { keep: "none", reason: "" };
}

function renderPinChips() {
  const host = $("pin-chips");
  if (!host) return;
  host.innerHTML = "";
  const list = [...pins].sort();
  list.forEach((v) => {
    const b = document.createElement("button");
    b.type = "button";
    b.className = "chip";
    b.textContent = v + " ×";
    b.addEventListener("click", () => {
      pins.delete(v);
      markDirty();
      renderParamList();
      renderOverview();
    });
    host.appendChild(b);
  });
}

function renderParamList() {
  const host = $("param-list");
  const q = (($("paramSearch") && $("paramSearch").value) || "").trim().toLowerCase();
  const rows = sceneItems.filter((item) => {
    const live = classifyClient(item);
    const keep = live.keep;
    if (q && !item.name.toLowerCase().includes(q)) return false;
    if (paramFilter === "none") return keep === "none";
    if (paramFilter === "pin") return keep === "pin";
    if (paramFilter === "keep") return keep !== "none";
    return true;
  });
  const rename = sceneItems.filter((item) => classifyClient(item).keep === "none").length;
  const kept = sceneItems.length - rename;
  $("param-meta").textContent = sceneItems.length
    ? t("params.meta", { total: sceneItems.length, rename, keep: kept })
    : t("params.none");
  if (sceneMeta.unity === false) {
    $("param-note").textContent = t("params.notePreview");
  } else if (!sceneMeta.hasAvatar) {
    $("param-note").textContent = sceneMeta.note || t("params.noteEmpty");
  } else {
    $("param-note").textContent = t("meta.avatar") + " " + (sceneMeta.avatar || sceneMeta.note || "");
  }
  renderPinChips();
  host.innerHTML = "";
  if (!rows.length) {
    const empty = document.createElement("div");
    empty.className = "param-empty";
    empty.textContent = sceneItems.length ? t("params.nomatch") : t("params.unread");
    host.appendChild(empty);
    return;
  }
  const paramsOn = $("obfuscateParams").selected;
  rows.forEach((item) => {
    const live = classifyClient(item);
    const row = document.createElement("label");
    row.className = "param-row" + (live.keep === "none" ? "" : " is-keep");
    const box = document.createElement("input");
    box.type = "checkbox";
    const locked = live.keep === "reserved" || live.keep === "auto" || live.keep === "extra" || live.keep === "physbone";
    box.checked = live.keep !== "none";
    box.disabled = locked || !paramsOn;
    box.addEventListener("change", () => {
      if (box.checked) pins.add(item.name);
      else pins.delete(item.name);
      markDirty();
      renderParamList();
      renderOverview();
    });
    const name = document.createElement("span");
    name.className = "param-name";
    name.textContent = item.name;
    const src = document.createElement("span");
    src.className = "param-src";
    src.textContent = item.source || "";
    const tag = document.createElement("span");
    tag.className = "tag";
    tag.textContent = keepLabel(live.keep) || live.keep;
    row.append(box, name, src, tag);
    host.appendChild(row);
  });
}

async function loadScene() {
  const res = await fetch("/api/scene/parameters");
  if (!res.ok) throw new Error(t("err.scene"));
  const data = await res.json();
  sceneMeta = {
    unity: !!data.unity,
    hasAvatar: !!data.hasAvatar,
    note: data.note || data.avatar || "",
    avatar: data.avatar || "",
    physbonePrefixes: Array.isArray(data.physbonePrefixes) ? data.physbonePrefixes : [],
    transforms: Number(data.transforms) || 0,
    transformsRename: Number(data.transformsRename) || 0,
    blendshapes: Number(data.blendshapes) || 0,
    blendshapesRename: Number(data.blendshapesRename) || 0,
    animatorLayers: Number(data.animatorLayers) || 0
  };
  sceneItems = Array.isArray(data.items) ? data.items : [];
  renderParamList();
  renderOverview();
}

async function refreshAttest() {
  const [statusRes, fpRes] = await Promise.all([
    fetch("/api/attest/status"),
    fetch("/api/attest/fingerprint")
  ]);
  const status = await statusRes.json();
  const fp = await fpRes.json();
  $("fingerprint").textContent = fp.fingerprint || "—";
  const provider = radioValue("provider") || status.provider || "off";
  const has = provider === "github" ? status.hasGithubSecret
    : provider === "gitee" ? status.hasGiteeSecret
    : false;
  $("secret-state").textContent = provider === "off"
    ? t("attest.secretOff")
    : (has ? t("attest.secretYes") : t("attest.secretNo"));
  renderOverview();
}

async function load() {
  const res = await fetch("/api/settings");
  if (!res.ok) throw new Error(t("err.load") + " " + res.status);
  fromSettings(await res.json());
  await refreshAttest();
  await loadScene().catch(() => {});
  await loadLastBuild();
  markClean();
  toast(t("loaded"), true);
}

async function save() {
  const res = await fetch("/api/settings", {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(toSettings())
  });
  if (!res.ok) throw new Error(t("err.save"));
  $("save").classList.remove("pulse");
  void $("save").offsetWidth;
  $("save").classList.add("pulse");
  await refreshAttest();
  await loadScene().catch(() => {});
  markClean();
  toast(t("saved"), true);
}

function selectedProvider() {
  const value = radioValue("provider");
  return value === "github" || value === "gitee" ? value : null;
}

async function saveToken(clear) {
  const provider = selectedProvider();
  if (!provider) throw new Error(t("err.token"));
  const token = clear ? "" : ($("attestToken").value || "");
  const res = await fetch("/api/attest/secret", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ provider, token })
  });
  if (!res.ok) throw new Error(t("err.tokenWrite"));
  $("attestToken").value = "";
  await refreshAttest();
  toast(clear ? t("attest.cleared") : t("attest.tokenSaved"), true);
}

async function publishStub() {
  await save();
  const res = await fetch("/api/attest/publish", { method: "POST" });
  const body = await res.json();
  const pre = $("would-post");
  pre.hidden = false;
  pre.textContent = JSON.stringify(body, null, 2);
  toast(body.ok ? t("attest.published") : t("attest.notWired"), !!body.ok);
}

async function copyFingerprint() {
  const text = $("fingerprint").textContent || "";
  await navigator.clipboard.writeText(text);
  toast(t("attest.copied"), true);
}

function bind() {
  document.querySelectorAll(".dest").forEach((btn) => {
    btn.addEventListener("click", () => showPage(btn.dataset.page));
  });
  document.querySelectorAll(".layer-row").forEach((row) => {
    row.addEventListener("click", () => showPage(row.dataset.page));
  });
  $("reload").addEventListener("click", () => load().catch((e) => toast(String(e.message || e), false)));
  $("save").addEventListener("click", () => save().catch((e) => toast(String(e.message || e), false)));
  $("saveToken").addEventListener("click", () => saveToken(false).catch((e) => toast(String(e.message || e), false)));
  $("clearToken").addEventListener("click", () => saveToken(true).catch((e) => toast(String(e.message || e), false)));
  $("publishAttest").addEventListener("click", () => publishStub().catch((e) => toast(String(e.message || e), false)));
  $("copyFp").addEventListener("click", () => copyFingerprint().catch((e) => toast(String(e.message || e), false)));
  $("substrAddBtn").addEventListener("click", () => addChip("substr"));
  $("extraAddBtn").addEventListener("click", () => addChip("extra"));
  $("substrAdd").addEventListener("keydown", (e) => { if (e.key === "Enter") addChip("substr"); });
  $("extraAdd").addEventListener("keydown", (e) => { if (e.key === "Enter") addChip("extra"); });
  $("paramRefresh").addEventListener("click", () => loadScene().catch((e) => toast(String(e.message || e), false)));
  $("paramSearch").addEventListener("input", () => renderParamList());
  document.querySelectorAll(".filter-btn").forEach((btn) => {
    btn.addEventListener("click", () => {
      paramFilter = btn.dataset.filter;
      document.querySelectorAll(".filter-btn").forEach((b) => b.classList.toggle("active", b === btn));
      renderParamList();
    });
  });
  document.querySelectorAll(".provider-card").forEach((card) => {
    card.addEventListener("click", () => {
      const radio = card.querySelector("md-radio");
      if (!radio) return;
      radio.checked = true;
      setRadio("provider", radio.value);
      refreshAttest().catch(() => {});
      markDirty();
    });
  });
  document.querySelectorAll("md-checkbox").forEach((box) => {
    box.addEventListener("change", () => { syncTiles(); markDirty(); renderOverview(); });
  });
  document.querySelector(".content").addEventListener("change", () => {
    markDirty();
    renderParamList();
    renderOverview();
  });
  document.querySelector(".content").addEventListener("input", () => {
    markDirty();
    if (currentPage === "pack") syncRawJson();
  });
  $("obfuscateParams").addEventListener("change", () => { renderParamList(); renderOverview(); });
  $("autoDetectPreserve").addEventListener("change", () => { renderParamList(); renderOverview(); });
  document.querySelectorAll("[data-locale]").forEach((btn) => {
    btn.addEventListener("click", () => {
      setLocale(btn.dataset.locale);
      refreshChrome();
      renderOverview();
      renderParamList();
      markDirty();
    });
  });
  document.querySelectorAll("[data-mode-set]").forEach((btn) => {
    btn.addEventListener("click", () => applyMode(btn.dataset.modeSet, true));
  });
  document.querySelectorAll("[data-amp]").forEach((btn) => {
    btn.addEventListener("click", () => {
      $("watermarkAmp").value = fmtAmp(btn.dataset.amp);
      markDirty();
      renderOverview();
    });
  });
  $("packCopy").addEventListener("click", () => copyBrief(packCtx()).then(() => toast(t("pack.copied"), true)).catch((e) => toast(String(e.message || e), false)));
  $("packZip").addEventListener("click", () => {
    try { downloadPack(packCtx()); toast(t("pack.zip"), true); }
    catch (e) { toast(String(e.message || e), false); }
  });
  $("packImport").addEventListener("click", () => $("import-dlg").showModal());
  $("importClose").addEventListener("click", () => $("import-dlg").close());
  $("importApply").addEventListener("click", () => {
    const parsed = parseParams($("importPaste").value);
    if (!parsed) { toast(t("pack.bad"), false); return; }
    fromSettings(mergeSettings(toSettings(), parsed));
    $("import-dlg").close();
    markDirty();
    toast(t("pack.applied"), true);
  });
  if ($("rawJson")) {
    $("rawJson").addEventListener("change", () => {
      try {
        const parsed = JSON.parse($("rawJson").value);
        fromSettings(parsed);
        markDirty();
      } catch (e) {
        toast(String(e.message || e), false);
      }
    });
  }
  window.addEventListener("resize", moveRailPill);
  window.addEventListener("beforeunload", (e) => {
    if ($("dirty-flag").hidden) return;
    e.preventDefault();
    e.returnValue = "";
  });
  document.addEventListener("keydown", (e) => {
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "s") {
      e.preventDefault();
      save().catch((err) => toast(String(err.message || err), false));
    }
  });
}

async function boot() {
  bind();
  const defs = [
    "md-filled-button", "md-outlined-button", "md-filled-tonal-button", "md-text-button",
    "md-checkbox", "md-switch", "md-outlined-select", "md-outlined-text-field",
    "md-radio"
  ].map((name) => customElements.whenDefined(name));
  const timeout = new Promise((_, reject) => setTimeout(() => reject(new Error("m3-timeout")), 10000));
  try {
    await Promise.race([Promise.all(defs), timeout]);
  } catch {
    $("boot-fail").hidden = false;
  }
  await load().catch((e) => toast(String(e.message || e), false));
  moveRailPill();
}

boot();
