using System;
using System.IO;
using System.Xml;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UnityEditor.XR.OpenXR.Features.EyeTracking
{
    internal class OpenXREyeTrackingFeatureBuildHooks : OpenXRFeatureBuildHooks
    {
        const string kCapabilitiesElementName = "Capabilities";
        const string kGazeAttributeValue = "gazeInput";

        public override int callbackOrder => 1;

        public override Type featureType => typeof(UnityEngine.XR.OpenXR.Features.Interactions.EyeGazeInteraction);

        protected override void OnPreprocessBuildExt(BuildReport report)
        {}

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {}

        protected override void OnPostprocessBuildExt(BuildReport report)
        {
            var bootConfigPath = report.summary.outputPath;

            if (report.summary.platformGroup == BuildTargetGroup.WSA)
            {
                Debug.Log($"OutputPath: {report.summary.outputPath};");

                string path = report.summary.outputPath;
                string manifestPath = Path.Combine(path, PlayerSettings.productName);

                manifestPath = Path.Combine(manifestPath, "Package.appxmanifest");

                if (!File.Exists(manifestPath))
                    return;

                XmlDocument doc = new XmlDocument();
                doc.Load(manifestPath);

                var root = doc.DocumentElement;
                var capabilitiesNode = root[kCapabilitiesElementName];

                // No Capabilities Node at all
                if(capabilitiesNode == null)
                {
                    capabilitiesNode = doc.CreateElement(kCapabilitiesElementName, root.NamespaceURI);
                    root.AppendChild(capabilitiesNode);
                }

                // Check first if Gaze is already enabled.
                bool gazeEnabled = false;
                for(int i = 0; i < capabilitiesNode.ChildNodes.Count; i++)
                {
                    var element = capabilitiesNode.ChildNodes[i];
                    var attr = element.Attributes.GetNamedItem("Name");
                    if(attr.Value == kGazeAttributeValue)
                    {
                        gazeEnabled = true;
                        break;
                    }
                }

                // If already enabled, nothing to do
                if (gazeEnabled)
                    return;

                var newCapability = doc.CreateElement("DeviceCapability", root.NamespaceURI);
                newCapability.SetAttribute("Name", kGazeAttributeValue);
                capabilitiesNode.AppendChild(newCapability);

                // Write back to File
                File.Delete(manifestPath);
                using (var tw = new XmlTextWriter(manifestPath, System.Text.Encoding.UTF8))
                {
                    tw.Formatting = Formatting.Indented;
                    doc.WriteContentTo(tw);

                }
            }
        }
    }
}
