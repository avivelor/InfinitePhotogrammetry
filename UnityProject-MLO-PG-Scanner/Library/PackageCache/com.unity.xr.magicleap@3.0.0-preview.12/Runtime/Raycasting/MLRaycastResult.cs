using System.Runtime.InteropServices;

namespace UnityEngine.XR.MagicLeap
{
    /*! Result of a raycast. */
    [StructLayout(LayoutKind.Sequential)]
    internal struct MLRaycastResult
    {
        /*!
            \brief Where in the world the collision happened.
            This field is only valid if the state is either #MLRaycastResultState_HitUnobserved
            or #MLRaycastResultState_HitObserved.
        */
        internal Vector3 hitpoint;

        /*!
            \brief Normal to the surface where the ray collided.
            This field is only valid if the state is either
            #MLRaycastResultState_HitUnobserved or #MLRaycastResultState_HitObserved.
        */
        internal Vector3 normal;

        /*!
            \brief Confidence of the raycast result. Confidence is a non-negative value from 0 to 1 where closer
            to 1 indicates a higher quality. It is an indication of how confident we are about raycast result
            and underlying 3D shape. This field is only valid if the state is
            either #MLRaycastResultState_HitUnobserved or #MLRaycastResultState_HitObserved.
        */
        internal float confidence;

        /*!
            \brief The raycast result. If this field is either #MLRaycastResultState_RequestFailed or
            #MLRaycastResultState_NoCollision, fields in this structure are invalid.
        */
        internal MLRaycastResultState state;
    }
}
