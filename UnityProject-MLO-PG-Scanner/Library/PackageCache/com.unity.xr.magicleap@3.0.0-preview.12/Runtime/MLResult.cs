namespace UnityEngine.XR.MagicLeap
{
    internal enum MLApiResult : int
    {
        /*! Operation completed successfuly. */
        Ok = 0,
        /*! Asynchronous operation has not completed. */
        Pending,
        /*! Operation has timed out. */
        Timeout,
        /*! Request to lock a shared resource that is already locked. */
        Locked,
        /*! Operation failed due to an unspecified internal error. */
        UnspecifiedFailure,
        /*! Operation failed due to an invalid parameter being supplied. */
        InvalidParam,
        /*! Operation failed because memory failed to be allocated. */
        AllocFailed,
        /*! Operation failed because a required privilege has not been granted. */
        PrivilegeDenied,
        /*! Operation failed because it is not currently implemented. */
        NotImplemented,

        LowMapQuality = 0x41c7 << 16,
        UnableToLocalize,
        ServerUnavailable,

        PrivilegesGranted = 0xcbcd << 16,
        PrivilegesDenied,
    }

    internal static class MLApiResultExtensions
    {
        public static RaycastResultState ToRaycastResultState(this MLApiResult mlResult)
        {
            switch (mlResult)
            {
                case MLApiResult.InvalidParam:
                    return RaycastResultState.ErrorInvalidParameter;
                default:
                    return RaycastResultState.ErrorUnknown;
            }
        }
    }
}
