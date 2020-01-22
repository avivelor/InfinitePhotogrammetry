using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.MagicLeap
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct MLCoordinateFrameUID : IEquatable<MLCoordinateFrameUID>
    {
        internal fixed ulong data[2];

        public override string ToString()
        {
            return $"{data[0].ToString("X16")}-{data[1].ToString("X16")}";
        }

        public override bool Equals(object obj)
        {
            return ((obj is MLCoordinateFrameUID) && Equals((MLCoordinateFrameUID)obj));
        }

        public bool Equals(MLCoordinateFrameUID other)
        {
            return
                (data[0] == other.data[0]) &&
                (data[1] == other.data[1]);
        }

        public static bool operator ==(MLCoordinateFrameUID lhs, MLCoordinateFrameUID rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(MLCoordinateFrameUID lhs, MLCoordinateFrameUID rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return data[0].GetHashCode() * 486187739 + data[1].GetHashCode();
            }
        }
    }
}
