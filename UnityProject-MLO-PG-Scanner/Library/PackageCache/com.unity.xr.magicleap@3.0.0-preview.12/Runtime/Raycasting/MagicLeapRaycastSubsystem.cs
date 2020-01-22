using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Lumin;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.MagicLeap.Internal;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// The Magic Leap implementation of the <c>XRRaycastSubsystem</c>. Do not create this directly.
    /// Use <c>XRRaycastSubsystemDescriptor.Create()</c> instead.
    /// </summary>
    [Preserve]
    [UsesLuminPrivilege("WorldReconstruction")]
    public sealed class MagicLeapRaycastSubsystem : XRRaycastSubsystem
    {
        Provider m_Provider;

        /// <summary>
        /// Asynchronously casts a ray. Use the returned <see cref="AsyncRaycastResult"/> to check for completion and
        /// retrieve the raycast hit results.
        /// </summary>
        /// <param name="query">The input query for the raycast job.</param>
        /// <returns>An <see cref="AsyncRaycastResult"/> which can be used to check for completion and retrieve the raycast result.</returns>
        public AsyncRaycastResult AsyncRaycast(RaycastQuery query)
        {
            return m_Provider.AsyncRaycast(query);
        }

        protected override IProvider CreateProvider()
        {
            m_Provider = new Provider();
            return m_Provider;
        }

        class Provider : IProvider
        {
            ulong m_TrackerHandle = Native.InvalidHandle;

            PerceptionHandle m_PerceptionHandle;

            static Vector3 FlipHandedness(Vector3 v)
            {
                return new Vector3(v.x, v.y, -v.z);
            }

            public AsyncRaycastResult AsyncRaycast(RaycastQuery query)
            {
                return new AsyncRaycastResult(m_TrackerHandle, query);
            }

            public Provider()
            {
                m_PerceptionHandle = PerceptionHandle.Acquire();
            }

            public override void Start()
            {
                var result = Native.Create(out m_TrackerHandle);
                if (result != MLApiResult.Ok)
                {
                    m_TrackerHandle = Native.InvalidHandle;
                }
            }

            public override void Stop()
            {
                if (m_TrackerHandle != Native.InvalidHandle)
                {
                    Native.Destroy(m_TrackerHandle);
                    m_TrackerHandle = Native.InvalidHandle;
                }
            }

            public override void Destroy()
            {
                m_PerceptionHandle.Dispose();
            }
        }

        static class Native
        {
            public const ulong InvalidHandle = ulong.MaxValue;

            const string Library = "ml_perception_client";

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLRaycastCreate")]
            public static extern MLApiResult Create(out ulong handle);

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLRaycastDestroy")]
            public static extern MLApiResult Destroy(ulong handle);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterDescriptor()
        {
#if PLATFORM_LUMIN
            XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
            {
                id = "MagicLeap-Raycast",
                subsystemImplementationType = typeof(MagicLeapRaycastSubsystem),
                supportsViewportBasedRaycast = false,
                supportsWorldBasedRaycast = false,
                supportedTrackableTypes = TrackableType.None,
            });
#endif
        }
    }
}
