using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Microsoft.Win32;

namespace UnityEditor.XR.OpenXR
{
    internal class OpenXRRuntimeSelector
    {
        class RuntimeDetector
        {
            public virtual string name { get; }
            public virtual string jsonPath { get; }
            public virtual string tooltip => jsonPath;

            public virtual bool detected => File.Exists(jsonPath);

            public virtual void MakeActive()
            {
                if (detected)
                {
                    Environment.SetEnvironmentVariable("XR_RUNTIME_JSON", jsonPath);
                }
            }
        };

        class SystemDefault : RuntimeDetector
        {
            public override string name
            {
                get
                {
                    string ret = "System Default";
                    if (string.IsNullOrEmpty(tooltip))
                    {
                        ret += " (None Set)";
                    }
                    return ret;
                }
            }

            public override string jsonPath => "";

            public override string tooltip => (string) Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Khronos\OpenXR\1", "ActiveRuntime", "");

            public override bool detected => true;
        }

        class OtherRuntime : RuntimeDetector
        {
            private string runtimeJsonPath = "";

            public override string name => "Other";

            public override string jsonPath => runtimeJsonPath;

            public override void MakeActive()
            {
                var selectedJson = EditorUtility.OpenFilePanel("Select OpenXR Runtime json", "", "json");
                if (!string.IsNullOrEmpty(selectedJson))
                {
                    runtimeJsonPath = selectedJson;
                    base.MakeActive();
                }
            }

            public override bool detected => true;
        }

        class OculusDetector : RuntimeDetector
        {
            private const string installLocKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Oculus";
            private const string installLocValue = "InstallLocation";
            private const string jsonName = @"Support\oculus-runtime\oculus_openxr_64.json";

            public override string name => "Oculus";

            public override string jsonPath
            {
                get
                {
                    var oculusPath = (string)Registry.GetValue(installLocKey, installLocValue, "");
                    if (string.IsNullOrEmpty(oculusPath)) return "";
                    return Path.Combine(oculusPath, jsonName);
                }
            }
        }

        class SteamVRDetector : RuntimeDetector
        {
            public override string name => "SteamVR";

            public override string jsonPath => @"C:\Program Files (x86)\Steam\steamapps\common\SteamVR\steamxr_win64.json";
        }

        class WindowsMRDetector : RuntimeDetector
        {
            public override string name => "Windows Mixed Reality";

            public override string jsonPath => @"C:\WINDOWS\system32\MixedRealityRuntime.json";
        }


        private static List<RuntimeDetector> runtimeDetectors = new List<RuntimeDetector>()
        {
            new SystemDefault(),
            new WindowsMRDetector(),
            new SteamVRDetector(),
            new OculusDetector(),
            new OtherRuntime(),
        };

        static class Content
        {
            public static readonly GUIContent k_ActiveRuntimeLabel = new GUIContent("OpenXR Runtime (Editor Instance Only)", "Changing this value will only affect this instance of the editor.");
        }

        static int GetActiveRuntimeIndex(List<RuntimeDetector> runtimes)
        {
            var runtimeJson = Environment.GetEnvironmentVariable("XR_RUNTIME_JSON");

            if (string.IsNullOrEmpty(runtimeJson))
                return 0;

            var index = runtimes.FindLastIndex(s => s.jsonPath.Equals(runtimeJson, StringComparison.CurrentCultureIgnoreCase));

            if (index == -1)
                return runtimes.Count - 1;

            return index;
        }

        public static void DrawSelector()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Content.k_ActiveRuntimeLabel);
            var runtimes = runtimeDetectors.Where(runtime => runtime.detected).ToList();
            int selectedIndex = GetActiveRuntimeIndex(runtimes);
            int index = EditorGUILayout.Popup(selectedIndex, runtimes.Select(s => new GUIContent(s.name, s.tooltip)).ToArray());
            if (selectedIndex != index)
            {
                runtimes[index].MakeActive();
            }
            GUILayout.EndHorizontal();
        }
    }
}