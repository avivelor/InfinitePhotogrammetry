using System;

namespace UnityEngine.XR.MagicLeap
{
    [Flags]
    internal enum MLPlanesQueryFlags : uint
    {
        None = 0,

        /*! Include planes whose normal is perpendicular to gravity. */
        Vertical         = 1 << 0,
        /*! Include planes whose normal is parallel to gravity. */
        Horizontal       = 1 << 1,
        /*! Include planes with arbitrary normals. */
        Arbitrary        = 1 << 2,
        /*! Include all plane orientations. */
        AllOrientations  = Vertical |
                           Horizontal |
                           Arbitrary,
        /*! For non-horizontal planes, setting this flag will result in the top of
            the plane rectangle being perpendicular to gravity. */
        OrientToGravity  = 1 << 3,
        /*! If this flag is set, inner planes will be returned; if it is not set,
            outer planes will be returned. */
        Inner            = 1 << 4,
        /*!
            \brief Instructs the plane system to ignore holes in planar surfaces. If set,
            planes can patch over holes in planar surfaces. Otherwise planes will
            be built around holes.
            \deprecated Deprecated since 0.15.0.
            The expected behavior is - As long as a hole is big enough (diameter
            of the hole ~16cm), the inner planes will avoid covering the holes.
            The outer planes by definition will cover all the holes.
        */
        IgnoreHoles      = 1 << 5,
        /*! Include planes semantically tagged as ceiling. */
        Semantic_Ceiling = 1 << 6,
        /*! Include planes semantically tagged as floor. */
        Semantic_Floor   = 1 << 7,
        /*! Include planes semantically tagged as wall. */
        Semantic_Wall    = 1 << 8,
        /*! Include all planes that are semantically tagged. */
        Semantic_All     = Semantic_Ceiling |
                           Semantic_Floor |
                           Semantic_Wall,
        /*!
            \brief Include polygonal planes.
            When this flag is set:
                - MLPlanesQueryGetResultsWithBoundaries returns polygons along with
                other applicable rectangular planes. MLPlanesReleaseBoundariesList
                MUST be called before the next call to MLPlanesQueryGetResultsWithBoundaries
                or MLPlanesQueryGetResults, otherwise UnspecifiedFailure will be returned.
                - MLPlanesQueryGetResults returns just the rectangular planes.
                polygons (if any) extracted during the query will be discarded.
                No need to call MLPlanesReleaseBoundariesList before the
                next MLPlanesQueryGetResultsWithBoundaries or MLPlanesQueryGetResults.
            When this flag is not set:
                - both the APIs - MLPlanesQueryGetResultsWithBoundaries and
                MLPlanesQueryGetResults returns just rectangular planes.
                No need to call MLPlanesReleaseBoundariesList.
        */
        Polygons    = 1 << 9,
    }
}
