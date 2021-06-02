using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR;

[assembly: InternalsVisibleTo("Unity.XR.OpenXR.Tests.Editor")]
[assembly: InternalsVisibleTo("Unity.XR.OpenXR.Tests")]
namespace UnityEditor.XR.OpenXR.Features
{
    /// <summary>
    /// Editor OpenXR Feature helpers.
    /// </summary>
    public static class FeatureHelpers
    {
        /// <summary>
        /// Discovers all features in project and ensures that OpenXRSettings.Instance.features is up to date
        /// for selected build target group.
        /// </summary>
        /// <param name="group">build target group to refresh</param>
        public static void RefreshFeatures(BuildTargetGroup group)
        {
            FeatureHelpersInternal.GetAllFeatureInfo(group);
        }

        /// <summary>
        /// Given a feature id, returns the first instance of <see cref="OpenXRFeature" /> associated with that id.
        /// </summary>
        /// <param name="featureId">The unique id identifying the feature</param>
        /// <returns>The instance of the feature matching thd id, or null.</returns>
        public static OpenXRFeature GetFeatureWithIdForActiveBuildTarget(string featureId)
        {
            if (String.IsNullOrEmpty(featureId))
                return null;

            foreach (var feature in OpenXRSettings.ActiveBuildTargetInstance.features)
            {
                if (String.Compare(featureId, feature.featureIdInternal, true) == 0)
                    return feature;
            }

            return null;
        }

        /// <summary>
        /// Given an array of feature ids, returns an array of matching <see cref="OpenXRFeature" /> instances that match.
        /// </summary>
        /// <param name="featureIds">Array of feature ids to match against.</param>
        /// <returns>An array of all matching features.</returns>
        public static OpenXRFeature[] GetFeaturesWithIdsForActiveBuildTarget(string[] featureIds)
        {
            List<OpenXRFeature> ret = new List<OpenXRFeature>();

            if (featureIds == null || featureIds.Length == 0)
                return ret.ToArray();

            foreach(var featureId in featureIds)
            {
                var feature = GetFeatureWithIdForActiveBuildTarget(featureId);
                if (feature != null)
                    ret.Add(feature);
            }

            return ret.ToArray();
        }
    }

    internal static class FeatureHelpersInternal
    {
        public class AllFeatureInfo
        {
            public List<FeatureInfo> Features;
            public BuildTarget[] CustomLoaderBuildTargets;
        }

        public enum FeatureInfoCategory
        {
            Feature,
            Interaction
        }

        public struct FeatureInfo
        {
            public string PluginPath;
            public OpenXRFeatureAttribute Attribute;
            public OpenXRFeature Feature;
            public FeatureInfoCategory Category;
        }

        private static FeatureInfoCategory DetermineExtensionCategory(string extensionCategoryString)
        {
            if (String.Compare(extensionCategoryString, FeatureCategory.Interaction) == 0)
            {
                return FeatureInfoCategory.Interaction;
            }

            return FeatureInfoCategory.Feature;
        }

        /// <summary>
        /// Gets all features for group. If serialized feature instances do not exist, creates them.
        /// </summary>
        /// <param name="group">BuildTargetGroup to get feature information for.</param>
        /// <returns>feature info</returns>
        public static AllFeatureInfo GetAllFeatureInfo(BuildTargetGroup group)
        {
            AllFeatureInfo ret = new AllFeatureInfo {Features = new List<FeatureInfo>()};
            var openXrSettings = OpenXRPackageSettings.GetOrCreateInstance().GetSettingsForBuildTargetGroup(group);
            if (openXrSettings == null)
            {
                Debug.LogError("Invalid OpenXR Settings");
                return ret;
            }

            // Find any current extensions that are already serialized
            Dictionary<OpenXRFeatureAttribute, OpenXRFeature> currentExts =
                new Dictionary<OpenXRFeatureAttribute, OpenXRFeature>();
            foreach (var ext in openXrSettings.features)
            {
                if (ext != null)
                {
                    foreach (Attribute attr in Attribute.GetCustomAttributes(ext.GetType()))
                    {
                        if (attr is OpenXRFeatureAttribute)
                        {
                            var extAttr = (OpenXRFeatureAttribute) attr;
                            currentExts[extAttr] = ext;
                            break;
                        }
                    }
                }
            }

            // only one custom loader is allowed per platform.
            string customLoaderExtName = "";

            // Find any extensions that haven't yet been added to the feature list and create instances of them
            List<OpenXRFeature> all = new List<OpenXRFeature>();
            foreach (var extType in TypeCache.GetTypesWithAttribute<OpenXRFeatureAttribute>())
            {
                foreach (Attribute attr in Attribute.GetCustomAttributes(extType))
                {
                    if (attr is OpenXRFeatureAttribute)
                    {
                        var extAttr = (OpenXRFeatureAttribute) attr;
                        if (extAttr.BuildTargetGroups != null && !((IList) extAttr.BuildTargetGroups).Contains(group))
                            continue;

                        if (!currentExts.TryGetValue(extAttr, out var extObj))
                        {
                            // Create a new one
                            extObj = (OpenXRFeature) ScriptableObject.CreateInstance(extType);
                            extObj.name = extType.Name + " " + group;
                            AssetDatabase.AddObjectToAsset(extObj, openXrSettings);
                            AssetDatabase.SaveAssets();
                        }

                        if (extObj == null)
                            continue;

                        bool enabled = (extObj.enabled);

                        if (extObj is OpenXRInteractionFeature)
                            ((OpenXRInteractionFeature)extObj).ActiveStateChanged();

                        var ms = MonoScript.FromScriptableObject(extObj);
                        var path = AssetDatabase.GetAssetPath(ms);

                        var dir = "";
                        if(!String.IsNullOrEmpty(path))
                            dir = Path.GetDirectoryName(path);
                        ret.Features.Add(new FeatureInfo()
                        {
                            PluginPath = dir,
                            Attribute = extAttr,
                            Feature = extObj,
                            Category = DetermineExtensionCategory(extAttr.Category)
                        });

                        if (enabled && extAttr.CustomRuntimeLoaderBuildTargets?.Length > 0)
                        {
                            if (ret.CustomLoaderBuildTargets != null && (bool) extAttr.CustomRuntimeLoaderBuildTargets?.Intersect(ret.CustomLoaderBuildTargets).Any())
                            {
                                Debug.LogError($"Only one OpenXR feature may have a custom runtime loader per platform. Disable {customLoaderExtName} or {extAttr.UiName}.");
                            }
                            ret.CustomLoaderBuildTargets = extAttr.CustomRuntimeLoaderBuildTargets?.Union(ret?.CustomLoaderBuildTargets ?? new BuildTarget[]{}).ToArray();
                            customLoaderExtName = extAttr.UiName;
                        }

                        all.Add(extObj);
                        break;
                    }
                }
            }

            openXrSettings.features = all.ToArray();

#if UNITY_EDITOR
            // Ensure the settings are saved after the features are populated
            EditorUtility.SetDirty(openXrSettings);
#endif
            return ret;
        }
    }
}
