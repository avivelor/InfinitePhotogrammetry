using System.Runtime.InteropServices;

namespace UnityEngine.XR.MagicLeap
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MLPlanesQuery
    {
        /*! The flags to apply to this query. */
        internal MLPlanesQueryFlags flags;
        /*! The center of the bounding box which defines where planes extraction should occur. */
        internal Vector3 bounds_center;
        /*! The rotation of the bounding box where planes extraction will occur. */
        internal Quaternion bounds_rotation;
        /*! The size of the bounding box where planes extraction will occur. */
        internal Vector3 bounds_extents;
        /*! The maximum number of results that should be returned. This is also
            the minimum expected size of the array of results passed to the
            MLPlanesGetResult function. */
        internal uint max_results;
        /*!
        \brief If #MLPlanesQueryFlag_IgnoreHoles is set to false, holes with a perimeter
            (in meters) smaller than this value will be ignored, and can be part of the
            plane. This value cannot be lower than 0 (lower values will be capped to
            this minimum). A good default value is 0.5.
        \deprecated Deprecated since 0.15.0.
        */
        internal float min_hole_length;
        /*! The minimum area (in squared meters) of planes to be returned. This value
            cannot be lower than 0.04 (lower values will be capped to this minimum).
            A good default value is 0.25. */
        internal float min_plane_area;
    }
}
