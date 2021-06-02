using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.OpenXR
{
#if UNITY_EDITOR
    public interface IPackageSettings
    {
        OpenXRSettings GetSettingsForBuildTargetGroup(UnityEditor.BuildTargetGroup buildTargetGroup);
        string GetActiveLoaderLibraryPath();
    }
#endif


    /// <summary>
    /// Build time settings for OpenXR. These are serialized and available at runtime.
    /// </summary>
    [Serializable]
    public partial class OpenXRSettings : ScriptableObject
    {
#if !UNITY_EDITOR
        private static OpenXRSettings s_RuntimeInstance = null;
#endif

        void Awake()
        {
#if !UNITY_EDITOR
            s_RuntimeInstance = this;
#endif
        }

        internal void ApplySettings()
        {
            ApplyRenderSettings();
        }

        private static OpenXRSettings GetInstance(bool useActiveBuildTarget)
        {
            OpenXRSettings settings = null;
            // When running in the Unity Editor, we have to load user's customization of configuration data directly from
            // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
            UnityEngine.Object obj = null;
            UnityEditor.EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out obj);
            if (obj == null || !(obj is IPackageSettings))
                return null;
            var packageSettings = (IPackageSettings) obj;
            // Use standalone settings when running in editor
            var activeBuildTargetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget);
            settings = packageSettings.GetSettingsForBuildTargetGroup(useActiveBuildTarget ? activeBuildTargetGroup : UnityEditor.BuildTargetGroup.Standalone);
#else
            settings = s_RuntimeInstance;
            if (settings == null)
                settings = ScriptableObject.CreateInstance<OpenXRSettings>();
#endif
            return settings;
        }

#if UNITY_EDITOR
        internal static OpenXRSettings GetSettingsForBuildTargetGroup(BuildTargetGroup buildTargetGroup)
        {
            UnityEngine.Object obj = null;
            UnityEditor.EditorBuildSettings.TryGetConfigObject(Constants.k_SettingsKey, out obj);
            if (obj == null || !(obj is IPackageSettings))
                return null;
            var packageSettings = (IPackageSettings) obj;
            return packageSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
        }
#endif

        /// <summary>
        /// Accessor to OpenXR build time settings.
        ///
        /// In the Unity Editor, this returns the settings for the active build target group.
        /// </summary>
        public static OpenXRSettings ActiveBuildTargetInstance => GetInstance(true);

        /// <summary>
        /// Accessor to OpenXR build time settings.
        ///
        /// In the Unity Editor, this returns the settings for the Standalone build target group.
        /// </summary>
        public static OpenXRSettings Instance => GetInstance(false);
    }
}
