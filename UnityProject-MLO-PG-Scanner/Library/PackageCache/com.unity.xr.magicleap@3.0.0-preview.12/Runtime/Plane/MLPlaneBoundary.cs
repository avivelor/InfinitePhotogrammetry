using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.MagicLeap
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct MLPlaneBoundary : IEquatable<MLPlaneBoundary>
    {
        internal bool valid
        {
            get { return polygon != null; }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (new IntPtr(polygon)).GetHashCode();
                hash = hash * 486187739 + (new IntPtr(holes)).GetHashCode();
                hash = hash * 486187739 + holes_count.GetHashCode();
                return hash;
            }
        }

        public bool Equals(MLPlaneBoundary other)
        {
            return
                (polygon == other.polygon) &&
                (holes == other.holes) &&
                (holes_count == other.holes_count);
        }

        /*!
            \brief The polygon that defines the region.
        */
        internal MLPolygon* polygon;
        /*!
            \brief A polygon may contains multiple holes.
        */
        internal MLPolygon* holes;
        /*!
            \brief Count of the holes.
        */
        internal uint holes_count;
    }
}
