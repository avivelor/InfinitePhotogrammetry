using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.MagicLeap.PlaneJobs
{
    internal struct CopyPlaneResultsJob : IJobParallelFor
    {
        static readonly Quaternion k_MagicLeapToUnityRotation = Quaternion.AngleAxis(-90f, Vector3.right);

        static readonly Quaternion k_UnityToMagicLeapRotation = Quaternion.Inverse(k_MagicLeapToUnityRotation);

        static public Quaternion TransformMLRotationToUnity(Quaternion rotation)
        {
            return rotation * k_MagicLeapToUnityRotation;
        }

        static public Quaternion TransformUnityRotationToML(Quaternion rotation)
        {
            return rotation * k_UnityToMagicLeapRotation;
        }

        [ReadOnly]
        public NativeArray<MLPlane> planesIn;

        [WriteOnly]
        public NativeArray<BoundedPlane> planesOut;

        PlaneAlignment ToUnityAlignment(MLPlanesQueryFlags flags, Quaternion rotation)
        {
            if ((flags & MLPlanesQueryFlags.Vertical) != 0)
            {
                return PlaneAlignment.Vertical;
            }
            else if ((flags & MLPlanesQueryFlags.Horizontal) != 0)
            {
                var normal = rotation * Vector3.up;
                return (normal.y > 0f) ? PlaneAlignment.HorizontalUp : PlaneAlignment.HorizontalDown;
            }
            else
            {
                return PlaneAlignment.NotAxisAligned;
            }
        }

        public void Execute(int index)
        {
            var plane = planesIn[index];
            var rotation = new Quaternion(plane.rotation.x, plane.rotation.y, -plane.rotation.z, -plane.rotation.w);
            rotation = TransformMLRotationToUnity(rotation);

            planesOut[index] = new BoundedPlane
            (
                MagicLeapPlaneSubsystem.GetTrackableId(plane.id), // trackableId
                TrackableId.invalidId, // subsumedBy
                new Pose(
                    new Vector3(plane.position.x, plane.position.y, -plane.position.z),
                    rotation),
                Vector3.zero, // center
                new Vector2(plane.width, plane.height), // size
                ToUnityAlignment(plane.flags, rotation), // alignment
                TrackingState.Tracking, // tracking state
                IntPtr.Zero // native pointer
            );
        }
    }
}
