namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Represents the state of a <see cref="RaycastResult"/>.
    /// </summary>
    public enum RaycastResultState
    {
        /// <summary>
        /// An unknown error occurred.
        /// </summary>
        ErrorUnknown,

        /// <summary>
        /// One of the fields of the <see cref="RaycastQuery"/> was invalid.
        /// </summary>
        ErrorInvalidParameter,

        /// <summary>
        /// The raycast is still being processed.
        /// </summary>
        Pending,

        /// <summary>
        /// The raycast did not hit anything.
        /// </summary>
        ErrorNoCollision,

        /// <summary>
        /// The raycast succeeded, and hit an unobserved portion of the world.
        /// </summary>
        SuccessHitUnobserved,

        /// <summary>
        /// The raycast succeeded, and hit an observed portion of the world.
        /// </summary>
        SuccessHitObserved,
    }

    /// <summary>
    /// Extensions for the <see cref="RaycastResultState"/> enum.
    /// </summary>
    public static class RaycastResultStateExtensions
    {
        /// <summary>
        /// Whether the <see cref="RaycastResultState"/> has completed or not.
        /// </summary>
        /// <param name="state">The <see cref="RaycastResultState"/> being extended.</param>
        /// <returns><c>true</c> if <paramref name="state"/> is not
        /// <see cref="RaycastResultState.Pending"/>.
        /// </returns>
        public static bool Done(this RaycastResultState state)
        {
            return state != RaycastResultState.Pending;
        }

        /// <summary>
        /// Whether the <see cref="RaycastResultState"/> represents a successful raycast hit.
        /// </summary>
        /// <param name="state">The <see cref="RaycastResultState"/> being extended.</param>
        /// <returns><c>true</c> if <paramref name="state"/> represents a successful raycast hit (i.e.,
        /// <see cref="RaycastResultState.SuccessHitObserved"/> or
        /// <see cref="RaycastResultState.SuccessHitUnobserved"/>) otherwise <c>false</c>.</returns>
        public static bool Success(this RaycastResultState state)
        {
            switch (state)
            {
                case RaycastResultState.SuccessHitUnobserved:
                case RaycastResultState.SuccessHitObserved:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether the <see cref="RaycastResultState"/> represents an error condition.
        /// </summary>
        /// <param name="state">The <see cref="RaycastResultState"/> being extended.</param>
        /// <returns><c>true</c> if the <see cref="Success"/> returns <c>false</c>.</returns>
        public static bool Error(this RaycastResultState state)
        {
            return Done(state) && !Success(state);
        }
    }
}
