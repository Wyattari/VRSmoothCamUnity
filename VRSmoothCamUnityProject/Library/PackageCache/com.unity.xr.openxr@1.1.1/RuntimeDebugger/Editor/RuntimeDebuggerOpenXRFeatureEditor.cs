using UnityEngine;

using UnityEngine.XR.OpenXR.Features.RuntimeDebugger;

namespace UnityEditor.XR.OpenXR.Features.RuntimeDebugger
{
    [CustomEditor(typeof(RuntimeDebuggerOpenXRFeature))]
    internal class RuntimeDebuggerOpenXRFeatureEditor : Editor
    {
        private SerializedProperty cacheSize;
        private SerializedProperty perThreadCacheSize;

        void OnEnable()
        {
            cacheSize = serializedObject.FindProperty("cacheSize");
            perThreadCacheSize = serializedObject.FindProperty("perThreadCacheSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(cacheSize);
            EditorGUILayout.PropertyField(perThreadCacheSize);

            if (GUILayout.Button("Open Debugger Window"))
            {
                RuntimeDebuggerWindow window = (RuntimeDebuggerWindow)EditorWindow.GetWindow(typeof(RuntimeDebuggerWindow));
                window.Show();
                window.titleContent = new GUIContent("OpenXR Runtime Debugger");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
