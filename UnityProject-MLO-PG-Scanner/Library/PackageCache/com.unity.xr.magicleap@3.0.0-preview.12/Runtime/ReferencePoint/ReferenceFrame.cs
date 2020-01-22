using System;
using System.Runtime.InteropServices;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Contains information necessary to report on <c>XRReferencePoint</c>s.
    /// </summary>
    internal struct ReferenceFrame
    {
        /// <summary>
        /// Information necessary to construct a reference frame
        /// </summary>
        public struct Cinfo
        {
            /// <summary>
            /// The closet coordinate frame to the requested point
            /// </summary>
            public Pose closetCoordinateFrame;

            /// <summary>
            /// The closest coordinate frame's UID. Necessary so we can
            /// update the reference point in the future.
            /// </summary>
            public MLCoordinateFrameUID cfuid;

            /// <summary>
            /// The tracking state of the reference point. Necessary so we can
            /// report an update if the tracking state changes.
            /// </summary>
            public TrackingState trackingState;

            /// <summary>
            /// The initial pose of the reference point. Necessary so we can compute
            /// the transform between <see cref="closetCoordinateFrame"/> and the
            /// reference point.
            /// </summary>
            public Pose initialReferencePointPose;
        }

        public ReferenceFrame(Cinfo cinfo)
        {
            trackableId = GenerateTrackableId();
            coordinateFrame = cinfo.closetCoordinateFrame;
            cfuid = cinfo.cfuid;
            trackingState = cinfo.trackingState;

            // Compute the delta transform between the closet coordinate frame and the reference point
            m_ReferencePointFromCoordinateFrame = Pose.identity;
            ComputeDelta(cinfo.initialReferencePointPose);
        }

        /// <summary>
        /// A pose which describes the delta beteen the reference point and the closest MLCoordinateFrame.
        /// </summary>
        Pose m_ReferencePointFromCoordinateFrame;

        /// <summary>
        /// The reference point's trackable id.
        /// </summary>
        public TrackableId trackableId { get; private set; }

        /// <summary>
        /// The pose of the coordinate frame used as the origin when calculating the <see cref="referencePointPose"/>.
        /// </summary>
        public Pose coordinateFrame { get; set; }

        /// <summary>
        /// The UID of the closest coordinate frame
        /// </summary>
        public MLCoordinateFrameUID cfuid { get; private set; }

        /// <summary>
        /// The tracking state associated with the reference point
        /// </summary>
        /// <value></value>
        public TrackingState trackingState { get; set; }

        /// <summary>
        /// Compute the pose of the reference point.
        /// </summary>
        public Pose referencePointPose
        {
            get
            {
                return m_ReferencePointFromCoordinateFrame.GetTransformedBy(coordinateFrame);
            }
        }

        /// <summary>
        /// Get the reference frame as a refernce point
        /// </summary>
        public XRReferencePoint referencePoint
        {
            get
            {
                return new XRReferencePoint(
                    trackableId,
                    referencePointPose,
                    trackingState,
                    IntPtr.Zero);
            }
        }

        /// <summary>
        /// Sets a new coordinate frame. This is different from simply setting
        /// the <see cref="coordinateFrame"/>. This method causes the reference point
        /// to be computed relative to a different coordinate frame entirely.
        /// </summary>
        /// <param name="cfuid">The UID of the new coordinate frame</param>
        /// <param name="coordinateFrame">The pose of the new coordinate frame</param>
        public void SetCoordinateFrame(MLCoordinateFrameUID cfuid, Pose coordinateFrame)
        {
            // Compute the current reference point pose
            var pose = referencePointPose;

            // Set new coordinate frame
            this.cfuid = cfuid;
            this.coordinateFrame = coordinateFrame;

            // Recompute the delta transform based on the new coordinate frame
            ComputeDelta(pose);
        }

        /// <summary>
        /// Sets the tracking state and returns true if the state changed.
        /// </summary>
        /// <param name="trackingState">The new tracking state</param>
        /// <returns>true if the tracking state changed.</returns>
        public bool SetTrackingState(TrackingState trackingState)
        {
            var oldTrackingState = this.trackingState;
            this.trackingState = trackingState;
            return oldTrackingState != trackingState;
        }

        void ComputeDelta(Pose pose)
        {
            var invRotation = Quaternion.Inverse(coordinateFrame.rotation);
            m_ReferencePointFromCoordinateFrame = new Pose(
                invRotation * (pose.position - coordinateFrame.position),
                invRotation * pose.rotation);
        }

        static unsafe TrackableId GenerateTrackableId()
        {
            var guid = Guid.NewGuid();
            void* guidPtr = &guid;
            return Marshal.PtrToStructure<TrackableId>((IntPtr)guidPtr);
        }
    }
}
