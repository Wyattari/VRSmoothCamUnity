using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.OpenXR;

using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.OpenXR.Features
{
    /// <summary>
    /// API for finding and managing feature sets for OpenXR.
    /// </summary>
    [InitializeOnLoad]
    public static class OpenXRFeatureSetManager
    {

        static OpenXRFeatureSetManager()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAssemblyReload;
        }

        static void OnAssemblyReload()
        {
            InitializeFeatureSets();
        }

        internal static void FirstRunInitOfFeatureSets()
        {
            EditorApplication.update -= FirstRunInitOfFeatureSets;
            InitializeFeatureSets();
        }

        /// <summary>
        /// Description of a known (either built-in or found) feature set.
        /// </summary>
        public class FeatureSet
        {
            /// <summary>
            /// Toggles the enabled state for this feature. Impacts the effect of <see cref="OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets"/>.
            /// If you change this value, you must call <see cref="OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets"/> to reflect that change on the actual feature sets.
            /// </summary>
            public bool isEnabled;

            /// <summary>
            /// The name that displays in the UI.
            /// </summary>
            public string name;

            /// <summary>
            /// Description of this feature set.
            /// </summary>
            public string description;

            /// <summary>
            /// The feature set id as defined in <see cref="OpenXRFeatureSetAttribute.FeatureSetId"/>.
            /// </summary>
            public string featureSetId;

            /// <summary>
            /// The text to be shown with the <see cref="downloadLink" />.
            /// </summary>
            public string downloadText;

            /// <summary>
            /// The URI string used to link to external documentation.
            /// </summary>
            public string downloadLink;

            /// <summary>
            /// The set of features that this feature set menages.
            /// </summary>
            public string[] featureIds;

            /// <summary>
            /// State that tracks whether this feature set is built in or was detected after the user installed it.
            /// </summary>
            public bool isInstalled;
        }

        internal class FeatureSetInfo : FeatureSet
        {
            public GUIContent uiName;

            public GUIContent uiLongName;

            public GUIContent uiDescription;

            public GUIContent helpIcon;

            public bool wasChanged;
        }

        static Dictionary<BuildTargetGroup, List<FeatureSetInfo>> s_AllFeatureSets = null;

        static void FillKnownFeatureSets(bool addTestFeatureSet = false)
        {
            BuildTargetGroup[] buildTargetGroups = new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android };

            if (addTestFeatureSet)
            {
                foreach (var buildTargetGroup in buildTargetGroups)
                {
                    List<FeatureSetInfo> knownFeatureSets = new List<FeatureSetInfo>();
                    if (addTestFeatureSet)
                    {
                        knownFeatureSets.Add(new FeatureSetInfo(){
                            isEnabled = false,
                            name = "Known Test",
                            featureSetId = "com.unity.xr.test.featureset",
                            description = "Known Test feature set.",
                            downloadText = "Click here to go to the Unity main website.",
                            downloadLink = "https://docs.unity3d.com/Packages/com.unity.xr.openxr@0.1/manual/index.html",
                            uiName = new GUIContent("Known Test"),
                            uiDescription = new GUIContent("Known Test feature set."),
                            helpIcon = new GUIContent("", CommonContent.k_HelpIcon.image, "Click here to go to the Unity main website."),
                        });
                    }
                    s_AllFeatureSets.Add(buildTargetGroup, knownFeatureSets);
                }
            }

            foreach (var kvp in KnownFeatureSets.k_KnownFeatureSets)
            {
                List<FeatureSetInfo> knownFeatureSets;
                if (!s_AllFeatureSets.TryGetValue(kvp.Key, out knownFeatureSets))
                {
                    knownFeatureSets= new List<FeatureSetInfo>();
                    foreach (var featureSet in kvp.Value)
                    {
                        knownFeatureSets.Add(new FeatureSetInfo(){
                            isEnabled = false,
                            name = featureSet.name,
                            featureSetId = featureSet.featureSetId,
                            description = featureSet.description,
                            downloadText = featureSet.downloadText,
                            downloadLink = featureSet.downloadLink,
                            uiName = new GUIContent(featureSet.name),
                            uiLongName = new GUIContent($"{featureSet.name} feature set"),
                            uiDescription = new GUIContent(featureSet.description),
                            helpIcon = new GUIContent("", CommonContent.k_HelpIcon.image, featureSet.downloadText),
                        });
                    }
                    s_AllFeatureSets.Add(kvp.Key, knownFeatureSets);
                }
            }
        }

        /// <summary>
        /// Initializes all currently known feature sets. This will do two initialization passes:
        ///
        /// 1) Starts with all built in/known feature sets.
        /// 2) Queries the system for anything with an <see cref="OpenXRFeatureSetAttribute"/>
        /// defined on it and uses that to add/update the store of known feature sets.
        /// </summary>
        public static void InitializeFeatureSets()
        {
            InitializeFeatureSets(false);
        }

        internal static void InitializeFeatureSets(bool addTestFeatureSet)
        {
            if (s_AllFeatureSets == null)
                s_AllFeatureSets = new Dictionary<BuildTargetGroup, List<FeatureSetInfo>>();

            s_AllFeatureSets.Clear();

            FillKnownFeatureSets(addTestFeatureSet);

            var types = TypeCache.GetTypesWithAttribute<OpenXRFeatureSetAttribute>();
            foreach (var t in types)
            {
                var attrs = Attribute.GetCustomAttributes(t);
                foreach (var attr in attrs)
                {
                    var featureSetAttr = attr as OpenXRFeatureSetAttribute;
                    if (featureSetAttr == null)
                        continue;

                    if (!addTestFeatureSet && featureSetAttr.FeatureSetId.Contains("com.unity.xr.test.featureset"))
                        continue;

                    foreach (var buildTargetGroup in featureSetAttr.SupportedBuildTargets)
                    {
                        var key = buildTargetGroup;
                        if (!s_AllFeatureSets.ContainsKey(key))
                        {
                            s_AllFeatureSets.Add(key, new List<FeatureSetInfo>());
                        }

                        var newFeatureSet = new FeatureSetInfo(){
                            isEnabled = false,
                            name = featureSetAttr.UiName,
                            description = featureSetAttr.Description,
                            featureSetId = featureSetAttr.FeatureSetId,
                            downloadText = "",
                            downloadLink = "",
                            featureIds = featureSetAttr.FeatureIds,
                            isInstalled = true,
                            uiName = new GUIContent(featureSetAttr.UiName),
                            uiLongName = new GUIContent($"{featureSetAttr.UiName} feature set"),
                            uiDescription = new GUIContent(featureSetAttr.Description),
                            helpIcon = String.IsNullOrEmpty(featureSetAttr.Description) ? null : new GUIContent("", CommonContent.k_HelpIcon.image, featureSetAttr.Description),
                        };

                        bool foundFeatureSet = false;
                        var featureSets = s_AllFeatureSets[key];
                        for (int i = 0; i < featureSets.Count; i++)
                        {
                            if (String.Compare(featureSets[i].featureSetId, newFeatureSet.featureSetId, true) == 0)
                            {
                                foundFeatureSet = true;
                                featureSets[i] = newFeatureSet;
                                break;
                            }
                        }
                        if (!foundFeatureSet)
                            featureSets.Add(newFeatureSet);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the list of all <see cref="FeatureSet"/> for the given build target group.
        /// </summary>
        /// <param name="buildTargetGroup">The build target group to find the feature sets for.</param>
        /// <returns>List of <see cref="FeatureSet"/> or null if there is nothing that matches the given input.</returns>
        public static List<FeatureSet> FeatureSetsForBuildTarget(BuildTargetGroup buildTargetGroup)
        {
            return OpenXRFeatureSetManager.FeatureSetInfosForBuildTarget(buildTargetGroup).Select((fi) => fi as FeatureSet).ToList();
        }

        internal static List<FeatureSetInfo> FeatureSetInfosForBuildTarget(BuildTargetGroup buildTargetGroup)
        {
            List<FeatureSetInfo> ret = new List<FeatureSetInfo>();
            HashSet<FeatureSetInfo> featureSetsForBuildTargetGroup = new HashSet<FeatureSetInfo>();

            if (s_AllFeatureSets == null)
                InitializeFeatureSets();

            if (s_AllFeatureSets == null)
                return ret;

            foreach (var key in s_AllFeatureSets.Keys)
            {
                if (key == buildTargetGroup)
                {
                    featureSetsForBuildTargetGroup.UnionWith(s_AllFeatureSets[key]);
                }
            }

            ret.AddRange(featureSetsForBuildTargetGroup);
            return ret;
        }

        /// <summary>
        /// Returns a specific <see cref="FeatureSet"/> instance that matches the input.
        /// </summary>
        /// <param name="buildTargetGroup">The build target group this feature set supports.</param>
        /// <param name="featureSetId">The feature set id for the specific feature set being requested.</param>
        /// <returns>The matching <see cref="FeatureSet"/> or null.</returns>
        public static FeatureSet GetFeatureSetWithId(BuildTargetGroup buildTargetGroup, string featureSetId)
        {
            return GetFeatureSetInfoWithId(buildTargetGroup, featureSetId) as FeatureSet;
        }

        internal static FeatureSetInfo GetFeatureSetInfoWithId(BuildTargetGroup buildTargetGroup, string featureSetId)
        {
            var featureSets = FeatureSetInfosForBuildTarget(buildTargetGroup);
            if (featureSets != null)
            {
                foreach (var featureSet in featureSets)
                {
                    if (String.Compare(featureSet.featureSetId, featureSetId, true) == 0)
                        return featureSet;
                }
            }
            return null;
        }

        /// <summary>
        /// Given the current enabled state of the feature sets that match for a build target group, enable and disable the features associated with
        /// each feature set. Features that overlap sets of varying enabled states will maintain their enabled setting.
        /// </summary>
        /// <param name="buildTargetGroup">The build target group to process features sets for.</param>
        public static void SetFeaturesFromEnabledFeatureSets(BuildTargetGroup buildTargetGroup)
        {
            HashSet<string> enabledFeatureIds = new HashSet<string>();
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(buildTargetGroup);
            foreach (var ext in extInfo.Features)
            {
                if (ext.Feature.enabled)
                    enabledFeatureIds.Add(ext.Attribute.FeatureId);
            }


            var featureSets = FeatureSetInfosForBuildTarget(buildTargetGroup);
            foreach (var featureSet in featureSets)
            {
                if (featureSet.featureIds == null)
                    continue;

                if (featureSet.isEnabled)
                    enabledFeatureIds.UnionWith(featureSet.featureIds);
                else if (featureSet.wasChanged)
                    enabledFeatureIds.ExceptWith(featureSet.featureIds);

                featureSet.wasChanged = false;
            }

            foreach (var ext in extInfo.Features)
            {
                ext.Feature.enabled = enabledFeatureIds.Contains(ext.Attribute.FeatureId);
            }
        }
    }
}
