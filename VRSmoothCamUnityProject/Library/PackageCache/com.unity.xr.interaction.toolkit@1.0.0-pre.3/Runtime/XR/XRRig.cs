using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The XR Rig component is typically attached to the base object of the XR Rig,
    /// and stores the <see cref="GameObject"/> that will be manipulated via locomotion.
    /// It is also used for offsetting the camera.
    /// </summary>
    [AddComponentMenu("XR/XR Rig")]
    [DisallowMultipleComponent]
    [HelpURL(XRHelpURLConstants.k_XRRig)]
    public class XRRig : MonoBehaviour
    {
        const float k_DefaultCameraYOffset = 1.36144f;

        [SerializeField]
        [Tooltip("The \"Rig\" GameObject is used to refer to the base of the XR Rig, by default it is this GameObject." +
            " This is the GameObject that will be manipulated via locomotion.")]
        GameObject m_RigBaseGameObject;

        /// <summary>
        /// The "Rig" <see cref="GameObject"/> is used to refer to the base of the XR Rig, by default it is this <see cref="GameObject"/>.
        /// This is the <see cref="GameObject"/> that will be manipulated via locomotion.
        /// </summary>
        public GameObject rig
        {
            get => m_RigBaseGameObject;
            set => m_RigBaseGameObject = value;
        }

        [SerializeField]
        [Tooltip("The GameObject to move to desired height off the floor (defaults to this object if none provided).")]
        GameObject m_CameraFloorOffsetObject;

        /// <summary>
        /// The <see cref="GameObject"/> to move to desired height off the floor (defaults to this object if none provided).
        /// </summary>
        public GameObject cameraFloorOffsetObject
        {
            get => m_CameraFloorOffsetObject;
            set
            {
                m_CameraFloorOffsetObject = value;
                MoveOffsetHeight();
            }
        }

        [SerializeField]
        [Tooltip("The GameObject that contains the camera, this is usually the \"Head\" of XR rigs.")]
        GameObject m_CameraGameObject;

        /// <summary>
        /// The <see cref="GameObject"/> that contains the camera, this is usually the "Head" of XR rigs.
        /// </summary>
        public GameObject cameraGameObject
        {
            get => m_CameraGameObject;
            set => m_CameraGameObject = value;
        }

        [SerializeField]
        [Tooltip("The type of tracking origin to use for this Rig. Tracking origins identify where (0, 0, 0) is in the world of tracking.")]
        TrackingOriginModeFlags m_TrackingOriginMode = TrackingOriginModeFlags.Unknown;

        /// <summary>
        /// The type of tracking origin to use for this Rig. Tracking origins identify where (0, 0, 0) is in the world
        /// of tracking. Not all devices support all tracking spaces; if the selected tracking space is not set it will
        /// fall back to <see cref="TrackingSpaceType.Stationary"/>.
        /// </summary>
        public TrackingOriginModeFlags trackingOriginMode
        {
            get => m_TrackingOriginMode;
            set
            {
                m_TrackingOriginMode = value;
                TryInitializeCamera();
            }
        }

#pragma warning disable 0618 // Disable Obsolete warnings for TrackingSpaceType, explicitly to read in old data and upgrade.
        [SerializeField]
        [Tooltip("Set if the XR experience is Room Scale or Stationary.")]
        TrackingSpaceType m_TrackingSpace = TrackingSpaceType.Stationary;

        /// <summary>
        /// Whether the experience is Room Scale or Stationary. Not all devices support all tracking spaces; if the
        /// selected tracking space is not set it will fall back to Stationary.
        /// </summary>
        [Obsolete("XRRig.trackingSpace is obsolete.  Please use XRRig.trackingOriginMode.")]
        public TrackingSpaceType trackingSpace
        {
            get => m_TrackingSpace;
            set
            {
                m_TrackingSpace = value;
                TryInitializeCamera();
            }
        }
#pragma warning restore 0618

        [SerializeField]
        [Tooltip("Camera Height to be used when in \"Device\" tracking origin mode to define the height of the user from the floor.")]
        float m_CameraYOffset = k_DefaultCameraYOffset;


        /// <summary>
        /// The amount the camera is offset from the floor (by moving the camera offset object).
        /// </summary>
        public float cameraYOffset
        {
            get => m_CameraYOffset;
            set
            {
                m_CameraYOffset = value;
                MoveOffsetHeight();
            }
        }

        /// <summary>
        /// (Read Only) The rig's local position in camera space.
        /// </summary>
        public Vector3 rigInCameraSpacePos => m_CameraGameObject.transform.InverseTransformPoint(m_RigBaseGameObject.transform.position);

        /// <summary>
        /// (Read Only) The camera's local position in rig space.
        /// </summary>
        public Vector3 cameraInRigSpacePos => m_RigBaseGameObject.transform.InverseTransformPoint(m_CameraGameObject.transform.position);

        /// <summary>
        /// (Read Only) The camera's height relative to the rig.
        /// </summary>
        public float cameraInRigSpaceHeight => cameraInRigSpacePos.y;

        /// <summary>
        /// Used to cache the input subsystems without creating additional GC allocations.
        /// </summary>
        static readonly List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();

        // Bookkeeping to track lazy initialization of the tracking space type.
        bool m_CameraInitialized;
        bool m_CameraInitializing;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnValidate()
        {
            UpgradeTrackingSpaceToTrackingOriginMode();

            if (m_RigBaseGameObject == null)
                m_RigBaseGameObject = gameObject;

            if (Application.isPlaying && isActiveAndEnabled)
            {
                // Respond to the mode changing by re-initializing the camera,
                // or just update the offset height in order to avoid recentering.
                if (IsModeStale())
                    TryInitializeCamera();
                else
                    MoveOffsetHeight();
            }

            bool IsModeStale()
            {
                if (s_InputSubsystems.Count > 0)
                {
                    foreach (var inputSubsystem in s_InputSubsystems)
                    {
                        if (inputSubsystem != null && inputSubsystem.GetTrackingOriginMode() != m_TrackingOriginMode)
                        {
                            return true;
                        }
                    }
                }
                else
                {
#pragma warning disable 0618 // Disable Obsolete warnings for TrackingSpaceType, Input Subsystems not present.
                    switch (m_TrackingOriginMode)
                    {
                        case TrackingOriginModeFlags.Floor:
                            return XRDevice.GetTrackingSpaceType() != TrackingSpaceType.RoomScale;
                        case TrackingOriginModeFlags.Device:
                            return XRDevice.GetTrackingSpaceType() != TrackingSpaceType.Stationary;
                    }
#pragma warning restore 0618
                }

                return false;
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            if (m_CameraFloorOffsetObject == null)
            {
                Debug.LogWarning("No Camera Floor Offset Object specified for XR Rig, using attached GameObject.", this);
                m_CameraFloorOffsetObject = gameObject;
            }
            if (m_CameraGameObject == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    m_CameraGameObject = mainCamera.gameObject;
                else
                    Debug.LogWarning("No Main Camera is found for XR Rig, please assign the Camera GameObject field manually.", this);
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Start()
        {
            TryInitializeCamera();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            foreach (var inputSubsystem in s_InputSubsystems)
            {
                if (inputSubsystem != null)
                    inputSubsystem.trackingOriginUpdated -= OnInputSubsystemTrackingOriginUpdated;
            }
        }

        /// <summary>
        /// Called when gizmos are drawn.
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            if (m_RigBaseGameObject != null)
            {
                // Draw XR Rig box
                Gizmos.color = Color.green;
                GizmoHelpers.DrawWireCubeOriented(m_RigBaseGameObject.transform.position, m_RigBaseGameObject.transform.rotation, 3f);
                GizmoHelpers.DrawAxisArrows(m_RigBaseGameObject.transform, 0.5f);
            }

            if (m_CameraFloorOffsetObject != null)
            {
                GizmoHelpers.DrawAxisArrows(m_CameraFloorOffsetObject.transform, 0.5f);
            }

            if (m_CameraGameObject != null)
            {
                var cameraPosition = m_CameraGameObject.transform.position;
                Gizmos.color = Color.red;
                GizmoHelpers.DrawWireCubeOriented(cameraPosition, m_CameraGameObject.transform.rotation, 0.1f);
                GizmoHelpers.DrawAxisArrows(m_CameraGameObject.transform, 0.5f);

                if (m_RigBaseGameObject != null)
                {
                    var floorPos = cameraPosition;
                    floorPos.y = m_RigBaseGameObject.transform.position.y;
                    Gizmos.DrawLine(floorPos, cameraPosition);
                }
            }
        }

        /// <summary>
        /// Utility helper to migrate from <see cref="TrackingSpaceType"/> to <see cref="TrackingOriginModeFlags"/> seamlessly.
        /// </summary>
        void UpgradeTrackingSpaceToTrackingOriginMode()
        {
#pragma warning disable 0618 // Disable Obsolete warnings for TrackingSpaceType, explicitly to allow a proper upgrade path.
            if (m_TrackingOriginMode == TrackingOriginModeFlags.Unknown && m_TrackingSpace <= TrackingSpaceType.RoomScale)
            {
                switch (m_TrackingSpace)
                {
                    case TrackingSpaceType.RoomScale:
                        m_TrackingOriginMode = TrackingOriginModeFlags.Floor;
                        break;
                    case TrackingSpaceType.Stationary:
                        m_TrackingOriginMode = TrackingOriginModeFlags.Device;
                        break;
                }

                // Set to an invalid value to indicate the value has been migrated.
                m_TrackingSpace = (TrackingSpaceType)3;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif // UNITY_EDITOR
#pragma warning restore 0618
            }
        }

        /// <summary>
        /// Repeatedly attempt to initialize the camera.
        /// </summary>
        void TryInitializeCamera()
        {
            if (!Application.isPlaying)
                return;

            m_CameraInitialized = SetupCamera();
            if (!m_CameraInitialized & !m_CameraInitializing)
                StartCoroutine(RepeatInitializeCamera());
        }

        IEnumerator RepeatInitializeCamera()
        {
            m_CameraInitializing = true;
            while (!m_CameraInitialized)
            {
                yield return null;
                if (!m_CameraInitialized)
                    m_CameraInitialized = SetupCamera();
            }
            m_CameraInitializing = false;
        }

        /// <summary>
        /// Handles re-centering and off-setting the camera in space depending on which tracking space it is setup in.
        /// </summary>
        bool SetupCamera()
        {
            var initialized = true;

            SubsystemManager.GetInstances(s_InputSubsystems);
            if (s_InputSubsystems.Count > 0)
            {
                foreach (var inputSubsystem in s_InputSubsystems)
                {
                    if (SetupCamera(inputSubsystem))
                    {
                        // It is possible this could happen more than
                        // once so unregister the callback first just in case.
                        inputSubsystem.trackingOriginUpdated -= OnInputSubsystemTrackingOriginUpdated;
                        inputSubsystem.trackingOriginUpdated += OnInputSubsystemTrackingOriginUpdated;
                    }
                    else
                    {
                        initialized = false;
                    }
                }
            }
            else
            {
#pragma warning disable 0618 // Disable Obsolete warnings for TrackingSpaceType, Input Subsystems not present.
                switch (m_TrackingOriginMode)
                {
                    case TrackingOriginModeFlags.Floor:
                        initialized = SetupCameraLegacy(TrackingSpaceType.RoomScale);
                        break;
                    case TrackingOriginModeFlags.Device:
                        initialized = SetupCameraLegacy(TrackingSpaceType.Stationary);
                        break;
                }
#pragma warning restore 0618
            }

            return initialized;
        }

        void OnInputSubsystemTrackingOriginUpdated(XRInputSubsystem inputSubsystem)
        {
            m_TrackingOriginMode = inputSubsystem.GetTrackingOriginMode();
            MoveOffsetHeight();
        }

        bool SetupCamera(XRInputSubsystem inputSubsystem)
        {
            if (inputSubsystem == null)
                return false;

            switch (m_TrackingOriginMode)
            {
                case TrackingOriginModeFlags.Floor:
                case TrackingOriginModeFlags.Device:
                {
                    var supportedModes = inputSubsystem.GetSupportedTrackingOriginModes();

                    // We need to check for Unknown because we may not be in a state where we can read this data yet.
                    if (supportedModes == TrackingOriginModeFlags.Unknown)
                        return false;

                    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags -- Treated like Flags enum when querying supported modes
                    if ((supportedModes & m_TrackingOriginMode) == 0)
                    {
                        Debug.LogWarning($"Attempting to set the tracking space to {m_TrackingOriginMode}, but that is not supported by the SDK." +
                            $" Supported types: {supportedModes:F}.", this);
                        return true;
                    }

                    var trackingSettingsSet = inputSubsystem.TrySetTrackingOriginMode(m_TrackingOriginMode);
                    if (m_TrackingOriginMode == TrackingOriginModeFlags.Device)
                        trackingSettingsSet = inputSubsystem.TryRecenter();

                    if (trackingSettingsSet)
                        MoveOffsetHeight();

                    return trackingSettingsSet;
                }
                default:
                    return false;
            }
        }

#pragma warning disable 0618 // Disable Obsolete warnings for TrackingSpaceType, explicitly to allow for using legacy data if available.
        bool SetupCameraLegacy(TrackingSpaceType trackingSpaceType)
        {
            var trackingSettingsSet = XRDevice.SetTrackingSpaceType(trackingSpaceType);
            if (trackingSpaceType == TrackingSpaceType.Stationary)
                InputTracking.Recenter();

            if (trackingSettingsSet)
                MoveOffsetHeight();

            return trackingSettingsSet;
        }
#pragma warning restore 0618

        /// <summary>
        /// Sets the height of the camera based on the current tracking origin mode by updating the <see cref="cameraFloorOffsetObject"/>.
        /// </summary>
        void MoveOffsetHeight()
        {
            if (!Application.isPlaying)
                return;

            switch (m_TrackingOriginMode)
            {
                case TrackingOriginModeFlags.Floor:
                    MoveOffsetHeight(0f);
                    break;
                case TrackingOriginModeFlags.Device:
                    MoveOffsetHeight(m_CameraYOffset);
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Sets the height of the camera to the given <paramref name="y"/> value by updating the <see cref="cameraFloorOffsetObject"/>.
        /// </summary>
        /// <param name="y">The local y-position to set.</param>
        void MoveOffsetHeight(float y)
        {
            if (m_CameraFloorOffsetObject != null)
            {
                var offsetTransform = m_CameraFloorOffsetObject.transform;
                var desiredPosition = offsetTransform.localPosition;
                desiredPosition.y = y;
                offsetTransform.localPosition = desiredPosition;
            }
        }

        /// <summary>
        /// Rotates the rig object around the camera object by the provided <paramref name="angleDegrees"/>.
        /// This rotation only occurs around the rig's Up vector
        /// </summary>
        /// <param name="angleDegrees">The amount of rotation in degrees.</param>
        /// <returns>Returns <see langword="true"/> if the rotation is performed. Otherwise, returns <see langword="false"/>.</returns>
        public bool RotateAroundCameraUsingRigUp(float angleDegrees)
        {
            return RotateAroundCameraPosition(m_RigBaseGameObject.transform.up, angleDegrees);
        }

        /// <summary>
        /// Rotates the rig object around the camera object's position in world space using the provided <paramref name="vector"/>
        /// as the rotation axis. The rig object is rotated by the amount of degrees provided in <paramref name="angleDegrees"/>.
        /// </summary>
        /// <param name="vector">The axis of the rotation.</param>
        /// <param name="angleDegrees">The amount of rotation in degrees.</param>
        /// <returns>Returns <see langword="true"/> if the rotation is performed. Otherwise, returns <see langword="false"/>.</returns>
        public bool RotateAroundCameraPosition(Vector3 vector, float angleDegrees)
        {
            if (m_CameraGameObject == null || m_RigBaseGameObject == null)
            {
                return false;
            }

            // Rotate around the camera position
            m_RigBaseGameObject.transform.RotateAround(m_CameraGameObject.transform.position, vector, angleDegrees);

            return true;
        }

        /// <summary>
        /// This function will rotate the rig object such that the rig's up vector will match the provided vector.
        /// </summary>
        /// <param name="destinationUp">the vector to which the rig object's up vector will be matched.</param>
        /// <returns>Returns <see langword="true"/> if the rotation is performed or the vectors have already been matched.
        /// Otherwise, returns <see langword="false"/>.</returns>
        public bool MatchRigUp(Vector3 destinationUp)
        {
            if (m_RigBaseGameObject == null)
            {
                return false;
            }

            if (m_RigBaseGameObject.transform.up == destinationUp)
                return true;

            var rigUp = Quaternion.FromToRotation(m_RigBaseGameObject.transform.up, destinationUp);
            m_RigBaseGameObject.transform.rotation = rigUp * transform.rotation;

            return true;
        }

        /// <summary>
        /// This function will rotate the rig object around the camera object using the <paramref name="destinationUp"/> vector such that:
        /// <list type="bullet">
        /// <item>The camera will look at the area in the direction of the <paramref name="destinationForward"/></item>
        /// <item>The projection of camera's forward vector on the plane with the normal <paramref name="destinationUp"/> will be in the direction of <paramref name="destinationForward"/></item>
        /// <item>The up vector of the rig object will match the provided <paramref name="destinationUp"/> vector (note that the camera's Up vector can not be manipulated)</item>
        /// </list>
        /// </summary>
        /// <param name="destinationUp">The up vector that the rig's up vector will be matched to.</param>
        /// <param name="destinationForward">The forward vector that will be matched to the projection of the camera's forward vector on the plane with the normal <paramref name="destinationUp"/>.</param>
        /// <returns>Returns <see langword="true"/> if the rotation is performed. Otherwise, returns <see langword="false"/>.</returns>
        public bool MatchRigUpCameraForward(Vector3 destinationUp, Vector3 destinationForward)
        {
            if (m_CameraGameObject != null && MatchRigUp(destinationUp))
            {
                // Project current camera's forward vector on the destination plane, whose normal vector is destinationUp.
                var projectedCamForward = Vector3.ProjectOnPlane(cameraGameObject.transform.forward, destinationUp).normalized;

                // The angle that we want the rig to rotate is the signed angle between projectedCamForward and destinationForward, after the up vectors are matched.
                var signedAngle = Vector3.SignedAngle(projectedCamForward, destinationForward, destinationUp);

                RotateAroundCameraPosition(destinationUp, signedAngle);

                return true;
            }

            return false;
        }

        /// <summary>
        /// This function will rotate the rig object around the camera object using the <paramref name="destinationUp"/> vector such that:
        /// <list type="bullet">
        /// <item>The forward vector of the rig object, which is the direction the player moves in Unity when walking forward in the physical world, will match the provided <paramref name="destinationUp"/> vector</item>
        /// <item>The up vector of the rig object will match the provided <paramref name="destinationUp"/> vector</item>
        /// </list>
        /// </summary>
        /// <param name="destinationUp">The up vector that the rig's up vector will be matched to.</param>
        /// <param name="destinationForward">The forward vector that will be matched to the forward vector of the rig object,
        /// which is the direction the player moves in Unity when walking forward in the physical world.</param>
        /// <returns>Returns <see langword="true"/> if the rotation is performed. Otherwise, returns <see langword="false"/>.</returns>
        public bool MatchRigUpRigForward (Vector3 destinationUp, Vector3 destinationForward)
        {
            if (m_RigBaseGameObject != null && MatchRigUp(destinationUp))
            {
                // The angle that we want the rig to rotate is the signed angle between the rig's forward and destinationForward, after the up vectors are matched.
                var signedAngle = Vector3.SignedAngle(m_RigBaseGameObject.transform.forward, destinationForward, destinationUp);

                RotateAroundCameraPosition(destinationUp, signedAngle);

                return true;
            }

            return false;
        }

        /// <summary>
        /// This function moves the camera to the world location provided by desiredWorldLocation.
        /// It does this by moving the rig object so that the camera's world location matches the desiredWorldLocation
        /// </summary>
        /// <param name="desiredWorldLocation">the position in world space that the camera should be moved to</param>
        /// <returns>Returns <see langword="true"/> if the move is performed. Otherwise, returns <see langword="false"/>.</returns>
        public bool MoveCameraToWorldLocation(Vector3 desiredWorldLocation)
        {
            if (m_CameraGameObject == null)
            {
                return false;
            }

            var rot = Matrix4x4.Rotate(cameraGameObject.transform.rotation);
            var delta = rot.MultiplyPoint3x4(rigInCameraSpacePos);
            m_RigBaseGameObject.transform.position = delta + desiredWorldLocation;

            return true;
        }
    }
}
