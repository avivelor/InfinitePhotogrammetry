using System.Runtime.InteropServices;

namespace UnityEngine.XR.MagicLeap
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct MLPlaneBoundaries
    {
        internal bool valid
        {
            get { return boundaries != null; }
        }

        /*!
            \brief Plane ID, the same value associating to the ID in #MLPlane if they
            belong to the same plane.
        */
        internal ulong id;
        /*!
            \brief The boundaries in a plane.
        */
        internal MLPlaneBoundary* boundaries;
        /*!
            \brief Count of boundaries. A plane may contain multiple boundaries
            each of which defines a region.
        */
        internal uint boundaries_count;
    }
}
