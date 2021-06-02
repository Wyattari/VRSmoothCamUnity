using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEditor.XR.OpenXR.Features
{
    internal class OpenXRFeatureEditor : SettingsProvider
    {
        /// <summary>
        /// Path of the OpenXR settings in the Settings window. Uses "/" as separator. The last token becomes the settings label if none is provided.
        /// </summary>
        public const string k_FeatureSettingsPathUI =
#if XR_MGMT_320
            "Project/XR Plug-in Management/OpenXR/Features";
#else
            "Project/XR Plugin Management/OpenXR/Features";
#endif

        static class Styles
        {
            public static float k_IconWidth = 16f;
            public static float k_DefaultSelectionWidth = 200f;
            public static float k_DefualtLineMultiplier = 2f;

            public static GUIStyle s_SelectionStyle = "TV Selection";
            public static GUIStyle s_SelectionBackground = "ScrollViewAlt";
            public static GUIStyle s_FeatureSetTitleLable;
            public static GUIStyle s_ListLabel;
            public static GUIStyle s_ListSelectedLabel;
            public static GUIStyle s_Feature;
            public static GUIStyle s_CategoryLabel;
        }

        static class Content
        {
            public static readonly GUIContent k_HelpIcon = EditorGUIUtility.IconContent("_Help");
        }

        List<OpenXRFeatureSetManager.FeatureSetInfo> selectionListItems = new List<OpenXRFeatureSetManager.FeatureSetInfo>();
        private List<IssueType> issuesPerFeatureSet = new List<IssueType>();
        OpenXRFeatureSetManager.FeatureSetInfo selectedItem = null;

        class ChildListItem
        {
            public GUIContent uiName;
            public GUIContent documentationIcon;
            public GUIContent categoryName;
            public GUIContent version;
            public GUIContent partner;
            public string partnerName;
            public string documentationLink;
            public bool settingsExpanded;
            public OpenXRFeature feature;
            public bool shouldDisplaySettings;
            public UnityEditor.Editor settingsEditor;
            public string featureId;
            public IssueType issueType;
        }

        List<ChildListItem> filteredListItems = new List<ChildListItem>();
        List<ChildListItem> allListItems = new List<ChildListItem>();

        Vector2 selectionScrollPosition = Vector2.zero;
        Vector2 featureScrollPosition = Vector2.zero;


        FeatureHelpersInternal.AllFeatureInfo allFeatureInfos = null;
        BuildTargetGroup activeBuildTarget = BuildTargetGroup.Unknown;

        enum IssueType
        {
            None,
            Warning,
            Error
        }

        List<OpenXRFeature.ValidationRule> _issues = new List<OpenXRFeature.ValidationRule>();

        Dictionary<BuildTargetGroup, int> lastSelectedItemIndex = new Dictionary<BuildTargetGroup, int>();

        void UpdateValidationIssues(BuildTargetGroup buildTargetGroup)
        {
            _issues.Clear();
            OpenXRProjectValidation.GetCurrentValidationIssues(_issues, buildTargetGroup);

            foreach (var item in allListItems)
            {
                item.issueType = GetValidationIssueType(item.feature);
            }

            issuesPerFeatureSet.Clear();
            foreach (var featureSet in selectionListItems)
            {
                var featureSetIssue = IssueType.None;
                foreach (var item in allListItems)
                {
                    if (featureSet.featureIds == null)
                        break;
                    if (Array.IndexOf(featureSet.featureIds, item.featureId) == -1)
                        continue;

                    if (item.issueType == IssueType.Error)
                    {
                        featureSetIssue = IssueType.Error;
                        break;
                    }

                    if (item.issueType == IssueType.Warning)
                    {
                        featureSetIssue = IssueType.Warning;
                    }
                }
                issuesPerFeatureSet.Add(featureSetIssue);
            }
        }

        IssueType GetValidationIssueType(OpenXRFeature feature)
        {
            IssueType ret = IssueType.None;

            foreach (var issue in _issues)
            {
                if (feature == issue.feature)
                {
                    if (issue.error)
                    {
                        ret = IssueType.Error;
                        break;
                    }

                    ret = IssueType.Warning;
                }
            }

            return ret;
        }

        void OnSelectItem(OpenXRFeatureSetManager.FeatureSetInfo selectedItem)
        {
            this.selectedItem = selectedItem;

            int selectedItemIndex = selectionListItems.IndexOf(selectedItem);
            if (lastSelectedItemIndex.ContainsKey(activeBuildTarget))
                lastSelectedItemIndex[activeBuildTarget] = selectedItemIndex;
            else
                lastSelectedItemIndex.Add(activeBuildTarget, selectedItemIndex);

            if (this.selectedItem != null)
            {
                if (String.IsNullOrEmpty(selectedItem.featureSetId))
                    filteredListItems = allListItems.OrderBy((item) => item.uiName.text).ToList();
                else
                    filteredListItems = allListItems.Where((item) => Array.IndexOf(selectedItem.featureIds, item.featureId) > -1 ).OrderBy((item) => item.uiName.text).ToList();
            }
        }


        void DrawSelectionList()
        {
            var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            var lineHeight = EditorGUIUtility.singleLineHeight * Styles.k_DefualtLineMultiplier;

            EditorGUILayout.BeginVertical(GUILayout.Width(Styles.k_DefaultSelectionWidth), GUILayout.ExpandWidth(true));
            {
                EditorGUILayout.LabelField("Feature Sets", Styles.s_FeatureSetTitleLable);

                selectionScrollPosition = EditorGUILayout.BeginScrollView(selectionScrollPosition, GUILayout.Width(Styles.k_DefaultSelectionWidth), GUILayout.ExpandWidth(true));
                {
                    EditorGUILayout.BeginVertical(Styles.s_SelectionBackground, GUILayout.ExpandHeight(true));
                    {
                        int index = 0;
                        foreach (var item in selectionListItems)
                        {
                            var typeOfIssues = issuesPerFeatureSet[index++];
                            var selected = (item == this.selectedItem);
                            var style = selected ? Styles.s_ListSelectedLabel : Styles.s_ListLabel;
                            bool disabled = item.uiName.text != "All" && item.featureIds == null;
                            EditorGUILayout.BeginHorizontal(style, GUILayout.ExpandWidth(true));
                            {
                                EditorGUI.BeginDisabledGroup(disabled);
                                {
                                    if (GUILayout.Button(item.uiName, Styles.s_ListLabel, GUILayout.ExpandWidth(true), GUILayout.Height(lineHeight)))
                                    {
                                        OnSelectItem(item);
                                    }
                                    EditorGUI.EndDisabledGroup();
                                }

                                if (disabled && item.helpIcon != null)
                                {
                                    if (GUILayout.Button(item.helpIcon, EditorStyles.label, GUILayout.Width(Styles.k_IconWidth), GUILayout.Height(lineHeight)))
                                    {
                                        System.Diagnostics.Process.Start(item.downloadLink);
                                    }
                                }
                                if (typeOfIssues != IssueType.None)
                                {
                                    GUIContent icon = (typeOfIssues == IssueType.Error) ? CommonContent.k_ValidationErrorIcon : CommonContent.k_ValidationWarningIcon;
                                    if (GUILayout.Button(icon, EditorStyles.label, GUILayout.Width(Styles.k_IconWidth), GUILayout.Height(lineHeight)))
                                    {
                                        OpenXRProjectValidationWindow.ShowWindow(activeBuildTarget);
                                    }
                                }

                                EditorGUILayout.EndHorizontal();
                            }
                        }

                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
        }

        void DrawFeatureList()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("", Styles.s_FeatureSetTitleLable);

                featureScrollPosition = EditorGUILayout.BeginScrollView(featureScrollPosition, GUILayout.ExpandWidth(true));
                {
                    foreach (var filteredListItem in filteredListItems)
                    {

                        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                        {
                            EditorGUILayout.BeginVertical(Styles.s_Feature, GUILayout.ExpandWidth(false));
                            {
                                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                                {
                                    var typeOfIssue = filteredListItem.issueType;
                                    var featureNameSize = EditorStyles.toggle.CalcSize(filteredListItem.uiName);
                                    var oldEnabledState = filteredListItem.feature.enabled;
                                    filteredListItem.feature.enabled = EditorGUILayout.ToggleLeft(filteredListItem.uiName, filteredListItem.feature.enabled, GUILayout.ExpandWidth(false), GUILayout.Width(featureNameSize.x));
                                    if (oldEnabledState != filteredListItem.feature.enabled && filteredListItem.feature is OpenXRInteractionFeature)
                                    {
                                        ((OpenXRInteractionFeature)filteredListItem.feature).ActiveStateChanged();
                                        EditorUtility.SetDirty(filteredListItem.feature);
                                    }

                                    if (!String.IsNullOrEmpty(filteredListItem.documentationLink))
                                    {
                                        if (GUILayout.Button(filteredListItem.documentationIcon, EditorStyles.label, GUILayout.Width(Styles.k_IconWidth)))
                                        {
                                            System.Diagnostics.Process.Start(filteredListItem.documentationLink);
                                        }
                                    }

                                    if (typeOfIssue != IssueType.None)
                                    {
                                        GUIContent icon = (typeOfIssue == IssueType.Error) ? CommonContent.k_ValidationErrorIcon : CommonContent.k_ValidationWarningIcon;
                                        if (GUILayout.Button(icon, EditorStyles.label, GUILayout.Width(Styles.k_IconWidth)))
                                        {
                                            OpenXRProjectValidationWindow.ShowWindow(activeBuildTarget);
                                        }
                                    }

                                    EditorGUILayout.LabelField(filteredListItem.categoryName, Styles.s_CategoryLabel);
                                    EditorGUILayout.EndHorizontal();
                                }

                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.LabelField(filteredListItem.partner, GUILayout.ExpandWidth(false));
                                    EditorGUILayout.LabelField(filteredListItem.version, Styles.s_CategoryLabel);
                                    EditorGUILayout.EndHorizontal();
                                }
                                EditorGUILayout.Space();

                                if (filteredListItem.shouldDisplaySettings)
                                {
                                    filteredListItem.settingsExpanded = EditorGUILayout.Foldout(filteredListItem.settingsExpanded, "Settings");
                                    if (filteredListItem.settingsExpanded)
                                    {
                                        EditorGUILayout.BeginVertical();
                                        {
                                            if (filteredListItem.settingsEditor == null)
                                            {
                                                filteredListItem.settingsEditor = UnityEditor.Editor.CreateEditor(filteredListItem.feature);
                                            }
                                            EditorGUI.indentLevel += 1;
                                            filteredListItem.settingsEditor.OnInspectorGUI();
                                            EditorGUI.indentLevel -= 1;
                                            EditorGUILayout.EndVertical();
                                        }
                                    }

                                    EditorGUILayout.Space();
                                }

                                EditorGUILayout.EndVertical();
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                    }

                    EditorGUILayout.EndScrollView();
                }


                EditorGUILayout.EndVertical();
            }
        }

        void InitStyles()
        {
            if (Styles.s_ListLabel == null)
            {
                Styles.s_FeatureSetTitleLable = new GUIStyle(EditorStyles.label);
                Styles.s_FeatureSetTitleLable.fontSize = 14;
                Styles.s_FeatureSetTitleLable.fontStyle = FontStyle.Bold;

                Styles.s_ListLabel = new GUIStyle(EditorStyles.label);
                Styles.s_ListLabel.border = new RectOffset(0,0,0,0);
                Styles.s_ListLabel.padding = new RectOffset(5, 0, 0, 0);
                Styles.s_ListLabel.margin = new RectOffset(2, 2, 2, 2);

                Styles.s_ListSelectedLabel = new GUIStyle(Styles.s_SelectionStyle);
                Styles.s_ListSelectedLabel.border = Styles.s_ListLabel.border;
                Styles.s_ListSelectedLabel.padding = Styles.s_ListLabel.padding;
                Styles.s_ListSelectedLabel.margin = Styles.s_ListLabel.margin;

                Styles.s_CategoryLabel = new GUIStyle(Styles.s_SelectionStyle);
                Styles.s_CategoryLabel.alignment = TextAnchor.MiddleRight;
                Styles.s_CategoryLabel.border = new RectOffset(2, 2, 0, 0);
                Styles.s_CategoryLabel.padding = new RectOffset(5, 5, 0, 0);

                Styles.s_Feature = new GUIStyle(Styles.s_SelectionStyle);
                Styles.s_Feature.border = new RectOffset(0, 0, 0, 0);
                Styles.s_Feature.padding = new RectOffset(5, 0, 0, 0);
                Styles.s_Feature.margin = new RectOffset(2, 2, 2, 2);
            }
        }

        public override void OnGUI(string searchContext)
        {
            InitStyles();
            Vector2 iconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(Styles.k_IconWidth, Styles.k_IconWidth));

            var buildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();
            if (buildTargetGroup != activeBuildTarget)
            {
                InitializeFeatures(buildTargetGroup);
            }

            if (allFeatureInfos != null)
            {
                UpdateValidationIssues(buildTargetGroup);
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                DrawSelectionList();
                DrawFeatureList();

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndBuildTargetSelectionGrouping();

            EditorGUIUtility.SetIconSize(iconSize);

            base.OnGUI(searchContext);
        }

        bool HasSettingsToDisplay(OpenXRFeature feature)
        {
            FieldInfo[] fieldInfo = feature.GetType().GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            foreach (var field in fieldInfo)
            {
                var nonSerializedAttrs = field.GetCustomAttributes(typeof(NonSerializedAttribute));
                if (nonSerializedAttrs.Count() == 0)
                    return true;
            }

            fieldInfo = feature.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            foreach (var field in fieldInfo)
            {
                var serializedAttrs = field.GetCustomAttributes(typeof(SerializeField));
                if (serializedAttrs.Count() > 0)
                    return true;
            }

            return false;
        }

        void InitializeFeatures(BuildTargetGroup group)
        {
            selectionListItems.Clear();
            filteredListItems.Clear();
            allListItems.Clear();

            allFeatureInfos = FeatureHelpersInternal.GetAllFeatureInfo(group);

            activeBuildTarget = group;

            var featureSets = OpenXRFeatureSetManager.FeatureSetInfosForBuildTarget(group);
            selectionListItems.AddRange(featureSets.OrderBy((fs) => fs.uiName.text));

            foreach(var _ext in allFeatureInfos.Features)
            {
                if (_ext.Attribute.Hidden)
                    continue;

                allListItems.Add(new ChildListItem()
                {
                    uiName = new GUIContent(_ext.Attribute.UiName),
                    documentationIcon = new GUIContent("", Content.k_HelpIcon.image, "Click for documentation"),
                    categoryName = new GUIContent(_ext.Category.ToString()),
                    partner = new GUIContent($"Author: {_ext.Attribute.Company}"),
                    version = new GUIContent($"Version: {_ext.Attribute.Version}"),
                    partnerName = _ext.Attribute.Company,
                    documentationLink = _ext.Attribute.DocumentationLink,
                    shouldDisplaySettings = HasSettingsToDisplay(_ext.Feature),
                    feature = _ext.Feature,
                    featureId = _ext.Attribute.FeatureId
                });


            }

            selectionListItems.Add(new OpenXRFeatureSetManager.FeatureSetInfo() {
                uiName = new GUIContent("Show All"),
                featureSetId = string.Empty,
                featureIds = allFeatureInfos.Features.Select((e) => e.Attribute.FeatureId).ToArray(),
            });

            var initialSelectedItem = selectionListItems[selectionListItems.Count - 1];
            if (lastSelectedItemIndex.ContainsKey(activeBuildTarget))
            {
                initialSelectedItem = selectionListItems[lastSelectedItemIndex[activeBuildTarget]];
            }
            OnSelectItem(initialSelectedItem);
        }

        public OpenXRFeatureEditor(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProviders()
        {
            if (OpenXRSettings.Instance == null)
                return null;
            if (TypeCache.GetTypesWithAttribute<OpenXRFeatureAttribute>().Count > 0)
                return new OpenXRFeatureEditor(k_FeatureSettingsPathUI, SettingsScope.Project);
            return null;
        }
    }
}
