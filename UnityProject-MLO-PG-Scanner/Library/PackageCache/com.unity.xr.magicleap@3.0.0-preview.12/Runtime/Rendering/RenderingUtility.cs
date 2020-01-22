using System.Runtime.CompilerServices;

using UnityEngine;

namespace UnityEngine.XR.MagicLeap.Rendering
{
    public static class RenderingUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetParentScale(Transform transform)
        {
            var scale = Vector3.one;
            var parent = transform.parent;
            if (parent)
                scale = parent.lossyScale;
#if ML_RENDERING_VALIDATION
            if (!(Mathf.Approximately(scale.x, scale.y) && Mathf.Approximately(scale.x, scale.z)))
            {
                MLWarnings.WarnedAboutNonUniformScale.Trigger();
                return (scale.x + scale.y + scale.z) / 3;
            }
#endif
            // Avoid precision error caused by averaging x, y and z components.
            return scale.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetMainCameraScale()
        {
            return GetParentScale(Camera.main.transform);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToMagicLeapUnits(float val)
        {
            return ToMagicLeapUnits(val, RenderingSettings.s_CachedCameraScale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float ToMagicLeapUnits(float val, float scale)
        {
            return val / scale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToUnityUnits(float val)
        {
            return ToUnityUnits(val, RenderingSettings.s_CachedCameraScale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float ToUnityUnits(float val, float scale)
        {
            return val * scale;
        }
    }
}