using System;
using System.Runtime.InteropServices;
using UnityEngine.XR.InteractionSubsystems;

namespace UnityEngine.XR.MagicLeap
{
    public enum MagicLeapHand
    {
        Left,
        Right
    }

    public enum MagicLeapKeyPose
    {
        Finger,
        Fist,
        Pinch,
        Thumb,
        LShape,
        OpenHand,
        Ok,
        CShape,
        NoPose,
        NoHand
    }

    /// <summary>
    /// The event data related to a Magic Leap KeyPose gesture
    /// </summary>
    /// <seealso cref="XRGestureSubsystem"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct MagicLeapKeyPoseGestureEvent : IEquatable<MagicLeapKeyPoseGestureEvent>
    {
        /// <summary>
        /// The <see cref="GestureId"/> associated with this gesture.
        /// </summary>
        public GestureId id { get { return m_Id; } }

        /// <summary>
        /// The <see cref="state"/> of the gesture.
        /// </summary>
        public GestureState state { get { return m_State; } }

        /// <summary>
        /// The <see cref="MagicLeapKeyPose"/> of the gesture.
        /// </summary>
        public MagicLeapKeyPose keyPose { get { return m_KeyPose; } }

        /// <summary>
        /// The <see cref="MagicLeapHand"/> of the gesture.
        /// </summary>
        public MagicLeapHand hand { get { return m_Hand; } }

        /// <summary>
        /// Gets a default-initialized <see cref="MagicLeapKeyPoseGestureEvent"/>. 
        /// </summary>
        /// <returns>A default <see cref="MagicLeapKeyPoseGestureEvent"/>.</returns>
        public static MagicLeapKeyPoseGestureEvent GetDefault()
        {
            return new MagicLeapKeyPoseGestureEvent(GestureId.invalidId, GestureState.Invalid, MagicLeapKeyPose.NoHand, MagicLeapHand.Left);
        }

        /// <summary>
        /// Constructs a new <see cref="MagicLeapKeyPoseGestureEvent"/>.
        /// </summary>
        /// <param name="id">The <see cref="GestureId"/> associated with the gesture.</param>
        /// <param name="state">The <see cref="GestureId"/> associated with the gesture.</param>
        /// <param name="keyPose">The <see cref="MagicLeapKeyPose"/> associated with the gesture.</param>
        /// <param name="hand">The <see cref="MagicLeapHand"/> associated with the gesture.</param>
        public MagicLeapKeyPoseGestureEvent(GestureId id, GestureState state, MagicLeapKeyPose keyPose, MagicLeapHand hand)
        {
            m_Id = id;
            m_State = state;
            m_KeyPose = keyPose;
            m_Hand = hand;
        }

        /// <summary>
        /// Generates a new string describing the gestures's properties suitable for debugging purposes.
        /// </summary>
        /// <returns>A string describing the gestures's properties.</returns>
        public override string ToString()
        {
            return string.Format(
                "KeyPose Gesture:\n\tgestureId: {0}\n\tgestureState: {1}\n\tkeyPose: {2}\n\thand: {3}",
                id, state, keyPose, hand);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MagicLeapKeyPoseGestureEvent && Equals((MagicLeapKeyPoseGestureEvent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Id.GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)m_State).GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)m_KeyPose).GetHashCode();
                hashCode = (hashCode * 486187739) + ((int)m_Hand).GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MagicLeapKeyPoseGestureEvent lhs, MagicLeapKeyPoseGestureEvent rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(MagicLeapKeyPoseGestureEvent lhs, MagicLeapKeyPoseGestureEvent rhs)
        {
            return !lhs.Equals(rhs);
        }

        public bool Equals(MagicLeapKeyPoseGestureEvent other)
        {
            return
                m_Id.Equals(other.id) &&
                m_State == other.state &&
                m_KeyPose == other.keyPose &&
                m_Hand == other.hand;
        }

        GestureId m_Id;
        GestureState m_State;
        MagicLeapKeyPose m_KeyPose;
        MagicLeapHand m_Hand;
    }
}