using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Lumin;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.MagicLeap.Internal;

namespace UnityEngine.XR.MagicLeap
{
    using MLLog = UnityEngine.XR.MagicLeap.MagicLeapLogger;

    /// <summary>
    /// The Magic Leap implementation of the <c>XRReferencePointSubsystem</c>. Do not create this directly.
    /// Use <c>XRReferencePointSubsystemDescriptor.Create()</c> instead.
    /// </summary>
    [Preserve]
    [UsesLuminPrivilege("PwFoundObjRead")]
    public sealed class MagicLeapReferencePointSubsystem : XRReferencePointSubsystem
    {
        const string kLogTag = "Unity-ReferencePoints";

        static void DebugLog(string msg)
        {
            MLLog.Debug(kLogTag, msg);
        }

        static void LogWarning(string msg)
        {
            MLLog.Warning(kLogTag, msg);
        }

        static void LogError(string msg)
        {
            MLLog.Error(kLogTag, msg);
        }

        [Conditional("DEVELOPMENT_BUILD")]
        static void DebugError(string msg)
        {
            LogError(msg);
        }

        static class Native
        {
            public const ulong InvalidHandle = ulong.MaxValue;

            const string Library = "ml_perception_client";

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPersistentCoordinateFrameTrackerCreate")]
            public static extern MLApiResult Create(out ulong out_tracker_handle);

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPersistentCoordinateFrameGetClosest")]
            public static unsafe extern MLApiResult GetClosest(ulong tracker_handle, ref Vector3 target, out MLCoordinateFrameUID out_cfuid);

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPersistentCoordinateFrameTrackerDestroy")]
            public static extern MLApiResult Destroy(ulong tracker_handle);

