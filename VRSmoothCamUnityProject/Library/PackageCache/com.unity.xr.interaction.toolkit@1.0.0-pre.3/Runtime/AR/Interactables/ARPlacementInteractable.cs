//-----------------------------------------------------------------------
// <copyright originalFile="AndyPlacementManipulator.cs" company="Google">
// <renamed file="ARPlacementInteractable.cs">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

// Modifications copyright © 2020 Unity Technologies ApS

#if !AR_FOUNDATION_PRESENT && !PACKAGE_DOCS_GENERATION

// Stub class definition used to fool version defines that this MonoScript exists (fixed in 19.3)
namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// <see cref="UnityEvent"/> that responds to changes of hover and selection by this interactor.
    /// </summary>
    public class ARPlacementInteractable {}
}

#else

using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// <see cref="UnityEvent"/> that is invoked when an object is placed.
    /// </summary>
    [Serializable, Obsolete("ARObjectPlacedEvent has been deprecated. Use ARObjectPlacementEvent instead.")]
    public class ARObjectPlacedEvent : UnityEvent<ARPlacementInteractable, GameObject>
    {
    }

    /// <summary>
    /// <see cref="UnityEvent"/> that is invoked when an object is placed.
    /// </summary>
    [Serializable]
    public class ARObjectPlacementEvent : UnityEvent<ARObjectPlacementEventArgs>
    {
    }

    /// <summary>
    /// Event data associated with the event when an object is placed.
    /// </summary>
    public class ARObjectPlacementEventArgs
    {
        /// <summary>
        /// The Interactable that placed the object.
        /// </summary>
        public ARPlacementInteractable placementInteractable { get; set; }

        /// <summary>
        /// The object that was placed.
        /// </summary>
        public GameObject placementObject { get; set; }
    }

    /// <summary>
    /// Controls the placement of Andy objects via a tap gesture.
    /// </summary>
    [HelpURL(XRHelpURLConstants.k_ARPlacementInteractable)]
    public class ARPlacementInteractable : ARBaseGestureInteractable
    {
        [SerializeField]
        [Tooltip("A GameObject to place when a raycast from a user touch hits a plane.")]
        GameObject m_PlacementPrefab;

        /// <summary>
        /// A <see cref="GameObject"/> to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject placementPrefab
        {
            get => m_PlacementPrefab;
            set => m_PlacementPrefab = value;
        }

        [SerializeField]
        [Tooltip("The LayerMask that is used during an additional raycast when a user touch does not hit any AR trackable planes.")]
        LayerMask m_FallbackLayerMask;

        /// <summary>
        /// The <see cref="LayerMask"/> that is used during an additional raycast
        /// when a user touch does not hit any AR trackable planes.
        /// </summary>
        public LayerMask fallbackLayerMask
        {
            get => m_FallbackLayerMask;
            set => m_FallbackLayerMask = value;
        }

        [SerializeField]
        ARObjectPlacementEvent m_ObjectPlaced = new ARObjectPlacementEvent();

        /// <summary>
        /// Gets or sets the event that is called when this Interactable places a new <see cref="GameObject"/> in the world.
        /// </summary>
        public ARObjectPlacementEvent objectPlaced
        {
            get => m_ObjectPlaced;
            set => m_ObjectPlaced = value;
        }

#pragma warning disable 618
        [SerializeField]
        ARObjectPlacedEvent m_OnObjectPlaced = new ARObjectPlacedEvent();

        /// <summary>
        /// Gets or sets the event that is called when this Interactable places a new <see cref="GameObject"/> in the world.
        /// </summary>
        [Obsolete("onObjectPlaced has been deprecated. Use objectPlaced with updated signature instead.")]
        public ARObjectPlacedEvent onObjectPlaced
        {
            get => m_OnObjectPlaced;
            set => m_OnObjectPlaced = value;
        }
#pragma warning restore 618

        readonly ARObjectPlacementEventArgs m_ObjectPlacementEventArgs = new ARObjectPlacementEventArgs();

        static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        /// <summary>
        /// Gets the pose for the object to be placed from a raycast hit triggered by a <see cref="TapGesture"/>.
        /// </summary>
        /// <param name="gesture">The tap gesture that triggers the raycast.</param>
        /// <param name="pose">When this method returns, contains the pose of the placement object based on the raycast hit.</param>
        /// <returns>Returns <see langword="true"/> if there is a valid raycast hit that hit the front of a plane.
        /// Otherwise, returns <see langword="false"/>.</returns>
        protected virtual bool TryGetPlacementPose(TapGesture gesture, out Pose pose)
        {
            // Raycast against the location the player touched to search for planes.
            if (GestureTransformationUtility.Raycast(gesture.startPosition, s_Hits, arSessionOrigin, TrackableType.PlaneWithinPolygon, m_FallbackLayerMask))
            {
                pose = s_Hits[0].pose;

                // Use hit pose and camera pose to check if hit test is from the
                // back of the plane, if it is, no need to create the anchor.
                // ReSharper disable once LocalVariableHidesMember -- hide deprecated camera property
                var camera = arSessionOrigin != null ? arSessionOrigin.camera : Camera.main;
                if (camera == null)
                    return false;

                return Vector3.Dot(camera.transform.position - pose.position, pose.rotation * Vector3.up) >= 0f;
            }

            pose = default;
            return false;
        }

        /// <summary>
        /// Instantiates the placement object and positions it at the desired pose.
        /// </summary>
        /// <param name="pose">The pose at which the placement object will be instantiated.</param>
        /// <returns>Returns the instantiated placement object at the input pose.</returns>
        /// <seealso cref="placementPrefab"/>
        protected virtual GameObject PlaceObject(Pose pose)
        {
            var placementObject = Instantiate(m_PlacementPrefab, pose.position, pose.rotation);

            // Create anchor to track reference point and set it as the parent of placementObject.
            var anchor = new GameObject("PlacementAnchor").transform;
            anchor.position = pose.position;
            anchor.rotation = pose.rotation;
            placementObject.transform.parent = anchor;

            // Use Trackables object in scene to use as parent
            if (arSessionOrigin != null && arSessionOrigin.trackablesParent != null)
                anchor.parent = arSessionOrigin.trackablesParent;

            return placementObject;
        }

        /// <summary>
        /// This method is called after an object has been placed.
        /// </summary>
        /// <param name="args">Event data containing a reference to the instantiated placement object.</param>
        protected virtual void OnObjectPlaced(ARObjectPlacementEventArgs args)
        {
            m_ObjectPlaced?.Invoke(args);
            m_OnObjectPlaced?.Invoke(args.placementInteractable, args.placementObject);
        }

        /// <inheritdoc />
        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            return gesture.targetObject == null;
        }

        /// <inheritdoc />
        protected override void OnEndManipulation(TapGesture gesture)
        {
            base.OnEndManipulation(gesture);

            if (gesture.isCanceled)
                return;

            if (arSessionOrigin == null)
                return;

            if (TryGetPlacementPose(gesture, out var pose))
            {
                var placementObject = PlaceObject(pose);

                m_ObjectPlacementEventArgs.placementInteractable = this;
                m_ObjectPlacementEventArgs.placementObject = placementObject;
                OnObjectPlaced(m_ObjectPlacementEventArgs);
            }
        }
    }
}

#endif
