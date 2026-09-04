#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Ova.Editor
{
    [CustomEditor(typeof(OvaProtection))]
    public class OvaProtectionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var marker = (OvaProtection)target;
            EditorGUILayout.HelpBox(
                "主界面是 ova-web（本机浏览器）。这里只挂烘焙钩子和 JSON 路径。不要把这包装进封存中的辉夜。",
                MessageType.Info);

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("settingsJsonPath"));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("打开 ova-web"))
                OvaWebServer.StartAndOpen(marker.settingsJsonPath);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("导出 JSON"))
            {
                OvaSettingsStore.Save(marker.settingsJsonPath, marker.settings);
                Debug.Log("[OVA] wrote " + OvaSettingsStore.Resolve(marker.settingsJsonPath));
            }

            if (GUILayout.Button("从 JSON 导入"))
            {
                marker.settings = OvaSettingsStore.LoadOrDefault(marker.settingsJsonPath, marker.settings);
                EditorUtility.SetDirty(marker);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("后备设置（ova-web 未写文件时才用）", EditorStyles.boldLabel);
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "settingsJsonPath");
            serializedObject.ApplyModifiedProperties();
        }
    }

    internal static class OvaWebMenu
    {
        [MenuItem("OVA/打开 ova-web")]
        static void Open()
        {
            OvaWebServer.StartAndOpen(OvaSettingsStore.DefaultRelativePath);
        }

        [MenuItem("OVA/导出当前设置模板")]
        static void ExportTemplate()
        {
            OvaSettingsStore.Save(OvaSettingsStore.DefaultRelativePath, new OvaSettings());
            Debug.Log("[OVA] template " + OvaSettingsStore.Resolve(OvaSettingsStore.DefaultRelativePath));
        }
    }
}
#endif