            [DllImport("UnityMagicLeap", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UnityMagicLeap_TryGetPose")]
            public static extern bool TryGetPose(MLCoordinateFrameUID id, out Pose out_pose);
        }

        static Vector3 FlipHandedness(Vector3 position)
        {
            return new Vector3(position.x, position.y, -position.z);
        }

        static Quaternion FlipHandedness(Quaternion rotation)
        {
            return new Quaternion(rotation.x, rotation.y, -rotation.z, -rotation.w);
        }

        protected override IProvider CreateProvider()
        {
            return new Provider();
        }

        class Provider : IProvider
        {
            ulong m_TrackerHandle = Native.InvalidHandle;

            PerceptionHandle m_PerceptionHandle;

            /// <summary>
            /// The privilege required to access persistent coordinate frames
            /// </summary>
            const uint k_MLPivilegeID_PwFoundObjRead = 201;

            /// <summary>
            /// The squared amount by which a coordinate frame has to move for its reference points to be reported as "updated"
            /// </summary>
            const float k_CoordinateFramePositionEpsilonSquared = .0001f;

            bool RequestPrivilegesIfNecessary()
            {
                if (MagicLeapPrivileges.IsPrivilegeApproved(k_MLPivilegeID_PwFoundObjRead))
                {
                    return true;
                }
                else
                {
                    return MagicLeapPrivileges.RequestPrivilege(k_MLPivilegeID_PwFoundObjRead);
                }
            }

            public Provider()
            {
                m_PerceptionHandle = PerceptionHandle.Acquire();
                RequestPrivilegesIfNecessary();
            }

            public override void Start()
            {
                if (!RequestPrivilegesIfNecessary())
                {
                    LogWarning($"Could not start the reference point subsystem because privileges were denied.");
                    return;
                }

                var result = Native.Create(out m_TrackerHandle);
                if (result != MLApiResult.Ok)
                {
                    m_TrackerHandle = Native.InvalidHandle;
                    LogWarning($"Could not create a Magic Leap Persistent Coordinate Frame Tracker because '{result}'. Reference points are unavailable.");
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

            bool PosesAreApproximatelyEqual(Pose lhs, Pose rhs)
            {
                // todo 2019-05-21: consider rotation?
                return (lhs.position - rhs.position).sqrMagnitude <= k_CoordinateFramePositionEpsilonSquared;
            }

            /// <summary>
            /// Checks to see if there is a better PCF for a reference point and updates the
            /// <paramref name="referenceFrame"/> if necessary.
            /// </summary>
            /// <returns>true if the reference frame was updated</returns>
            bool UpdateReferenceFrame(ref ReferenceFrame referenceFrame)
            {
                // See if there is a better coordinate frame we could be using
                var mlTarget = FlipHandedness(referenceFrame.referencePointPose.position);
                if (Native.GetClosest(m_TrackerHandle, ref mlTarget, out MLCoordinateFrameUID cfuid) != MLApiResult.Ok)
                {
                    // No coordinate frame could be found, so set tracking state to None
                    // and return whether the tracking state changed.
#if DEVELOPMENT_BUILD
                    DebugError("GetClosest failed.");
#endif
                    return referenceFrame.SetTrackingState(TrackingState.None);
                }
                else if (!Native.TryGetPose(cfuid, out Pose coordinateFrame))
                {
                    // Couldn't get a pose for the coordinate frame, so set tracking state to None
#if DEVELOPMENT_BUILD
                    DebugError($"TryGetPose for cfuid {cfuid} failed.");
#endif
                    return referenceFrame.SetTrackingState(TrackingState.None);
                }
                else if (!cfuid.Equals(referenceFrame.cfuid))
                {
                    // A different coordinate frame has been chosen
#if DEVELOPMENT_BUILD
                    DebugLog($"cfuid's changed. Old: {referenceFrame.coordinateFrame} New: {coordinateFrame}.");
#endif
                    referenceFrame.trackingState = TrackingState.Tracking;
                    referenceFrame.SetCoordinateFrame(cfuid, coordinateFrame);
                    return true;
                }
                // If the CFUIDs are the same, then check to see if the coordinate frame has changed
                else if (!PosesAreApproximatelyEqual(coordinateFrame, referenceFrame.coordinateFrame))
                {
                    // Coordinate frame has changed
#if DEVELOPMENT_BUILD
                    DebugLog($"Coordinate frame updated. Old: {referenceFrame.coordinateFrame} New: {coordinateFrame}.");
#endif
                    referenceFrame.trackingState = TrackingState.Tracking;
                    referenceFrame.coordinateFrame = coordinateFrame;
                    return true;
                }
                else
                {
                    // Common case: pose was retrieved, but nothing has changed.
                    return referenceFrame.SetTrackingState(TrackingState.Tracking);
                }
            }

            /// <summary>
            /// Populates <paramref name="added"/> with all the reference points
            /// which have been added since the last call to <see cref="GetChanges(XRReferencePoint, Allocator)"/>.
            /// </summary>
            /// <param name="added">An already created array to populate. Its length must match <see cref="m_PendingAdds"/>.</param>
            void GetAdded(NativeArray<XRReferencePoint> added)
            {
                if (!added.IsCreated)
                    throw new ArgumentException("Array has not been created.", nameof(added));

                if (added.Length != m_PendingAdds.Count)
                    throw new ArgumentException($"Array is not the correct size. Should be {m_PendingAdds.Count} but is {added.Length}.", nameof(added));

                for (int i = 0; i < m_PendingAdds.Count; ++i)
                {
                    var referenceFrame = m_PendingAdds[i];
                    UpdateReferenceFrame(ref referenceFrame);

                    // Store it in the list of changes for this frame.
                    added[i] = referenceFrame.referencePoint;

                    // Add it to persistent storage for subsequent frames.
                    m_ReferenceFrames.Add(referenceFrame);
                }

                m_PendingAdds.Clear();
            }

            /// <summary>
            /// Returns an array containing all the updated reference points since the last
            /// call to <see cref="GetChanges(XRReferencePoint, Allocator)"/>.
            /// This method considers all <see cref="m_ReferenceFrames"/>, so it should
            /// be called before <see cref="GetAdded(NativeArray<XRReferencePoint>)"/> since
            /// that method will add elements to <see cref="m_ReferenceFrames"/>.
            /// </summary>
            /// <param name="allocator">The allocator to use for the returned array.</param>
            /// <param name="length">The number of updated reference points.</param>
            /// <returns>An array of updated reference points. Note the array's length
            /// will always be the total number of reference points. Use <paramref name="length"/>
            /// for the true number of updated reference points.</returns>
            NativeArray<XRReferencePoint> GetUpdated(Allocator allocator, out int length)
            {
                var updated = new NativeArray<XRReferencePoint>(m_ReferenceFrames.Count, allocator);
                length = 0;

                for (int i = 0; i < m_ReferenceFrames.Count; ++i)
                {
                    var referenceFrame = m_ReferenceFrames[i];
                    if (UpdateReferenceFrame(ref referenceFrame))
                    {
                        // Update the version in our persistent storage container
                        m_ReferenceFrames[i] = referenceFrame;
                        updated[length++] = referenceFrame.referencePoint;
                    }
                }

                return updated;
            }

            /// <summary>
            /// Populates <paramref name="removed"/> with the ids of the reference points
            /// removed since the last call to <see cref="GetChanges(XRReferencePoint, Allocator)"/>.
            /// </summary>
            /// <param name="removed">An already created array to populate. Its length must match <see cref="m_PendingRemoves"/>.</param>
            void GetRemoved(NativeArray<TrackableId> removed)
            {
                if (!removed.IsCreated)
                    throw new ArgumentException("Array has not been created.", nameof(removed));

                if (removed.Length != m_PendingRemoves.Count)
                    throw new ArgumentException($"Array is not the correct size. Should be {m_PendingRemoves.Count} but is {removed.Length}.", nameof(removed));

                for (int i = 0; i < removed.Length; ++i)
                {
                    removed[i] = m_PendingRemoves[i];
                }

                m_PendingRemoves.Clear();
            }

            public override unsafe TrackableChanges<XRReferencePoint> GetChanges(
                XRReferencePoint defaultReferencePoint,
                Allocator allocator)
            {
                using (var updated = GetUpdated(Allocator.Temp, out int updatedCount))
                {
                    var changes = new TrackableChanges<XRReferencePoint>(
                        m_PendingAdds.Count,
                        updatedCount,
                        m_PendingRemoves.Count,
                        allocator);

                    GetAdded(changes.added);
                    NativeArray<XRReferencePoint>.Copy(updated, changes.updated, updatedCount);
                    GetRemoved(changes.removed);

                    return changes;
                }
            }

            public override unsafe bool TryAddReferencePoint(Pose pose, out XRReferencePoint referencePoint)
            {
                if (m_TrackerHandle == Native.InvalidHandle)
                {
                    referencePoint = default;
                    return false;
                }

                // Get the pose position in right-handed coordinates
                var mlTarget = FlipHandedness(pose.position);

                // Get the ID of the closest PCF to our target position
                var getClosestResult = Native.GetClosest(m_TrackerHandle, ref mlTarget, out MLCoordinateFrameUID cfuid);
                if (getClosestResult != MLApiResult.Ok)
                {
                    LogWarning($"Could not create reference point because MLPersistentCoordinateFrameGetClosest returned {getClosestResult}.");
                    referencePoint = default;
                    return false;
                }

                // Get the pose of the PCF
                if (!Native.TryGetPose(cfuid, out Pose closetCoordinateFrame))
                {
                    LogWarning($"Could not create reference point because no pose could be determined for coordinate frame {cfuid}.");
                    referencePoint = default;
                    return false;
                }

                var referenceFrame = new ReferenceFrame(new ReferenceFrame.Cinfo
                {
                    closetCoordinateFrame = closetCoordinateFrame,
                    cfuid = cfuid,
                    trackingState = TrackingState.Tracking,
                    initialReferencePointPose = pose
                });

                m_PendingAdds.Add(referenceFrame);
                referencePoint = referenceFrame.referencePoint;

                return true;
            }

            /// <summary>
            /// Removes <paramref name="trackableId"/> from <paramref name="referenceFrames"/>
            /// or returns false if the reference frame with <paramref name="trackableId"/>
            /// is not found. If <paramref name="trackableId"/> is found, the dictionary
            /// of coordinate frames <see cref="m_CoordinateFrames"/> is also updated.
            /// </summary>
            /// <param name="trackableId">The id of the reference frame to remove.</param>
            /// <param name="referenceFrames">The list of reference frames to search.</param>
            /// <returns><c>true</c> if found, <c>false</c> otherwise.</returns>
            bool Remove(TrackableId trackableId, List<ReferenceFrame> referenceFrames)
            {
                // Removal is uncommon and we don't expect that many reference points,
                // so a linear search should do.
                for (int i = 0; i < referenceFrames.Count; ++i)
                {
                    var referenceFrame = referenceFrames[i];
                    if (referenceFrame.trackableId.Equals(trackableId))
                    {
                        referenceFrames.RemoveAt(i);
                        return true;
                    }
                }

                return false;
            }

            public override bool TryRemoveReferencePoint(TrackableId trackableId)
            {
                if (Remove(trackableId, m_PendingAdds))
                {
                    // Since it was pending, we have never reported it as added.
                    // That means we should not report it as removed, so take no action.
                    return true;
                }
                else if (Remove(trackableId, m_ReferenceFrames))
                {
                    // We must remember that we removed it here so that
                    // we can report it as removed in the next call to GetChanges
                    m_PendingRemoves.Add(trackableId);
                    return true;
                }
                else
                {
                    // We don't know about this reference point
                    return false;
                }
            }

            List<ReferenceFrame> m_PendingAdds = new List<ReferenceFrame>();
            List<ReferenceFrame> m_ReferenceFrames = new List<ReferenceFrame>();
            List<TrackableId> m_PendingRemoves = new List<TrackableId>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterDescriptor()
        {
#if PLATFORM_LUMIN
            XRReferencePointSubsystemDescriptor.Create(new XRReferencePointSubsystemDescriptor.Cinfo
            {
                id = "MagicLeap-ReferencePoint",
                subsystemImplementationType = typeof(MagicLeapReferencePointSubsystem),
                supportsTrackableAttachments = false
            });
#endif
        }
    }
}
