using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.MagicLeap
{
    /*! Request information for a raycast. */
    [StructLayout(LayoutKind.Sequential)]
    internal struct MLRaycastQuery : IEquatable<MLRaycastQuery>
    {
        /*! Origin of ray, in world space. */
        internal Vector3 position;

        /*!
            \brief Direction of ray, in world space.
            Use MLTransform.rotation * (0, 0, -1) to use the forward vector of the rig frame in world space.
        */
        internal Vector3 direction;

        /*!
            \brief Up vector, in world space.
            If multiple rays are to be fired, this is used to determine the coordinate system used to
            calculate the directions of those rays; therefore must be orthogonal to the direction vector.
            Use MLTransform.rotation * (0, 1, 0) to use the up vector of the rig frame in world space.
            This parameter has no effect on a single-point raycast.
        */
        internal Vector3 up_vector;

        /*! The number of horizontal rays. For single point raycast, set this to 1. */
        internal uint width;

        /*! The number of vertical rays. For single point raycast, set this to 1. */
        internal uint height;

        /*! The horizontal field of view, in degrees. */
        internal float horizontal_fov_degrees;

        /*!
            \brief If \c true, a ray will terminate when encountering an
            unobserved area and return a surface or the ray will continue until
            it ends or hits a observed surface.
        */
        internal bool collide_with_unobserved;

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = position.GetHashCode();
                hash = hash * 486187739 + direction.GetHashCode();
                hash = hash * 486187739 + up_vector.GetHashCode();
                hash = hash * 486187739 + width.GetHashCode();
                hash = hash * 486187739 + height.GetHashCode();
                hash = hash * 486187739 + horizontal_fov_degrees.GetHashCode();
                hash = hash * 486187739 + collide_with_unobserved.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return ((obj is MLRaycastQuery) && Equals((MLRaycastQuery)obj));
        }

        public bool Equals(MLRaycastQuery other)
        {
            return
                position.Equals(other.position) &&
                direction.Equals(other.direction) &&
                up_vector.Equals(other.up_vector) &&
                (width == other.width) &&
                (height == other.height) &&
                (horizontal_fov_degrees == other.horizontal_fov_degrees) &&
                (collide_with_unobserved == other.collide_with_unobserved);
        }

        public static bool operator==(MLRaycastQuery lhs, MLRaycastQuery rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(MLRaycastQuery lhs, MLRaycastQuery rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
