using System;

namespace Ova
{
    /// <summary>
    /// Nested feature tree. JsonUtility-compatible (public fields).
    /// ova-web and the NDMF plugin share this shape.
    /// </summary>
    [Serializable]
    public class OvaSettings
    {
        public int version = 1;
        public int seed = 5145514;
        public int nameLength = 12;
        public bool preserveMmd = true;
        /// <summary>
        /// Preserve CSV for Hierarchy / blendshape names only. Not Animator params.
        /// Tokens ending in '/' are prefixes. Two-letter tokens (FT) match at
        /// letter/digit boundaries only, so <c>left</c> / <c>Gift</c> are not kept.
        /// </summary>
        public string preserveNameSubstrings = "Go/,FT,eye,VRCEmote,VrcDcc/";
        public bool autoDetectPreserve = true;
        public OvaNamesLayer names = new OvaNamesLayer();
        public OvaParamLayer parameters = new OvaParamLayer();
        public OvaAssetLayer assets = new OvaAssetLayer();
        public OvaWatermarkLayer watermark = new OvaWatermarkLayer();
        public OvaCryptoLayer crypto = new OvaCryptoLayer();
        /// <summary>GitHub/Gitee fingerprint registry metadata. Never holds PATs.</summary>
        public OvaAttestLayer attest = new OvaAttestLayer();
        /// <summary>Console only. NDMF ignores this object.</summary>
        public OvaUiLayer ui = new OvaUiLayer();
    }

    [Serializable]
    public class OvaUiLayer
    {
        public string locale = "en";
        public string mode = "expert";
        public string projectNote = "";
    }

    [Serializable]
    public class OvaNamesLayer
    {
        public bool obfuscateHierarchy = true;
        public bool obfuscateBlendShapes = true;
        public bool obfuscateAnimatorLayers = true;
        public bool obfuscateStates = true;
        /// <summary>
        /// Extra unused blendshapes on the clone (unpack labor). Avatar total,
        /// not per renderer. 0 = off. Clamp 0–32. Names are souped; viseme / FT
        /// keep-list is not used for these.
        /// </summary>
        public int decoyBlendShapeCount = 0;
    }

    [Serializable]
    public class OvaParamLayer
    {
        public bool obfuscate = true;
        /// <summary>Substring CSV. Matches anywhere in the param name.</summary>
        public string extraPreserve = "";
        /// <summary>Exact param names CSV, from the ova-web picker.</summary>
        public string pinPreserve = "";
    }

    [Serializable]
    public class OvaAssetLayer
    {
        public bool obfuscateClonedNames = true;
    }

    [Serializable]
    public class OvaWatermarkLayer
    {
        public bool enabled = true;
        public float amplitude = 0.00001f;
    }

    [Serializable]
    public class OvaCryptoLayer
    {
        /// <summary>compose = ShellProtector / Ajisai / TTT; off = skip; ova = reserved.</summary>
        public string textureMode = "compose";
        public bool encryptSettingsAtRest = false;
    }

    /// <summary>
    /// Public registry coordinates only. Tokens live in Library/OVA/secrets.json (Editor).
    /// </summary>
    [Serializable]
    public class OvaAttestLayer
    {
        /// <summary>off | github | gitee</summary>
        public string provider = "off";
        public string owner = "";
        public string repo = "";
        public string branch = "main";
        /// <summary>Blob path inside the repo. Public JSON is fingerprint only.</summary>
        public string path = "ova-attest.json";
    }
}
