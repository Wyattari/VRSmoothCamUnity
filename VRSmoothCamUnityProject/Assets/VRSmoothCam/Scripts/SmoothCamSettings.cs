using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRSmoothCam
{ 
    [CreateAssetMenu(fileName = "SmoothCamSettings", menuName = "ScriptableObjects/SmoothCamSettings")]
    public class SmoothCamSettings : ScriptableObject
    {
        [Header("Smoothing Settings")]
        public SmoothCamMethods.SmoothingMethod smoothingMethod;
        public float positionDampening = 0.5f;
        public float rotationDampening = 0.09f;
        [Tooltip("Prevents camera from rotating along the Z axis")]
        public bool lockCameraRoll;

        [Space]

        [Header("Monitor Settings")]
        public bool enableMonitor;
        public bool automaticRotateToView;
        public Vector3 monitorPosition = new Vector3(0, 0, 0.5f);
        [Range(0,1)]
        public float monitorTransparency;
        public GameObject monitorPrefab;
        public RenderTexture monitorRenderTexture;

        public delegate void SettingsChanged();
        public static event SettingsChanged OnSettingsChanged;

        private void OnValidate()
        {
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged();
            }
        }
    }
}
