﻿using System;
using UnityEngine.SpatialTracking;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// <see cref="InteractionState"/> type to hold current state for a given interaction.
    /// </summary>
    [Serializable]
    public struct InteractionState
    {
        bool m_Active;

        /// <summary>
        /// Whether it is currently on.
        /// </summary>
        public bool active
        {
            get => m_Active;
            set => m_Active = value;
        }

        bool m_ActivatedThisFrame;

        /// <summary>
        /// Whether the interaction state was activated this frame.
        /// </summary>
        public bool activatedThisFrame
        {
            get => m_ActivatedThisFrame;
            set => m_ActivatedThisFrame = value;
        }

        bool m_DeactivatedThisFrame;

        /// <summary>
        /// Whether the interaction state was deactivated this frame.
        /// </summary>
        public bool deactivatedThisFrame
        {
            get => m_DeactivatedThisFrame;
            set => m_DeactivatedThisFrame = value;
        }

        /// <summary>
        /// Whether the interaction state was deactivated this frame.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        [Obsolete("deActivatedThisFrame has been deprecated. Use deactivatedThisFrame instead. (UnityUpgradable) -> deactivatedThisFrame")]
        public bool deActivatedThisFrame
        {
            get => deactivatedThisFrame;
            set => deactivatedThisFrame = value;
        }
#pragma warning restore IDE1006

        /// <summary>
        /// Resets the interaction states that are based on whether they occurred "this frame".
        /// </summary>
        /// <seealso cref="activatedThisFrame"/>
        /// <seealso cref="deactivatedThisFrame"/>
        public void ResetFrameDependent()
        {
            activatedThisFrame = false;
            deactivatedThisFrame = false;
        }

        /// <summary>
        ///  Resets the interaction states that are based on whether they occurred "this frame".
        /// </summary>
        [Obsolete("Reset has been renamed. Use ResetFrameDependent instead. (UnityUpgradable) -> ResetFrameDependent()")]
        public void Reset() => ResetFrameDependent();
    }

    /// <summary>
    /// Represents the current state of the <see cref="XRBaseController"/>.
    /// </summary>
    [Serializable]
    public class XRControllerState
    {
        /// <summary>
        /// The time value for this controller.
        /// </summary>
        public double time;

        /// <summary>
        /// The pose data flags of the controller.
        /// </summary>
        public PoseDataFlags poseDataFlags;

        /// <summary>
        /// The position of the controller.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The rotation of the controller.
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        /// State of selection interaction state.
        /// </summary>
        public InteractionState selectInteractionState;

        /// <summary>
        /// State of activate interaction state.
        /// </summary>
        public InteractionState activateInteractionState;

        /// <summary>
        /// State of UI press interaction state.
        /// </summary>
        public InteractionState uiPressInteractionState;

        /// <summary>
        /// Initializes and returns an instance of <see cref="XRControllerState"/>.
        /// </summary>
        public XRControllerState()
        {
            this.time = 0d;
            this.poseDataFlags = PoseDataFlags.Rotation | PoseDataFlags.Position;
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Initializes and returns an instance of <see cref="XRControllerState"/>.
        /// </summary>
        /// <param name="value"> The <see cref="XRControllerState"/> object used to create this object.</param>
        public XRControllerState(XRControllerState value)
        {
            this.time = value.time;
            this.poseDataFlags = value.poseDataFlags;
            this.position = value.position;
            this.rotation = value.rotation;
            this.selectInteractionState = value.selectInteractionState;
            this.activateInteractionState = value.activateInteractionState;
            this.uiPressInteractionState = value.uiPressInteractionState;
        }

        /// <summary>
        /// Initializes and returns an instance of <see cref="XRControllerState"/>.
        /// </summary>
        /// <param name="time">The time value for this controller.</param>
        /// <param name="position">The position for this controller.</param>
        /// <param name="rotation">The rotation for this controller.</param>
        /// <param name="selectActive">Whether select is active or not.</param>
        /// <param name="activateActive">Whether activate is active or not.</param>
        /// <param name="pressActive">Whether press is active or not.</param>
        public XRControllerState(double time, Vector3 position, Quaternion rotation, bool selectActive, bool activateActive, bool pressActive)
        {
            this.time = time;
            this.poseDataFlags = PoseDataFlags.Rotation | PoseDataFlags.Position;
            this.position = position;
            this.rotation = rotation;

            this.selectInteractionState.ResetFrameDependent();
            SimulateInteractionState(selectActive, ref this.selectInteractionState);
            this.activateInteractionState.ResetFrameDependent();
            SimulateInteractionState(activateActive, ref this.activateInteractionState);
            this.uiPressInteractionState.ResetFrameDependent();
            SimulateInteractionState(pressActive, ref this.uiPressInteractionState);
        }

        void SimulateInteractionState(bool pressed,  ref InteractionState interactionState)
        {
            if (pressed)
            {
                if (!interactionState.active)
                {
                    interactionState.activatedThisFrame = true;
                    interactionState.active = true;
                }
            }
            else
            {
                if (interactionState.active)
                {
                    interactionState.deactivatedThisFrame = true;
                    interactionState.active = false;
                }
            }
        }

        /// <summary>
        /// Resets all the interaction states that are based on whether they occurred "this frame".
        /// </summary>
        /// <seealso cref="InteractionState.ResetFrameDependent"/>
        public void ResetFrameDependentStates()
        {
            selectInteractionState.ResetFrameDependent();
            activateInteractionState.ResetFrameDependent();
            uiPressInteractionState.ResetFrameDependent();
        }

        /// <summary>
        /// Resets all the interaction states that are based on whether they occurred "this frame".
        /// </summary>
        [Obsolete("ResetInputs has been renamed. Use ResetFrameDependentStates instead. (UnityUpgradable) -> ResetFrameDependentStates()")]
        public void ResetInputs() => ResetFrameDependentStates();

        /// <summary>
        /// Converts state data to a string.
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString() => $"time: {time}, position: {position}, rotation: {rotation}, selectActive: {selectInteractionState.active}, activateActive: {activateInteractionState.active}, pressActive: {uiPressInteractionState.active}";
    }
}
