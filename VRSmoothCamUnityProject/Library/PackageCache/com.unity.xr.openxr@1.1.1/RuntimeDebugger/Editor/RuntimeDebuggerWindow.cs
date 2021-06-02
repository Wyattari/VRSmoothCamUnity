using UnityEditor.IMGUI.Controls;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.XR.OpenXR;

using UnityEngine.XR.OpenXR.Features.RuntimeDebugger;

namespace UnityEditor.XR.OpenXR.Features.RuntimeDebugger
{
    internal class DebuggerTreeView : TreeView
    {
        public DebuggerTreeView(TreeViewState state)
        : base(state)
        {
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(0, -1, "Root");
            foreach (var t in DebuggerState._functionCalls)
            {
                root.AddChild(t);
            }

            SetupDepthsFromParentsAndChildren(root);

            return root;
        }
    }

    internal class RuntimeDebuggerWindow : EditorWindow
    {
        private IConnectionState state;
        void OnEnable()
        {
            state = PlayerConnectionGUIUtility.GetConnectionState(this);
            EditorConnection.instance.Initialize();
            EditorConnection.instance.Register(RuntimeDebuggerOpenXRFeature.kPlayerToEditorSendDebuggerOutput, DebuggerState.OnMessageEvent);
        }

        void OnDisable()
        {
            EditorConnection.instance.Unregister(RuntimeDebuggerOpenXRFeature.kPlayerToEditorSendDebuggerOutput, DebuggerState.OnMessageEvent);
            state.Dispose();
        }

        private Vector2 scrollpos = new Vector2();
        private TreeViewState treeViewState;
        private DebuggerTreeView treeView;

        private string _lastRefreshStats;

        void OnGUI()
        {
            PlayerConnectionGUILayout.ConnectionTargetSelectionDropdown(state);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh"))
            {
                DebuggerState.SetDoneCallback(() =>
                {
                    if (treeViewState == null)
                        treeViewState = new TreeViewState();

                    treeView = new DebuggerTreeView(treeViewState);

                    var debugger = OpenXRSettings.ActiveBuildTargetInstance.GetFeature<RuntimeDebuggerOpenXRFeature>();
                    if (debugger != null)
                        _lastRefreshStats = $"Last payload size: {DebuggerState._lastPayloadSize} ({((100.0f * DebuggerState._lastPayloadSize / debugger.cacheSize)):F2}% cache full) Number of Frames: {DebuggerState._frameCount}";
                    else
                        _lastRefreshStats = $"Last payload size: {DebuggerState._lastPayloadSize}) Number of Frames: {DebuggerState._frameCount}";
                });

                _lastRefreshStats = "Refreshing ...";
                if (EditorApplication.isPlaying)
                {
                    var debugger = OpenXRSettings.Instance.GetFeature<RuntimeDebuggerOpenXRFeature>();
                    if (debugger.enabled)
                    {
                        debugger.RecvMsg(new MessageEventArgs());
                    }
                }
                else
                {
                    EditorConnection.instance.Send(RuntimeDebuggerOpenXRFeature.kEditorToPlayerRequestDebuggerOutput, new byte[]{byte.MinValue});
                }
            }

            if (GUILayout.Button("Clear"))
            {
                DebuggerState._functionCalls.Clear();
                treeView = null;
                treeViewState = null;
                _lastRefreshStats = "";
                scrollpos = Vector2.zero;
            }

            GUILayout.EndHorizontal();

            GUILayout.Label($"Connections: {EditorConnection.instance.ConnectedPlayers.Count}");
            GUILayout.Label(_lastRefreshStats);

            scrollpos = GUILayout.BeginScrollView(scrollpos);
            if (treeView != null)
            {
                var treeRect = GUILayoutUtility.GetRect(position.width, treeView.totalHeight);
                treeView.OnGUI(treeRect);
            }

            GUILayout.EndScrollView();
        }
    }
}
