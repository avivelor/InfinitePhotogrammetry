using System.Runtime.InteropServices;

namespace UnityEngine.XR.MagicLeap
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MLPlane
    {
        /*! Plane center. */
        internal Vector3 position;
        /*! Plane rotation. */
        internal Quaternion rotation;
        /*! Plane width. */
        internal float width;
        /*! Plane height. */
        internal float height;
        /*! Flags which describe this plane. */
        internal MLPlanesQueryFlags flags;
        /*! Plane ID. All inner planes within an outer plane will have the
            same ID (outer plane's ID). These IDs are persistent across
            plane queries unless a map merge occurs. On a map merge, IDs
            could be different. */
        internal ulong id;
    }
}
