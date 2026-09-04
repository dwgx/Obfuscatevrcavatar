using UnityEngine;
using VRC.SDKBase;

namespace Ova
{
    /// <summary>
    /// Marker. NDMF clone-only. Scene originals stay readable.
    /// Primary UI is ova-web; this component is the bake hook + JSON path.
    /// Not vertex lock. Not a ripper.
    /// </summary>
    [AddComponentMenu("OVA/OVA Protection")]
    [DisallowMultipleComponent]
    public class OvaProtection : MonoBehaviour, IEditorOnly
    {
        [Tooltip("Project-relative JSON written by ova-web. Empty = Library/OVA/settings.json")]
        public string settingsJsonPath = "Library/OVA/settings.json";

        [Tooltip("Used when the JSON file is missing. ova-web overwrites the file, not this blob, unless you Import.")]
        public OvaSettings settings = new OvaSettings();
    }
}
