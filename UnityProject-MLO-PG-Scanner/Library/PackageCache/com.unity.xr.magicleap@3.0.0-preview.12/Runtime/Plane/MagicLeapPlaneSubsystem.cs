using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Lumin;
using UnityEngine.Scripting;
using UnityEngine.XR.MagicLeap.PlaneJobs;
using UnityEngine.XR.MagicLeap.Internal;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// The Magic Leap implementation of the <c>XRPlaneSubsystem</c>. Do not create this directly.
    /// Use <c>MagicLeapPlaneSubsystemDescriptor.Create()</c> instead.
    /// </summary>
    [Preserve]
    [UsesLuminPrivilege("WorldReconstruction")]
    public sealed class MagicLeapPlaneSubsystem : XRPlaneSubsystem
    {
        /// <summary>
        /// Gets a collection of plane boundaries associated with the plane with <paramref name="trackableId"/>.
        /// </summary>
        /// <remarks>
        /// The base class's <c>GetBoundary</c> method provides access to a single convex boundary.
        /// However, Magic Leap can produce multiple non-convex boundaries for each plane. This method
        /// provides access to those boundaries.
        /// </remarks>
        /// <param name="trackableId"></param>
        /// <returns>A <see cref="PlaneBoundaryCollection"/> containing all the boundaries associated
        /// with the plane with <paramref name="trackableId"/>.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the plane with the given <paramref cref="trackableId"/> cannot be found.</exception>
        /// <seealso cref="PlaneBoundaryCollection"/>
        public PlaneBoundaryCollection GetAllBoundariesForPlane(TrackableId trackableId)
        {
            return m_Provider.GetAllBoundariesForPlane(trackableId);
        }

        Provider m_Provider;

        protected override IProvider CreateProvider()
        {
            m_Provider = new Provider();
            return m_Provider;
        }

        internal static TrackableId GetTrackableId(ulong planeId)
        {
            const ulong planeTrackableIdSalt = 0xf52b75076e45ad88;
            return new TrackableId(planeId, planeTrackableIdSalt);
        }

        class Provider : IProvider
        {
            ulong m_PlanesTracker = Native.k_InvalidHandle;

            ulong m_QueryHandle = Native.k_InvalidHandle;

            uint m_MaxResults = 4;

            uint m_LastNumResults;

            Dictionary<TrackableId, BoundedPlane> m_Planes = new Dictionary<TrackableId, BoundedPlane>();

            MLPlaneBoundariesList m_BoundariesList;

            MLPlanesQueryFlags m_PlaneDetectionMode;

            PerceptionHandle m_PerceptionHandle;

            // todo: 2019-05-22: Unity.Collections.NativeHashMap would be better
            // but introduces another package dependency. Probably not worth it
            // for just this one thing, but if it becomes a dependency, we should
            // switch to using the NativeHashMap (or NativeHashSet if it exists).
            static HashSet<TrackableId> s_CurrentSet = new HashSet<TrackableId>();

            public Provider()
            {
                m_PerceptionHandle = PerceptionHandle.Acquire();
            }

            public override PlaneDetectionMode planeDetectionMode
            {
                set
                {
                    m_PlaneDetectionMode = MLPlanesQueryFlags.None;
                    if ((value & PlaneDetectionMode.Horizontal) != 0)
                        m_PlaneDetectionMode |= MLPlanesQueryFlags.Horizontal;
                    if ((value & PlaneDetectionMode.Vertical) != 0)
                        m_PlaneDetectionMode |= MLPlanesQueryFlags.Vertical;
                }
            }

            public override void Start()
            {
                var result = Native.Create(out m_PlanesTracker);
                if (result == MLApiResult.Ok)
                {
                    m_QueryHandle = BeginNewQuery();
                }
                else
                {
                    m_PlanesTracker = Native.k_InvalidHandle;
                    m_QueryHandle = Native.k_InvalidHandle;
                }

                if (m_BoundariesList.valid)
                {
                    Debug.LogError($"Restarting the plane subsystem with an existing boundaries list.");
                }

                m_BoundariesList = MLPlaneBoundariesList.Create();
            }

            public override void Stop()
            {
                if (m_PlanesTracker != Native.k_InvalidHandle)
                {
                    if (m_BoundariesList.valid)
                    {
                        Native.ReleaseBoundaries(m_PlanesTracker, ref m_BoundariesList);
                        m_BoundariesList = MLPlaneBoundariesList.Create();
                    }

                    Native.Destroy(m_PlanesTracker);
                    m_PlanesTracker = Native.k_InvalidHandle;
                }

                m_QueryHandle = Native.k_InvalidHandle;
            }

            public override void Destroy()
            {
                m_PerceptionHandle.Dispose();
            }

            public unsafe PlaneBoundaryCollection GetAllBoundariesForPlane(TrackableId trackableId)
            {
                if (!m_Planes.TryGetValue(trackableId, out BoundedPlane plane))
                    return default;

                // MLPlaneBoundaries is an array of boundaries, so planeBoundariesArray represents an array of MLPlaneBoundaries
                // which is itself an array of boundaries.
                var planeBoundariesArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<MLPlaneBoundaries>(
                    m_BoundariesList.plane_boundaries,
                    (int)m_BoundariesList.plane_boundaries_count,
                    Allocator.None);

                // Find the plane boundaries with the given trackable id
                foreach (var planeBoundaries in planeBoundariesArray)
                {
                    if (GetTrackableId(planeBoundaries.id) == trackableId)
                    {
                        return new PlaneBoundaryCollection(planeBoundaries, plane.pose);
                    }
                }

                return default;
            }

            public unsafe override void GetBoundary(
                TrackableId trackableId,
                Allocator allocator,
                ref NativeArray<Vector2> convexHullOut)
            {
                var boundaries = GetAllBoundariesForPlane(trackableId);
                if (boundaries.count > 0)
                {
                    // TODO 2019-05-21: handle multiple boundaries?
                    using (var polygon = boundaries[0].GetPolygon(Allocator.TempJob))
                    {
                        ConvexHullGenerator.Giftwrap(polygon, allocator, ref convexHullOut);
                        return;
                    }
                }

                CreateOrResizeNativeArrayIfNecessary<Vector2>(0, allocator, ref convexHullOut);
            }

            public unsafe override TrackableChanges<BoundedPlane> GetChanges(
                BoundedPlane defaultPlane,
                Allocator allocator)
            {
                if (m_QueryHandle == Native.k_InvalidHandle)
                {
                    m_QueryHandle = BeginNewQuery();
                    return default;
                }
                else
                {
                    // Get results
                    var mlPlanes = new NativeArray<MLPlane>((int)m_MaxResults, Allocator.TempJob);
                    if (m_BoundariesList.valid)
                    {
                        Native.ReleaseBoundaries(m_PlanesTracker, ref m_BoundariesList);
                    }
                    m_BoundariesList = MLPlaneBoundariesList.Create();

                    try
                    {
                        var result = Native.QueryGetResultsWithBoundaries(
                            m_PlanesTracker, m_QueryHandle,
                            (MLPlane*)mlPlanes.GetUnsafePtr(), out uint numResults, ref m_BoundariesList);

                        switch (result)
                        {
                            case MLApiResult.Ok:
                            {
                                m_LastNumResults = numResults;
                                m_QueryHandle = BeginNewQuery();

                                using (var uPlanes = new NativeArray<BoundedPlane>((int)numResults, Allocator.TempJob))
                                {
                                    new CopyPlaneResultsJob
                                    {
                                        planesIn = mlPlanes,
                                        planesOut = uPlanes
                                    }.Schedule((int)numResults, 1).Complete();

                                    var added = new NativeFixedList<BoundedPlane>((int)numResults, Allocator.Temp);
                                    var updated = new NativeFixedList<BoundedPlane>((int)numResults, Allocator.Temp);
                                    var removed = new NativeFixedList<TrackableId>((int)numResults, Allocator.Temp);

                                    s_CurrentSet.Clear();
                                    for (int i = 0; i < numResults; ++i)
                                    {
                                        var uPlane = uPlanes[i];
                                        var trackableId = uPlane.trackableId;
                                        s_CurrentSet.Add(trackableId);

                                        if (m_Planes.ContainsKey(trackableId))
                                        {
                                            updated.Add(uPlane);
                                        }
                                        else
                                        {
                                            added.Add(uPlane);
                                        }

                                        m_Planes[trackableId] = uPlane;
                                    }

                                    // Look for removed planes
                                    foreach (var kvp in m_Planes)
                                    {
                                        var trackableId = kvp.Key;
                                        if (!s_CurrentSet.Contains(trackableId))
                                        {
                                            removed.Add(trackableId);
                                        }
                                    }

                                    foreach (var trackableId in removed)
                                    {
                                        m_Planes.Remove(trackableId);
                                    }

                                    using (added)
                                    using (updated)
                                    using (removed)
                                    {
                                        var changes = new TrackableChanges<BoundedPlane>(
                                            added.Length,
                                            updated.Length,
                                            removed.Length,
                                            allocator);

                                        added.CopyTo(changes.added);
                                        updated.CopyTo(changes.updated);
                                        removed.CopyTo(changes.removed);

                                        return changes;
                                    }
                                }
                            }
                            case MLApiResult.Pending:
                            {
                                return default;
                            }
                            default:
                            {
                                m_QueryHandle = BeginNewQuery();
                                return default;
                            }
                        }
                    }
                    finally
                    {
                        mlPlanes.Dispose();
                    }
                }
            }

            ulong BeginNewQuery()
            {
                // We hit the max, so increase for next time
                if (m_MaxResults == m_LastNumResults)
                    m_MaxResults = m_MaxResults * 3 / 2;

                var query = new MLPlanesQuery
                {
                    flags = m_PlaneDetectionMode | MLPlanesQueryFlags.Polygons,
                    bounds_center = Vector3.zero,
                    bounds_rotation = Quaternion.identity,
                    bounds_extents = Vector3.zero,
                    max_results = m_MaxResults,
                    min_plane_area = 0.25f
                };

                ulong queryHandle;
                var result = Native.QueryBegin(m_PlanesTracker, in query, out queryHandle);
                if (result != MLApiResult.Ok)
                {
                    return Native.k_InvalidHandle;
                }

                return queryHandle;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterDescriptor()
        {
#if PLATFORM_LUMIN
            XRPlaneSubsystemDescriptor.Create(new XRPlaneSubsystemDescriptor.Cinfo
            {
                id = "MagicLeap-Planes",
                subsystemImplementationType = typeof(MagicLeapPlaneSubsystem),
                supportsHorizontalPlaneDetection = true,
                supportsVerticalPlaneDetection = true,
                supportsArbitraryPlaneDetection = true,
                supportsBoundaryVertices = true,
            });
#endif
        }

        static class Native
        {
            public const ulong k_InvalidHandle = ulong.MaxValue;

            const string Library = "ml_perception_client";

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPlanesCreate")]
            public static extern MLApiResult Create(out ulong planes_tracker);

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPlanesDestroy")]
            public static extern MLApiResult Destroy(ulong planes_tracker);

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPlanesQueryBegin")]
            public static extern MLApiResult QueryBegin(ulong planes_tracker, in MLPlanesQuery query, out ulong request_handle);

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPlanesQueryGetResults")]
            public static extern unsafe MLApiResult QueryGetResults(ulong planes_tracker, ulong query_handle, MLPlane* out_results, out uint num_results);

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPlanesQueryGetResultsWithBoundaries")]
            public static extern unsafe MLApiResult QueryGetResultsWithBoundaries(ulong planes_tracker, ulong planes_query, MLPlane* out_results, out uint out_num_results, ref MLPlaneBoundariesList out_boundaries);

            [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLPlanesReleaseBoundariesList")]
            public static extern MLApiResult ReleaseBoundaries(ulong planes_tracker, ref MLPlaneBoundariesList plane_boundaries);
        }
    }
}
