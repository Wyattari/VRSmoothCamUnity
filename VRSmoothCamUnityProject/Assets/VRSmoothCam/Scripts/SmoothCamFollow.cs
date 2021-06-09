using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRSmoothCam
{ 
    [RequireComponent(typeof(Camera))]
    public class SmoothCamFollow : MonoBehaviour
    {
        [Tooltip("Place the eye or target you want to follow here")]
        [SerializeField] private GameObject followTarget;

        [Tooltip("Put SmoothCamSettings ScriptableObject here")]
        [SerializeField] private SmoothCamSettings settings;

        private Camera thisCamera;
        private GameObject monitorPrefab;
        private Renderer monitorRenderer;
        private Transform smoothedTransform;

        private void OnEnable()
        {
            SmoothCamSettings.OnSettingsChanged += UpdateSettings;
        }

        private void Start()
        {
            if (followTarget == null)
            {
                Debug.LogError("VRSmoothCam doesn't have a follow target");
                followTarget = new GameObject("Blank Target");
            }
            thisCamera = GetComponent<Camera>();
            UpdateSettings();
        }

        private void LateUpdate()
        {
            switch (settings.smoothingMethod)
            {
                case SmoothCamMethods.SmoothingMethod.SmoothDamp:
                    smoothedTransform = SmoothCamMethods.SmoothDamp(transform, followTarget.transform, settings);
                    transform.position = smoothedTransform.position;
                    transform.rotation = smoothedTransform.rotation;
                    break;
                case SmoothCamMethods.SmoothingMethod.ContinuousLerp:
                    smoothedTransform = SmoothCamMethods.ContinuousLerp(transform, followTarget.transform, settings);
                    transform.position = smoothedTransform.position;
                    transform.rotation = smoothedTransform.rotation;
                    break;
            }
        }

        void UpdateSettings()
        {
            if (settings.enableMonitor && monitorPrefab == null)
            {
                CreateMonitor();
                UpdateMonitorSettings();
            } 
            else if (settings.enableMonitor)
            {
                UpdateMonitorSettings();
            }
            else if (thisCamera.targetTexture != null)
            {
                DestroyMonitor();
            }
        }

        void CreateMonitor()
        {
            thisCamera.targetTexture = settings.monitorRenderTexture;
            monitorPrefab = Instantiate(settings.monitorPrefab, followTarget.transform, false);
        }

        void UpdateMonitorSettings()
        {
            monitorPrefab.transform.GetChild(1).localPosition = settings.monitorPosition;
            monitorRenderer = monitorPrefab.transform.GetChild(1).GetComponent<Renderer>();
            var transparentColor = new Color(1, 1, 1, settings.monitorTransparency);
            monitorRenderer.material.color = transparentColor;
            monitorRenderer.material.SetColor("_EmissionColor", transparentColor);
            if (settings.automaticRotateToView)
            {
                monitorPrefab.transform.GetChild(1).LookAt(2*monitorPrefab.transform.GetChild(1).position - followTarget.transform.position);
            }
            else
            {
                monitorPrefab.transform.GetChild(1).localRotation = Quaternion.identity;
            }
        }

        void DestroyMonitor()
        {
            thisCamera.targetTexture.Release();
            thisCamera.targetTexture = null;
            if (monitorPrefab != null)
                Destroy(monitorPrefab);
        }

        private void OnDisable()
        {
            SmoothCamSettings.OnSettingsChanged -= UpdateSettings;
        }
    }
}
