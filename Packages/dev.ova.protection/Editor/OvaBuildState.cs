using System.Collections.Generic;

namespace Ova.Editor
{
    internal sealed class OvaBuildState
    {
        public bool Enabled;
        public OvaSettings Settings = new OvaSettings();
        public OvaNameGenerator Names;
        public readonly Dictionary<string, string> BlendShapeRenames = new Dictionary<string, string>();
        public readonly Dictionary<string, string> ParameterRenames = new Dictionary<string, string>();
        public readonly HashSet<string> PreserveParameters = new HashSet<string>();
        public readonly HashSet<string> PhysBonePrefixes = new HashSet<string>();
        public int WatermarkedMeshes;
        public int OrigSmr = -1;
        public int DecoyAdded;
        public int RenamedParameters;
        public int RenamedAssets;
        public int RenamedLayers;
        public int ClipObjectRewrites;
        public int BehaviourAudioRewrites;
        public int ClipNameRewrites;
        public int MeshNameRewrites;
    }
}
