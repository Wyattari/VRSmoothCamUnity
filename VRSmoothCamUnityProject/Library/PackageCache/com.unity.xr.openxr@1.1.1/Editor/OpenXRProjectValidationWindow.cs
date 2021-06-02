using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEditor.XR.OpenXR
{
    internal class OpenXRProjectValidationWindow : EditorWindow
    {
        private Vector2 scrollViewPos = Vector2.zero;

        private List<OpenXRFeature.ValidationRule> _failures = new List<OpenXRFeature.ValidationRule>();

        // Fix all state
        private List<OpenXRFeature.ValidationRule> _fixAllStack = new List<OpenXRFeature.ValidationRule>();
        private const uint desiredFramesBetweenFixes = 60;
        private uint _framesBetweenFixesCounter = 0;

        static class Content
        {
            public const float k_Space = 15.0f;
            public static readonly GUIContent k_Title = new GUIContent("OpenXR Project Validation", CommonContent.k_ErrorIcon.image);
            public static readonly GUIStyle k_Wrap = new GUIStyle(EditorStyles.label);
            public static readonly GUIContent k_FixButton = new GUIContent("Fix", "");
            public static readonly GUIContent k_PlayMode = new GUIContent("Exit play mode before fixing project validation issues.", EditorGUIUtility.IconContent("console.infoicon").image);
            public static readonly Vector2 k_IconSize = new Vector2(16.0f, 16.0f);
        }

        static class Styles
        {
            public static GUIStyle s_SelectionStyle = "TV Selection";
            public static GUIStyle s_IssuesBackground = "ScrollViewAlt";
            public static GUIStyle s_ListLabel;
            public static GUIStyle s_IssuesTitleLabel;

        }

        BuildTargetGroup selectedBuildTargetGroup = BuildTargetGroup.Unknown;

        internal static void ShowWindow(BuildTargetGroup buildTargetGroup = BuildTargetGroup.Unknown)
        {
            var window = (OpenXRProjectValidationWindow) GetWindow(typeof(OpenXRProjectValidationWindow));
            window.titleContent = Content.k_Title;
            window.minSize = new Vector2(500.0f, 300.0f);
            window.selectedBuildTargetGroup = buildTargetGroup;
            window.Show();
        }

        internal static void CloseWindow()
        {
            var window = (OpenXRProjectValidationWindow) GetWindow(typeof(OpenXRProjectValidationWindow));
            window.Close();
        }

        private void InitStyles()
        {
            if (Styles.s_ListLabel == null)
            {
                Styles.s_ListLabel = new GUIStyle(Styles.s_SelectionStyle);
                Styles.s_ListLabel.border = new RectOffset(0,0,0,0);
                Styles.s_ListLabel.padding = new RectOffset(5, 5, 0, 0);
                Styles.s_ListLabel.margin = new RectOffset(5, 5, 5, 5);

                Styles.s_IssuesTitleLabel = new GUIStyle(EditorStyles.label);
                Styles.s_IssuesTitleLabel.fontSize = 14;
                Styles.s_IssuesTitleLabel.fontStyle = FontStyle.Bold;

            }
        }

        bool DrawIssuesAndFixAll()
        {
            bool fixAll = false;
            GUILayout.Space(Content.k_Space);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"({_failures.Count}) OpenXR Validation Issues.", GUILayout.ExpandWidth(false));
            if (_failures.Count > 0)
                fixAll = GUILayout.Button("Fix All", GUILayout.ExpandWidth(false), GUILayout.Width(100));
            GUILayout.EndHorizontal();

            return fixAll;
        }

        bool DrawIssuesList()
        {
            bool anyFixAppiled = false;
            EditorGUILayout.LabelField("Issues", Styles.s_IssuesTitleLabel);
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos, Styles.s_IssuesBackground, GUILayout.ExpandHeight(true));

            foreach (var result in _failures)
            {
                EditorGUILayout.BeginHorizontal(Styles.s_ListLabel);

                EditorGUILayout.BeginVertical(Styles.s_ListLabel);
                Content.k_FixButton.tooltip = result.fixItMessage;
                if (result.fixIt != null)
                {
                    if (GUILayout.Button(Content.k_FixButton, GUILayout.Width(55.0f)))
                    {
                        result.fixIt.Invoke();
                        anyFixAppiled = true;
                    }
                }
                else
                {
                    GUILayout.Label("", GUILayout.Width(55.0f), GUILayout.ExpandWidth(true));
                }

                EditorGUILayout.EndVertical();

                if (result.error)
                    GUILayout.Label(CommonContent.k_ErrorIcon, EditorStyles.label);
                else
                    GUILayout.Label(CommonContent.k_WarningIcon, EditorStyles.label);

                string message = result.message;
                if (result.feature != null)
                    message = $"[{result.feature.nameUi}] {result.message}";
                Content.k_Wrap.wordWrap = true;
                GUILayout.Label(message, Content.k_Wrap);
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUI.EndDisabledGroup();

            return anyFixAppiled;
        }

        void ActionAnyFixes(bool fixAllSelected, bool anyFixApplied, BuildTargetGroup activeBuildTargetGroup)
        {
            bool fixApplied = anyFixApplied;

            if ((_failures.Any(s => s.fixIt != null) && fixAllSelected) || _fixAllStack.Count > 0)
            {
                // Copy the failures list if there are any that we need to fix
                if (_fixAllStack.Count == 0 && _failures.Count > 0)
                    _fixAllStack = _failures.ToList();

                // If we have any failures that we're fixing ..
                if (_fixAllStack.Count > 0)
                {
                    // Wait a few frames between fixes - some are deferred
                    ++_framesBetweenFixesCounter;
                    if (_framesBetweenFixesCounter >= desiredFramesBetweenFixes)
                    {
                        // Do the fix, remove from the fixall stack, reset counter.
                        _fixAllStack[0].fixIt?.Invoke();
                        _fixAllStack.Remove(_fixAllStack[0]);
                        _framesBetweenFixesCounter = 0;
                    }
                }

                // Request that come in here again next frame to fix the rest of the errors
                if (_fixAllStack.Count > 0)
                    Repaint();

                fixApplied = true;
            }

            if (fixApplied)
                OpenXRProjectValidation.GetCurrentValidationIssues(_failures, activeBuildTargetGroup);
        }

        public void OnGUI()
        {

            InitStyles();

            Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(Content.k_IconSize);

            var activeBuildTargetGroup = selectedBuildTargetGroup;

            if (activeBuildTargetGroup == BuildTargetGroup.Unknown)
                activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            OpenXRProjectValidation.GetCurrentValidationIssues(_failures, activeBuildTargetGroup);

            EditorGUILayout.BeginVertical();

            bool fixAllSelected = DrawIssuesAndFixAll();

            if (EditorApplication.isPlaying)
            {
                GUILayout.Space(Content.k_Space);
                GUILayout.Label(Content.k_PlayMode);
            }

            EditorGUILayout.Space();

            bool anyFixApplied = false;
            if (_failures.Count > 0)
                anyFixApplied = DrawIssuesList();

            EditorGUILayout.EndVertical();

            EditorGUIUtility.SetIconSize(oldIconSize);

            ActionAnyFixes(fixAllSelected, anyFixApplied, activeBuildTargetGroup);
        }
    }

    internal class OpenXRProjectValidationBuiildStep : IPreprocessBuildWithReport
    {
        [OnOpenAsset()]
        static bool ConsoleErrorDoubleClicked(int instanceId, int line)
        {
            var objName = EditorUtility.InstanceIDToObject(instanceId).name;
            if (objName == "OpenXRProjectValidation")
            {
                OpenXRProjectValidationWindow.ShowWindow();
                return true;
            }

            return false;
        }

        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!BuildHelperUtils.HasLoader(report.summary.platformGroup, typeof(OpenXRLoaderBase)))
                return;

            if (OpenXRProjectValidation.LogBuildValidationIssues(report.summary.platformGroup))
                throw new BuildFailedException("OpenXR Build Failed.");
        }
    }
}