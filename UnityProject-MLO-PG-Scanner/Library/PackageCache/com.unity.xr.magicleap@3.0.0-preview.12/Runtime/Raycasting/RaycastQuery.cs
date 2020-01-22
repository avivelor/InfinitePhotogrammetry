using System;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Represents a raycast query.
    /// </summary>
    /// <seealso cref="MagicLeapRaycastSubsystem.CreateRaycastJob(NativeArray{RaycastQuery}, NativeArray{RaycastResult})"/>
    /// <seealso cref="MagicLeapRaycastSubsystem.Raycast(RaycastQuery)"/>
    public struct RaycastQuery : IEquatable<RaycastQuery>
    {
        /// <summary>
        /// A <c>Ray</c>, in session space.
        /// </summary>
        public Ray ray { get; private set; }

        /// <summary>
        /// If multiple rays are to be fired (i.e., <see cref="width"/> or <see cref="height"/> are greater than 1),
        /// this is used to determine the coordinate system used to
        /// calculate the directions of those rays; therefore must be orthogonal to the direction vector.
        /// This parameter is has no effect on a single-point raycast.
        /// </summary>
        public Vector3 up { get; private set; }

        /// <summary>
        /// The number of horizontal rays to cast.
        /// </summary>
        public int width { get; private set; }

        /// <summary>
        /// The number of vertical rays to cast.
        /// </summary>
        public int height { get; private set; }

        /// <summary>
        /// The horizontal field of view, in degrees. When
        /// <paramref name="width"/> or <paramref name="height"/>
        /// are greater than 1, this is used to determine the frustum in which
        /// to cast multiple rays.
        /// </summary>
        public float horizontalFov { get; private set; }

        /// <summary>
        /// If <c>true</c>, a ray will temrinate when encountering an
        /// unobserved area and return a surface or the ray will continue until
        /// it ends or hits an observed surface.
        /// </summary>
        public bool collideWithUnobserved { get; private set; }

        /// <summary>
        /// Constructs a "single-point" raycast query, i.e.,
        /// <paramref name="width"/> and <paramref name="height"/> are both 1.
        /// </summary>
        /// <param name="ray">The ray, in session space, to cast.</param>
        /// <param name="collideWithUnobserved">If <c>true</c>, a ray will temrinate when encountering an unobserved area and return a surface or the ray will continue until it ends or hits an observed surface.</param>
        public RaycastQuery(Ray ray, bool collideWithUnobserved = false)
        : this(ray, Vector3.up, 1, 1, 0f, collideWithUnobserved)
        {}

        /// <summary>
        /// Constructs a raycast query.
        /// </summary>
        /// <remarks>
        /// If <paramref name="width"/> or <paramref name="height"/> are greater than 1,
        /// <c>width * height</c> rays are cast from the <paramref name="ray"/> origin within a frustum defined
        /// by <paramref name="horizontalFov"/> and <paramref name="up"/>.
        /// </remarks>
        /// <param name="ray">The ray, in session space, to cast.</param>
        /// <param name="up">
        /// If multiple rays are to be fired (i.e., <paramref name="width"/> or <paramref name="height"/> are greater than 1),
        /// this is used to determine the coordinate system used to
        /// calculate the directions of those rays; therefore must be orthogonal to the direction vector.
        /// This parameter is has no effect on a single-point raycast.
        /// </param>
        /// <param name="width">The number of horizontal rays to cast. Must be at least 1.</param>
        /// <param name="height">The number of vertical rays to cast. Must be at least 1.</param>
        /// <param name="horizontalFov">
        /// The horizontal field of view, in degrees. When
        /// <paramref name="width"/> or <paramref name="height"/>
        /// are greater than 1, this is used to determine the frustum in which
        /// to cast multiple rays.
        /// </param>
        /// <param name="collideWithUnobserved">If <c>true</c>, a ray will temrinate when encountering an unobserved area and return a surface or the ray will continue until it ends or hits an observed surface.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="width"/> is less than 1.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="height"/> is less than 1.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="width"/> or <paramref name="height"/>
        /// is greater than 1 but <paramref name="up"/> is not orthogonal to the <paramref name="ray"/>'s direction.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="width"/> or <paramref name="height"/> is
        /// greater than 1, but <paramref name="up"/> is zero.</exception>
        public RaycastQuery(Ray ray, Vector3 up, int width, int height, float horizontalFov, bool collideWithUnobserved)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be at least 1.");

            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be at least 1.");

            this.ray = ray;
            this.up = up.normalized;

            if ((width == 1) && (height == 1))
            {
                // Although up "has no effect" on single-point raycasts,
                // Magic Leap will abort the qury if this is zero, so ensure
                // it has some reasonable value.
                this.up = Vector3.up;
            }
            else if (this.up.sqrMagnitude < 0.0001f)
            {
                throw new ArgumentException("Up must be a non-zero direction vector.", nameof(up));
            }
            else if (Mathf.Abs(Vector3.Dot(this.up, ray.direction)) > 0.01f)
            {
                throw new ArgumentException("Up is not orthogonal to ray.direction", nameof(up));
            }

            this.width = width;
            this.height = height;
            this.horizontalFov = horizontalFov;
            this.collideWithUnobserved = collideWithUnobserved;
        }

        /// <summary>
        /// Computes a hash code suitable for use in a <c>Dictionary</c> or <c>HashSet</c>.
        /// </summary>
        /// <returns>A hash code suitable for use in a <c>Dictionary</c> or <c>HashSet</c>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = ray.GetHashCode();
                hash = hash * 486187739 + up.GetHashCode();
                hash = hash * 486187739 + width.GetHashCode();
                hash = hash * 486187739 + height.GetHashCode();
                hash = hash * 486187739 + horizontalFov.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// IEquatable interface. Compares for equality.
        /// </summary>
        /// <param name="obj">The object to compare for equality.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="RaycastQuery"/> and compares equal with <see cref="Equals(RaycastQuery)"/>.</returns>
        public override bool Equals(object obj)
        {
            return ((obj is RaycastQuery) && Equals((RaycastQuery)obj));
        }

        /// <summary>
        /// IEquatable interface. Comapres for equality.
        /// </summary>
        /// <param name="other">The <see cref="RaycastQuery"/> to compare against.</param>
        /// <returns><c>true</c> if all fields of this <see cref="RaycastQuery"/> compare equal to <paramref name="other"/>.</returns>
        public bool Equals(RaycastQuery other)
        {
            return
                ray.Equals(other.ray) &&
                up.Equals(other.up) &&
                (width == other.width) &&
                (height == other.height) &&
                (horizontalFov == other.horizontalFov);
        }

        /// <summary>
        /// Comapres for equality. Same as <see cref="Equals(RaycastQuery)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if all fields of this <see cref="RaycastQuery"/> compare equal to <paramref name="other"/>.</returns>
        public static bool operator==(RaycastQuery lhs, RaycastQuery rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Comapres for inequality. Same as <c>!</c><see cref="Equals(RaycastQuery)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if any of the fields of this <see cref="RaycastQuery"/> are not equal to <paramref name="other"/>.</returns>
        public static bool operator!=(RaycastQuery lhs, RaycastQuery rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
