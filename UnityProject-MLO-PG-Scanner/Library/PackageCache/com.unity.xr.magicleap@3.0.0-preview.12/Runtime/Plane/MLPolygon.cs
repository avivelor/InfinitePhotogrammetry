using System.Runtime.InteropServices;

namespace UnityEngine.XR.MagicLeap
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct MLPolygon
    {
        /*!
            \brief Vertices of all polygons in a plane.
        */
        internal Vector3* vertices;
        /*!
            \brief Count of vertices.
        */
        internal uint vertices_count;
    }
}
