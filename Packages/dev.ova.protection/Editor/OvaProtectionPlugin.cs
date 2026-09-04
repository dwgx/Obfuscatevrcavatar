#if UNITY_EDITOR
using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(Ova.Editor.OvaProtectionPlugin))]

namespace Ova.Editor
{
    public class OvaProtectionPlugin : Plugin<OvaProtectionPlugin>
    {
        public override string QualifiedName => "dev.ova.protection";
        public override string DisplayName => "OVA Avatar Protection";

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving)
                .Run("OVA store config", ctx =>
                {
                    var state = ctx.GetState<OvaBuildState>();
                    var marker = ctx.AvatarRootObject.GetComponentInChildren<OvaProtection>(true);
                    if (marker == null)
                    {
                        state.Enabled = false;
                        Debug.Log("[OVA] no OvaProtection on avatar — skip");
                        return;
                    }

                    var settings = OvaSettingsStore.LoadOrDefault(marker.settingsJsonPath, marker.settings);
                    state.Settings = settings;
                    var namesOn = settings.names != null &&
                                  (settings.names.obfuscateHierarchy || settings.names.obfuscateBlendShapes ||
                                   settings.names.obfuscateAnimatorLayers || settings.names.obfuscateStates);
                    var otherOn = (settings.parameters != null && settings.parameters.obfuscate)
                                  || (settings.assets != null && settings.assets.obfuscateClonedNames)
                                  || (settings.watermark != null && settings.watermark.enabled);
                    if (!namesOn && !otherOn)
                    {
                        state.Enabled = false;
                        Object.DestroyImmediate(marker);
                        Debug.Log("[OVA] all layers off — skip");
                        return;
                    }

                    state.Enabled = true;
                    state.OrigSmr = ctx.AvatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
                    state.Names = new OvaNameGenerator(settings.seed, settings.nameLength);
                    if (settings.crypto != null && settings.crypto.textureMode == "compose")
                        Debug.Log("[OVA] texture layer is compose-only (ShellProtector / TTT / Ajisai). No lilToon vertex lock.");
                    Object.DestroyImmediate(marker);
                });

            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("nadena.dev.modular-avatar")
                .AfterPlugin("com.vrcfury.vrcfury")
                .AfterPlugin("com.anatawa12.avatar-optimizer")
                .AfterPlugin("net.rs64.tex-trans-tool")
                .AfterPlugin("jp.suzuryg.face-emo.blink-disabler")
                .WithRequiredExtension(typeof(nadena.dev.ndmf.animator.AnimatorServicesContext), seq =>
                {
                    seq.Run("OVA watermark", OvaWatermarkPass.Run);
                    seq.Run("OVA name obfuscation", OvaNameObfuscationPass.Run);
                    seq.Run("OVA parameter obfuscation", OvaParameterPass.Run);
                    seq.Run("OVA cloned asset names", OvaSharedAssetPass.Run);
                    seq.Run("OVA last-build report", OvaBuildReport.Run);
                });
        }
    }
}
#endif
