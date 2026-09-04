using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ova.Editor
{
    internal static class OvaSerializedRename
    {
        public static void ReplaceExactStrings(Object obj, Dictionary<string, string> map)
        {
            if (obj == null || map == null || map.Count == 0) return;
            var so = new SerializedObject(obj);
            var it = so.GetIterator();
            var enter = true;
            var dirty = false;
            while (it.Next(enter))
            {
                enter = it.propertyType == SerializedPropertyType.Generic || it.isArray;
                if (it.propertyType != SerializedPropertyType.String) continue;
                if (it.propertyPath == "m_Name") continue;
                var v = it.stringValue;
                string nn;
                if (string.IsNullOrEmpty(v) || !map.TryGetValue(v, out nn)) continue;
                it.stringValue = nn;
                dirty = true;
            }

            if (dirty)
                so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void ReplaceOnBehaviours(GameObject root, Dictionary<string, string> map)
        {
            if (root == null) return;
            var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
                ReplaceExactStrings(behaviours[i], map);
        }
    }
}
