using UnityEngine;

namespace UnityEditor.XR.OpenXR
{
    [CustomEditor(typeof(OpenXRPackageSettings))]
    internal class PackageSettingsEditor : UnityEditor.Editor
    {
        static class Content
        {
            public const float k_Space = 15.0f;
        }

        public override void OnInspectorGUI()
        {
            var buildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();

            OpenXRPackageSettings settings = serializedObject.targetObject as OpenXRPackageSettings;

            var openXrSettings = settings.GetSettingsForBuildTargetGroup(buildTargetGroup);

            var editor = UnityEditor.Editor.CreateEditor(openXrSettings);

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            EditorGUILayout.BeginVertical();
            editor.DrawDefaultInspector();

            if (buildTargetGroup == BuildTargetGroup.Standalone)
            {
                GUILayout.Space(Content.k_Space);
                OpenXRRuntimeSelector.DrawSelector();
            }

            EditorGUILayout.EndVertical();

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndBuildTargetSelectionGrouping();
        }
    }
}