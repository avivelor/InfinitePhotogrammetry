using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Represents a Magic Leap asynchronous raycast result. The result must be created using
    /// <see cref="MagicLeapRaycastSubsystem.AsyncRaycast(RaycastQuery)"/>. This API may be
    /// used from other threads.
    /// </summary>
    public struct AsyncRaycastResult : IEquatable<AsyncRaycastResult>
    {
        /// <summary>
        /// The pose of the raycast. The normal of the surface that was hit can be computed with <c>pose.up</c>.
        /// Only valid if <see cref="state"/> is <see cref="RaycastResultState.SuccessHitObserved"/> or <see cref="RaycastResultState.SuccessHitUnobserved"/>.
        /// </summary>
        public Pose pose { get; private set; }

        /// <summary>
        /// The confidence of the result in the range 0..1, where 0 indicates low confidence and 1 is high confidence.
        /// </summary>
        /// Only valid if <see cref="state"/> is <see cref="RaycastResultState.SuccessHitObserved"/> or <see cref="RaycastResultState.SuccessHitUnobserved"/>.
        public float confidence { get; private set; }

        /// <summary>
        /// The state of the raycasts, e.g. whether it was successful, pending, or incountered an error.
        /// If the state is anything other than
        /// <see cref="RaycastResultState.SuccessHitObserved"/> or
        /// <see cref="RaycastResultState.SuccessHitUnobserved"/>, then
        /// the other values in this struct are meaningless.
        /// </summary>
        public RaycastResultState state
        {
            get
            {
                // If m_State has a value, then MLRaycastGetResult has already completed
                // and the request either succeeded or failed. Don't invoke MLRaycastGetResult again.
                if (m_State.HasValue)
                    return m_State.Value;

                // MLResult_Ok Raycast Result was successfully retrieved.
                // MLResult_Pending Request has not completed. This does not indicate a failure.
                // MLResult_UnspecifiedFailure Failed due to internal error.
                // MLResult_InvalidParam Failed due to invalid parameter.
                var apiResult = MLRaycastGetResult(m_TrackerHandle, m_RequestHandle, out MLRaycastResult mlRaycastResult);

                if (apiResult == MLApiResult.Pending)
                {
                    return RaycastResultState.Pending;
                }
                else if (apiResult != MLApiResult.Ok)
                {
                    m_State = apiResult.ToRaycastResultState();
                    return m_State.Value;
                }

                // apiResult was Ok. Parse result.
                m_State = mlRaycastResult.state.ToRaycastResultState();
                if (m_State.Value.Success())
                {
                    var hitPoint = FlipHandedness(mlRaycastResult.hitpoint);
                    var normal = FlipHandedness(mlRaycastResult.normal);
                    var rotation = Quaternion.FromToRotation(Vector3.up, normal);

                    pose = new Pose(hitPoint, rotation);
                    confidence = mlRaycastResult.confidence;
                }

                return m_State.Value;
            }
        }
        RaycastResultState? m_State;

        /// <summary>
        /// Computes a hash code suitable for use in a <c>Dictionary</c> or <c>HashSet</c>.
        /// </summary>
        /// <returns>A hash code suitable for use in a <c>Dictionary</c> or <c>HashSet</c>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = pose.GetHashCode();
                hash = hash * 486187739 + confidence.GetHashCode();
                hash = hash * 486187739 + (m_State.HasValue ? ((int)m_State.Value).GetHashCode() : 0);
                hash = hash * 486187739 + m_TrackerHandle.GetHashCode();
                hash = hash * 486187739 + m_RequestHandle.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// IEquatable interface. Compares for equality.
        /// </summary>
        /// <param name="obj">The object to compare for equality.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="AsyncRaycastResult"/> and compares equal with <see cref="Equals(AsyncRaycastResult)"/>.</returns>
        public override bool Equals(object obj)
        {
            return ((obj is AsyncRaycastResult) && Equals((AsyncRaycastResult)obj));
        }

        /// <summary>
        /// IEquatable interface. Comapres for equality.
        /// </summary>
        /// <param name="other">The <see cref="AsyncRaycastResult"/> to compare against.</param>
        /// <returns><c>true</c> if all fields of this <see cref="AsyncRaycastResult"/> compare equal to <paramref name="other"/>.</returns>
        public bool Equals(AsyncRaycastResult other)
        {
            return
                pose.Equals(other.pose) &&
                (confidence == other.confidence) &&
                m_State.Equals(other.m_State) &&
                m_TrackerHandle.Equals(other.m_TrackerHandle) &&
                m_RequestHandle.Equals(other.m_RequestHandle);
        }

        /// <summary>
        /// Comapres for equality. Same as <see cref="Equals(AsyncRaycastResult)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if all fields of this <see cref="AsyncRaycastResult"/> compare equal to <paramref name="other"/>.</returns>
        public static bool operator==(AsyncRaycastResult lhs, AsyncRaycastResult rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Comapres for inequality. Same as <c>!</c><see cref="Equals(AsyncRaycastResult)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if any of the fields of this <see cref="AsyncRaycastResult"/> are not equal to <paramref name="other"/>.</returns>
        public static bool operator!=(AsyncRaycastResult lhs, AsyncRaycastResult rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Generates a string representation of the raycast result of the form
        /// "{state}: pose {pose} with confidence {confidence}"
        /// </summary>
        /// <returns>A string representation of the raycast result of the form
        /// "{state}: pose {pose} with confidence {confidence}"</returns>
        public override string ToString()
        {
            return $"{state}: pose {pose} with confidence {confidence}";
        }

        internal AsyncRaycastResult(ulong trackerHandle, RaycastQuery query)
        {
            pose = Pose.identity;
            confidence = 0f;

            // Translate the query to an MLRaycastQuery
            var mlQuery = new MLRaycastQuery
            {
                position = FlipHandedness(query.ray.origin),
                direction = FlipHandedness(query.ray.direction),
                up_vector = query.up.normalized,
                width = (uint)query.width,
                height = (uint)query.height,
                horizontal_fov_degrees = query.horizontalFov,
                collide_with_unobserved = query.collideWithUnobserved
            };

            var apiResult = MLRaycastRequest(trackerHandle, ref mlQuery, out m_RequestHandle);
            if (apiResult == MLApiResult.Ok)
            {
                m_State = null;
                m_TrackerHandle = trackerHandle;
            }
            else
            {
                m_State = apiResult.ToRaycastResultState();
                m_TrackerHandle = k_InvalidHandle;
            }
        }

        const ulong k_InvalidHandle = ulong.MaxValue;

        /// <summary>
        /// Set by the <see cref="MagicLeapRaycastSubsystem"/> when the result is created.
        /// </summary>
        ulong m_TrackerHandle;

        /// <summary>
        /// Set by the <see cref="MagicLeapRaycastSubsystem"/> when the result is created.
        /// </summary>
        ulong m_RequestHandle;

        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        static extern MLApiResult MLRaycastRequest(ulong tracker_handle, ref MLRaycastQuery request, out ulong out_handle);

        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        static extern MLApiResult MLRaycastGetResult(ulong tracker_handle, ulong raycast_request, out MLRaycastResult out_result);

        static Vector3 FlipHandedness(Vector3 v)
        {
            return new Vector3(v.x, v.y, -v.z);
        }
    }
}
