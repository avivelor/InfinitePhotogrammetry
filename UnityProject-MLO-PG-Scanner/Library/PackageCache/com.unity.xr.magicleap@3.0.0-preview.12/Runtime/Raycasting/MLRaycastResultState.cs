namespace UnityEngine.XR.MagicLeap
{
    /*! The states of a raycast result. */
    internal enum MLRaycastResultState : int
    {
        /*! The raycast request failed. */
        RequestFailed = -1,

        /*! The ray passed beyond maximum raycast distance and it doesn't hit any surface. */
        NoCollision,

        /*! The ray hit unobserved area. This will on occur when collide_with_unobserved is set to true. */
        HitUnobserved,

        /*! The ray hit only observed area. */
        HitObserved,
    }

    internal static class MLRaycastResultStateExtensions
    {
        internal static RaycastResultState ToRaycastResultState(this MLRaycastResultState state)
        {
            switch (state)
            {
                case MLRaycastResultState.RequestFailed:
                    return RaycastResultState.ErrorUnknown;
                case MLRaycastResultState.NoCollision:
                    return RaycastResultState.ErrorNoCollision;
                case MLRaycastResultState.HitUnobserved:
                    return RaycastResultState.SuccessHitUnobserved;
                case MLRaycastResultState.HitObserved:
                    return RaycastResultState.SuccessHitObserved;
                default:
                    return RaycastResultState.ErrorUnknown;
            }
        }
    }
}
