using System.Runtime.InteropServices;

namespace UnityEngine.XR.MagicLeap
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct MLPlaneBoundariesList
    {
        internal static MLPlaneBoundariesList Create()
        {
            // Same as MLPlaneBoundariesListInit in ml_planes.h
            return new MLPlaneBoundariesList
            {
                version = 1
            };
        }

        internal bool valid
        {
            get
            {
                return
                    (version > 0) &&
                    (plane_boundaries != null);
            }
        }

        internal uint version;
        /*!
            \brief List of #MLPlaneBoundaries.
            \apilevel 2
        */
        internal MLPlaneBoundaries* plane_boundaries;
        /*!
            \brief Count of #MLPlaneBoundaries in the array.
            \apilevel 2
        */
        internal uint plane_boundaries_count;
    }
}
