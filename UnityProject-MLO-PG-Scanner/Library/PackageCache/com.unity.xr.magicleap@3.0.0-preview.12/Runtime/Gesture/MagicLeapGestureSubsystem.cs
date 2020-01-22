using System;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;
using UnityEngine.XR.InteractionSubsystems;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// MagicLeap implementation of the <c>XRGestureSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
    /// </summary>
    [Preserve]
    public sealed class MagicLeapGestureSubsystem : XRGestureSubsystem
    {
        /// <summary>
        /// A collection of all MagicLeapKeyPoseGestureEvents managed by this subsystem.
        /// This is cleared every frame and refreshed with new gesture events.
        /// </summary>
        public NativeArray<MagicLeapKeyPoseGestureEvent> keyPoseGestureEvents { get { return m_MagicLeapProvider.keyPoseGestureEvents; } }

        /// <summary>
        /// A collection of all MagicLeapTouchpadGestureEvents managed by this subsystem.
        /// This is cleared every frame and refreshed with new gesture events.
        /// </summary>
        public NativeArray<MagicLeapTouchpadGestureEvent> touchpadGestureEvents { get { return m_MagicLeapProvider.touchpadGestureEvents; } }

        /// <summary>
        /// Creates the provider interface.
        /// </summary>
        /// <returns>The provider interface for MagicLeap</returns>
        protected override Provider CreateProvider()
        {
            m_MagicLeapProvider = new MagicLeapGestureProvider(this);
            return m_MagicLeapProvider;
        }

        internal void EnableControllerGestures(bool value)
        {
#if PLATFORM_LUMIN
            NativeApi.SetControllerGesturesEnabled(value);
#endif
        }

        internal void EnableHandGestures(bool value)
        {
#if PLATFORM_LUMIN
            NativeApi.SetHandGesturesEnabled(value);
#endif
        }

        class MagicLeapGestureProvider : Provider
        {
            MagicLeapGestureSubsystem m_Subsystem;

            public MagicLeapGestureProvider(MagicLeapGestureSubsystem subsystem)
            {
#if PLATFORM_LUMIN
                NativeApi.Create();
                m_Subsystem = subsystem;
#endif
            }

            public override void Start()
            {
#if PLATFORM_LUMIN
                NativeApi.Start();
#endif
            }

            public override void Stop()
            {
#if PLATFORM_LUMIN
                NativeApi.Stop();
#endif
            }

            public override void Update()
            {
#if PLATFORM_LUMIN
                NativeApi.Update();

                RetrieveGestureEvents();
#endif
            }

            public unsafe delegate void* GetGesturesDelegate(out int gestureEventsLength, out int elementSize);

            unsafe void GetGestureEvents<T>(ref NativeArray<T> gestureEventsArray, GetGesturesDelegate getGesturesAction) where T : struct
            {
                // gestureEventsPtr is not owned by this code, contents must be copied out in this function.
                int gestureEventsLength, elementSize;
                void* gestureEventsPtr = getGesturesAction(out gestureEventsLength, out elementSize);

                if (!(gestureEventsLength == 0 && gestureEventsArray.Length == 0))
                {
                    if (gestureEventsArray.IsCreated)
                        gestureEventsArray.Dispose();
                    gestureEventsArray = new NativeArray<T>(gestureEventsLength, Allocator.Persistent);

                    var sizeOfGestureEvent = UnsafeUtility.SizeOf<T>();
                    UnsafeUtility.MemCpy(gestureEventsArray.GetUnsafePtr(), gestureEventsPtr, elementSize * gestureEventsLength);
                }
            }

            unsafe void RetrieveGestureEvents()
            {
                if (NativeApi.IsHandGesturesEnabled())
                    GetGestureEvents<MagicLeapKeyPoseGestureEvent>(ref m_KeyPoseGestureEvents, NativeApi.GetKeyPoseGestureEventsPtr);
                if (NativeApi.IsControllerGesturesEnabled())
                    GetGestureEvents<MagicLeapTouchpadGestureEvent>(ref m_TouchpadGestureEvents, NativeApi.GetTouchpadGestureEventsPtr);

                // Count up valid activate gestures (Have to do this as we cannot dynamically grow NativeArray). 
                // This should be possible to fix with NativeList (when out of preview package).
                int activateGestureEventCount = 0;
                foreach (var gestureEvent in m_KeyPoseGestureEvents)
                {
                    if (gestureEvent.state == GestureState.Started && gestureEvent.keyPose == MagicLeapKeyPose.Finger)
                        activateGestureEventCount++;
                }
                
                if (m_ActivateGestureEvents.IsCreated)
                    m_ActivateGestureEvents.Dispose();
                m_ActivateGestureEvents = new NativeArray<ActivateGestureEvent>(activateGestureEventCount, Allocator.Persistent);

                int iActivateGestureEvent = 0;
                foreach (var gestureEvent in m_KeyPoseGestureEvents)
                {
                    if (gestureEvent.state == GestureState.Started && gestureEvent.keyPose == MagicLeapKeyPose.Finger)
                        m_ActivateGestureEvents[iActivateGestureEvent++] = 
                            new ActivateGestureEvent(GetNextGUID(), gestureEvent.state, Vector3.zero, Vector3.one);
                }
            }

            public override void Destroy()
            {
#if PLATFORM_LUMIN
                NativeApi.Destroy();

                m_KeyPoseGestureEvents.Dispose();
                m_TouchpadGestureEvents.Dispose();
#endif
                base.Destroy();
            }

            public NativeArray<MagicLeapKeyPoseGestureEvent> keyPoseGestureEvents { get { return m_KeyPoseGestureEvents; } }
            NativeArray<MagicLeapKeyPoseGestureEvent> m_KeyPoseGestureEvents = new NativeArray<MagicLeapKeyPoseGestureEvent>(0, Allocator.Persistent);

            public NativeArray<MagicLeapTouchpadGestureEvent> touchpadGestureEvents { get { return m_TouchpadGestureEvents; } }
            NativeArray<MagicLeapTouchpadGestureEvent> m_TouchpadGestureEvents = new NativeArray<MagicLeapTouchpadGestureEvent>(0, Allocator.Persistent);
        }

#if UNITY_EDITOR || PLATFORM_LUMIN
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static void RegisterDescriptor()
        {
            XRGestureSubsystemDescriptor.RegisterDescriptor(
                new XRGestureSubsystemDescriptor.Cinfo
                {
                    id = "MagicLeap-Gesture",
                    subsystemImplementationType = typeof(MagicLeapGestureSubsystem)
                }
            );
        }

        static class NativeApi
        {
            const string Library = "UnityMagicLeap";

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesCreate")]
            public static extern void Create();

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesUpdate")]
            public static extern void Update();

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesStart")]
            public static extern void Start();

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesGetKeyPoseGestureEventsPtr")]
            public static extern unsafe void* GetKeyPoseGestureEventsPtr(out int gesturesLength, out int elementSize);

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesGetTouchpadGestureEventsPtr")]
            public static extern unsafe void* GetTouchpadGestureEventsPtr(out int gesturesLength, out int elementSize);

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesDestroy")]
            public static extern void Destroy();

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesStop")]
            public static extern void Stop();

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesIsControllerGesturesEnabled")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool IsControllerGesturesEnabled();

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesIsHandGesturesEnabled")]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool IsHandGesturesEnabled();

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesSetControllerGesturesEnabled")]
            public static extern void SetControllerGesturesEnabled([MarshalAs(UnmanagedType.I1)] bool value);

            [DllImport(Library, EntryPoint="UnityMagicLeap_GesturesSetHandGesturesEnabled")]
            public static extern void SetHandGesturesEnabled([MarshalAs(UnmanagedType.I1)] bool value);
        }

        // High GUID bits saved for common (Activate) gesture for this subsystem
        static GestureId s_NextGUID = new GestureId(1, 0);
        static GestureId GetNextGUID()
        {
            unchecked
            {
                s_NextGUID.subId1 += 1;
                if (s_NextGUID.subId1 != 0) return s_NextGUID;
                s_NextGUID.subId1 += 1;                
            }

            return s_NextGUID;
        }

        MagicLeapGestureProvider m_MagicLeapProvider;
    }
}