using UnityEngine.Assertions;
using UnityEngine.XR.OpenXR.Features.Mock;

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class WaitForXrFrame  : CustomYieldInstruction
    {
        private int frames = 0;

        public override bool keepWaiting => frames > 0;

        public WaitForXrFrame(int frames)
        {
            this.frames = frames;
            if (frames == 0)
                return;

            // Start waiting for a new frame count
            var driver = OpenXRSettings.Instance.GetFeature<MockDriver>();
            Assert.IsNotNull(driver);
            Assert.IsTrue(driver.enabled);

            MockDriver.onEndFrame += OnEndFrame;
        }

        private void OnEndFrame()
        {
            frames--;
            if (frames > 0)
                return;

            frames = 0;
            MockDriver.onEndFrame -= OnEndFrame;
        }
    }
}