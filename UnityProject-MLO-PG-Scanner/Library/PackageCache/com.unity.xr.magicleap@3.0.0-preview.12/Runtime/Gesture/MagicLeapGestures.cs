using System;
using System.Collections;
using Unity.Collections;
using UnityEngine.XR.InteractionSubsystems;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// <para>
    /// Controls the lifecycle and configuration options for a Magic Leap gesture subsystem. There
    /// is only one active Magic Leap Gestures.  The event callbacks will inform code of when gesture events occur.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MagicLeapGestures : SubsystemLifecycleManager<XRGestureSubsystem, XRGestureSubsystemDescriptor>
    {
        /// <summary>
        /// Get the <c>MagicLeapGestureSubsystem</c> whose lifetime this component manages.
        /// </summary>
        public MagicLeapGestureSubsystem gestureSubsystem { get; private set; }

        /// <summary>
        /// This event is invoked whenever a <see cref="MagicLeapKeyPoseGestureEvent"/> is received by the gestures subsystem.
        /// </summary>
        public event Action<MagicLeapKeyPoseGestureEvent> onKeyPoseGestureChanged;

        /// <summary>
        /// This event is invoked whenever a <see cref="MagicLeapTouchpadGestureEvent"/> is received by the gestures subsystem.
        /// </summary>
        public event Action<MagicLeapTouchpadGestureEvent> onTouchpadGestureChanged;

        /// <summary>
        /// This event is invoked whenever a <see cref="ActivateGestureEvent"/> is received by the gestures subsystem.
        /// </summary>
        public event Action<ActivateGestureEvent> onActivate;

        [SerializeField]
        bool m_ControllerGesturesEnabled;

        [SerializeField]
        bool m_HandGesturesEnabled;

        public bool controllerGesturesEnabled
        {
            get => m_ControllerGesturesEnabled;
            set
            {
                m_ControllerGesturesEnabled = value;
                if (gestureSubsystem != null)
                    gestureSubsystem.EnableControllerGestures(m_ControllerGesturesEnabled);
            }
        }

        public bool handGesturesEnabled
        {
            get => m_HandGesturesEnabled;
            set
            {
                m_HandGesturesEnabled = value;
                if (gestureSubsystem != null)
                    gestureSubsystem.EnableHandGestures(m_HandGesturesEnabled);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (subsystem != null && subsystem is MagicLeapGestureSubsystem)
                gestureSubsystem = subsystem as MagicLeapGestureSubsystem;

            if (gestureSubsystem != null)
            {
                    gestureSubsystem.EnableControllerGestures(m_ControllerGesturesEnabled);
                    gestureSubsystem.EnableHandGestures(m_HandGesturesEnabled);
            }
        }

        void Reset()
        {
            m_ControllerGesturesEnabled = true;
            m_HandGesturesEnabled = true;
        }

        void Update()
        {
            if (gestureSubsystem == null && !gestureSubsystem.running)
                return;

            gestureSubsystem.Update();

            if (onKeyPoseGestureChanged != null)
            {
                foreach (var keyPoseGestureEvent in gestureSubsystem.keyPoseGestureEvents)
                    onKeyPoseGestureChanged(keyPoseGestureEvent);
            }

            if (onTouchpadGestureChanged != null)
            {
                foreach (var touchpadGestureEvent in gestureSubsystem.touchpadGestureEvents)
                    onTouchpadGestureChanged(touchpadGestureEvent);
            }

            if (onActivate != null)
            {
                foreach (var activateGestureEvent in gestureSubsystem.activateGestureEvents)
                    onActivate(activateGestureEvent);
            }
        }
    }
}
