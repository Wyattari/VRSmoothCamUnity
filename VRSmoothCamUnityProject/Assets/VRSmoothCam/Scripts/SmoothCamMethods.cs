using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRSmoothCam
{
    public class SmoothCamMethods
    {
        public enum SmoothingMethod { SmoothDamp, ContinuousLerp }

        public static Transform SmoothDamp(Transform transform, Transform target, SmoothCamSettings settings)
        {
            var velocity = Vector3.zero;
            transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, settings.positionDampening);

            float angularVelocity = 0f;
            float delta = Quaternion.Angle(transform.rotation, target.rotation);
            if (delta > 0f)
            {
                float t = Mathf.SmoothDampAngle(delta, 0.0f, ref angularVelocity, settings.rotationDampening);
                t = 1.0f - (t / delta);
                transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, t);
            }

            if(settings.lockCameraRoll) // broken when looking up and down
            { 
                float lockZ = 0f;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, lockZ);
            }
            return transform;
        }

        public static Transform ContinuousLerp(Transform transform, Transform target, SmoothCamSettings settings)
        {
            return transform;
        }

    }
}