using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.TestTooling;
using UnityEngine.XR.OpenXR.Features.Mock;
using Assert = UnityEngine.Assertions.Assert;
using XrSessionState = UnityEngine.XR.OpenXR.Features.Mock.MockDriver.XrSessionState;

[assembly: InternalsVisibleTo("Unity.XR.OpenXR.Tests.Editor")]
[assembly:UnityPlatform(RuntimePlatform.WindowsPlayer, RuntimePlatform.WindowsEditor)]

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class OpenXRLoaderSetup : LoaderTestSetup<OpenXRLoader, OpenXRSettings>
    {
        protected override string settingsKey => "OpenXRTestSettings";

        private OpenXRFeature[] savedFeatures = null;

        protected virtual bool EnableMockRuntime(bool enable)
        {
            var feature = MockRuntime.Instance;
            if(null == feature)
                return false;

            feature.enabled = enable;
            feature.openxrExtensionStrings = MockRuntime.XR_UNITY_null_gfx;
            feature.priority = 0;
            feature.required = false;
            feature.ignoreValidationErrors = true;

            return true;
        }

        protected void EnableMockDriver()
        {
            var driver = OpenXRSettings.Instance.GetFeature<MockDriver>();
            if(driver != null)
                driver.enabled = true;
        }

        protected void AddExtension(string extensionName)
        {
            MockRuntime.Instance.openxrExtensionStrings += $" {extensionName}";
        }

        private void DisableAllFeatures()
        {
            foreach (var ext in OpenXRSettings.Instance.features)
            {
                ext.enabled = false;
            }
        }

#pragma warning disable CS0618
        public OpenXRLoader Loader => XRGeneralSettings.Instance?.Manager?.loaders[0] as OpenXRLoader;
#pragma warning restore CS0618


        public override void SetupTest()
        {
            base.SetupTest();

#if UNITY_EDITOR
            UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(UnityEditor.BuildTargetGroup.Standalone);
#endif

            EnableMockRuntime(true);
        }

        // NOTE: If you override this function, do NOT add the SetUp test attribute.
        // If you do the overriden function and this function will be called separately
        // and will most likely invalidate your test or even crash Unity.
        [SetUp]
        public virtual void BeforeTest()
        {
            // Cache off the features before we start
            savedFeatures = (OpenXRFeature[])OpenXRSettings.Instance.features.Clone();

            // Disable all features incase some features were enable before the tests started.
            DisableAllFeatures();
            Assert.IsTrue(EnableMockRuntime(true));
#pragma warning disable CS0618
            loader = XRGeneralSettings.Instance?.Manager?.loaders[0] as OpenXRLoader;
            loader.GetRestarter().ShouldCancelQuit = () => false;
#pragma warning restore CS0618
        }

        // NOTE: If you override this function, do NOT add the SetUp test attribute.
        // If you do the overriden function and this function will be called separately
        // and will most likely invalidate your test or even crash Unity.
        [TearDown]
        public virtual void AfterTest()
        {
#pragma warning disable CS0618
            loader = XRGeneralSettings.Instance?.Manager?.loaders[0] as OpenXRLoader;
            loader.GetRestarter().ShouldCancelQuit = null;
#pragma warning restore CS0618

            StopAndShutdown();
            EnableMockRuntime(false);
            MockRuntime.Instance.TestCallback = (methodName, param) => true;

            // Replace the features with the saved fatures
            OpenXRSettings.Instance.features = savedFeatures;
        }

        public override void Setup()
        {
            SetupTest();
            EnableMockRuntime(true);
            base.Setup();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            TearDownTest();
            EnableMockRuntime(false);
        }

        static Dictionary<XrSessionState, HashSet<XrSessionState>> s_AllowedStateTransitions = new Dictionary<XrSessionState, HashSet<XrSessionState>>()
        {
            {XrSessionState.Unknown, new HashSet<XrSessionState>() {XrSessionState.Unknown}},
            {XrSessionState.Idle, new HashSet<XrSessionState>() {XrSessionState.Unknown, XrSessionState.Unknown, XrSessionState.Exiting, XrSessionState.LossPending, XrSessionState.Stopping}},
            {XrSessionState.Ready, new HashSet<XrSessionState>() {XrSessionState.Idle}},
            {XrSessionState.Synchronized, new HashSet<XrSessionState>() {XrSessionState.Ready, XrSessionState.Visible}},
            {XrSessionState.Visible, new HashSet<XrSessionState>() {XrSessionState.Synchronized, XrSessionState.Focused}},
            {XrSessionState.Focused, new HashSet<XrSessionState>() {XrSessionState.Visible}},
            {XrSessionState.Stopping, new HashSet<XrSessionState>() {XrSessionState.Synchronized}},
            {XrSessionState.LossPending, new HashSet<XrSessionState>() {XrSessionState.Unknown, XrSessionState.Idle, XrSessionState.Ready, XrSessionState.Synchronized, XrSessionState.Visible, XrSessionState.Focused, XrSessionState.Stopping, XrSessionState.Exiting, XrSessionState.LossPending}},
            {XrSessionState.Exiting, new HashSet<XrSessionState>() {XrSessionState.Idle}},
        };

        public void CheckValidStateTransition(XrSessionState oldState, XrSessionState newState)
        {
            bool hasNewState = s_AllowedStateTransitions.ContainsKey(newState);
            bool canTransitionTo = s_AllowedStateTransitions[newState].Contains(oldState);

            Debug.LogWarning($"Attempting to transition from {oldState} to {newState}");
            if (!hasNewState)
                Debug.LogError($"Has {newState} : {hasNewState}");

            if (!canTransitionTo)
                Debug.LogError($"Can transition from {oldState} to {newState} : {canTransitionTo}");


            NUnit.Framework.Assert.IsTrue(hasNewState);
            NUnit.Framework.Assert.IsTrue(canTransitionTo);
        }

        protected void ProcessOpenXRMessageLoop() => loader.ProcessOpenXRMessageLoop();
    }
}
