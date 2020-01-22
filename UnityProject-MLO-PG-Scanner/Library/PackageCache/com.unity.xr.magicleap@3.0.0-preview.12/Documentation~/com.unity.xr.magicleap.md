# About Magic Leap XR Plugin

Use the *Magic Leap XR Plugin* package enables Magic Leap Spatial Mapping support via Unity's multi-platform XR API. This package implements the following XR Subsystems:

* [Display](https://docs.unity3d.com/2018.3/Documentation/ScriptReference/Experimental.XR.XRDisplaySubsystem.html)
* [Input](https://docs.unity3d.com/2018.3/Documentation/ScriptReference/Experimental.XR.XRInputSubsystem.html)
* [Meshing](https://docs.unity3d.com/2018.3/Documentation/ScriptReference/Experimental.XR.XRMeshingSubsystem.html)

This version of *Magic Leap XR Plugin* supports the meshing functionality provided by the Magic Leap One:

* Generate meshes of the environment
* Generate point clouds of the environment

# Installing Magic Leap XR Plugin

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

# Using Magic Leap XR Plugin

The *Magic Leap XR Plugin* implements the native endpoints required for meshing using Unity's multi-platform XR API.

Inclusion of the *Magic Leap XR Plugin* will result in the inclusion of a C# component, runtime libraries, and plugin metadata.

## Spatial Mapper

This package includes the `MLSpatialMapper` component:

![alt text](images/mlspatialmapper_component.png "MLSpatialMapper Component")

The spatial mapper generates [`Mesh`es](https://docs.unity3d.com/ScriptReference/Mesh.html) from data collected from the Magic Leap One's depth sensor. Each mesh is a relatively small area of the environment. A separate [`GameObject`](https://docs.unity3d.com/ScriptReference/GameObject.html) is created for each generated mesh.

|Field|Description|
|-|-|
|Mesh Prefab|This is the prefab that will be instantiated for each generated mesh. The prefab should at least have a [`MeshFilter`](https://docs.unity3d.com/ScriptReference/MeshFilter.html) on it. To visualize the meshes, add a [`MeshRenderer`](https://docs.unity3d.com/ScriptReference/MeshRenderer.html). If a [`MeshCollider`](https://docs.unity3d.com/ScriptReference/MeshCollider.html) is present, then a physics collider will be generated as well. This happens on a background thread, so it will not stall the main thread.|
|Compute Normals|If `true`, will request that the device also generate a normal for each vertex. If `false`, normals will be calculated from the triangle data.|
|Mesh Parent|The parent transform for the generated meshes. By default, the `MLSpatialMapper` will select its own parent, so that the generated meshes will be siblings of the `MLSpatialMapper`'s [`GameObject`](https://docs.unity3d.com/ScriptReference/GameObject.html).|
|Type|Whether to generate triangle meshes or a point cloud. If you select point cloud, then the resulting [`Mesh`](https://docs.unity3d.com/ScriptReference/Mesh.html) will have [`MeshTopology.Points`](https://docs.unity3d.com/ScriptReference/MeshTopology.Points.html).
|Level of Detail (LOD)|The detail of the generated meshes. Lower levels of detail will result in simplified meshes, and will reduce CPU and GPU load, but will be less accurate. Higher LOD levels will be more accurate, but require more CPU and GPU resources.|
|Mesh Queue Size|The number of concurrent meshes to generate. Each frame, the `MLSpatialMapper` will add meshes to a generation queue as necessary. Larger values will lead to heavier CPU usage, but will generate meshes faster.|
|Fill Hole Length|Perimeter (in meters) of holes you wish to have filled.|
|Polling Rate|How often to check for updates, in seconds. More frequent updates will increase CPU usage.|
|Batch Size|Maximum number of meshes to update per batch. Larger values are more efficient, but have higher latency.|
|Planarize|If `true`, the system will planarize the returned mesh (planar regions will be smoothed out).|
|Disconnected Component Area|Any component that is disconnected from the main mesh and which has an area (in meters squared) less than this size will be removed.|
|Request Vertex Confidence|If `true`, the system will compute the confidence values for each vertex, ranging from 0 to 1.|
|Remove Mesh Skirt|If `true`, the mesh skirt (overlapping area between two meshes) will be removed.|

### Bounds

Meshes are only generated inside a specific region, relative to the Magic Leap One's starting position. The `MLSpatialMapper`'s [`transform`](https://docs.unity3d.com/ScriptReference/Transform.html) is used to define this region. The [`transform`](https://docs.unity3d.com/ScriptReference/Transform.html)'s `localPosition`, `localRotation`, and `localScale` are used for this calculation.

A green, transparent box is drawn in Unity's Scene View to indicate the area in which meshing will occur:

![alt text](images/meshing_gizmo.png "Meshing Gizmo")

## Spaces

Meshes are generated in "device space", also know as "session relative space". When the Magic Leap One boots up, its initial position is `(0, 0, 0)`. All meshing data is relative to this initial location.

The [`Camera`](https://docs.unity3d.com/ScriptReference/Camera.html)'s [`GameObject`](https://docs.unity3d.com/ScriptReference/GameObject.html) should have an identity transform to begin with.

![alt text](images/magic_leap_root_camera.png "Magic Leap Root Camera")

If you wish to have the device start at a different location within the Unity scene, we recommend you use a parent [`GameObject`](https://docs.unity3d.com/ScriptReference/GameObject.html) to provide position, rotation, and scale (the `Magic Leap Root` in this image):

![alt text](images/magic_leap_root_with_transform.png "Magic Leap Root")

## Gesture

Subsystem implementation to provide for recognition and tracking of gestures provided from the appropriate device.  This subsystem relies on the `com.unity.xr.interactionsubsystem` package for it's core implementation (see that package's documentation for further details/types).

The `MagicLeapGestureSubsystem` component manages a low-level interface for polling for Magic Leap gesture changes.  If this component is added to a scene, it is possible to poll for any gesture events each frame.  The following gestures and gesture data are provided:

* __MagicLeapKeyPoseGestureEvent__ - Event that fires when a key pose gesture changes.  See the Magic Leap documentation for further documenation on key poses.
  * __id__ - Unique `GestureId` that identifies this gesture.
  * __state__ - `GestureState` that indicates the state of this gesture (`Started`, `Updated`, `Completed`, `Canceled` or `Discrete`).
  * __keyPose__ - `MagicLeapKeyPose` indicating type of key pose that has been detected.  Valid key poses are `Finger`, `Fist`, `Pinch`, `Thumb`, `LShape`, `OpenHand`, `Ok`, `CShape`, `NoPose`, `NoHand` are all valid key poses.
  * __hand__ - `MagicLeapHand` indicating the hand source for this gesture (`Left` or `Right`);
* __MagicLeapTouchpadGestureEvent__ - Event that fires when a touchpad gesture changes.  See the Magic Leap documentation for further documenation on the touchpad.
  * __id__ - Unique `GestureId` that identifies this gesture.
  * __state__ - `GestureState` that indicates the state of this gesture (`Started`, `Updated`, `Completed`, `Canceled` or `Discrete`).
  * __controllerId__ - The controller id associated with this gesture.
  * __angle__ - Angle from the center of the touchpad to the finger.
  * __direction__ - `MagicLeapTouchpadGestureDirection` indicating the direction of this gesture (`None`, `Up`, `Down`, `Left`, `Right`, `In`, `Out`, `Clockwise`, `CounterClockwise`).
  * __fingerGap__ - Distance between the two fingers performing the gestures in touchpad distance. The touchpad is defined as having extents of [-1.0,1.0] so this distance has a range of [0.0,2.0].
  * __positionAndForce__ - Gesture position (x,y) and force (z). Position is in the [-1.0,1.0] range and force is in the [0.0,1.0] range.
  * __radius__ - For radial gestures, this is the radius of the gesture. The touchpad is defined as having extents of [-1.0,1.0] so this radius has a range of [0.0,2.0].
  * __speed__ - Speed of gesture. Note that this takes on different meanings depending on the gesture type being performed:
  * __type__ - `MagicLeapInputControllerTouchpadGestureType` indicating the type of this gesture (`None`, `Tap`, `ForceTapDown`, `ForceTapUp`, `ForceDwell`, `SecondForceDown`, `LongHold`, `RadialScroll`, `Swipe`, `Scroll`, `Pinch`).

Additionally, the `MagicLeapGestures` component can be used to provide a simpler polling mechanism for a simpler, event-based interface for listening to gestures.  This component provides a number of events that be hooked into to detect gesture events when they occur:

* __onKeyPoseGestureChanged__ - Occurs whenever a key pose gesture changes state.
* __onTouchpadGestureChanged__ - Occurs whenever a touchpad gesture changes state.
* __onActivate__ - Occurs whenever the cross-platform activate gesture occurs.  See the `com.unity.xr.interactionsubsystems` package documentation for more details.

Also see the relevant Magic Leap documentation about gestures for supported device information.

# Technical details
## Requirements

This version of *Magic Leap XR Plugin* is compatible with the following versions of the Unity Editor:

* Unity 2019.2

## Known limitations

No known issues.

## Package contents

This version of *Magic Leap XR Plugin* includes:

* A shared library which provides implementation of the XR Subsystems listed above
* A plugin metadata file

## Document revision history
|Date|Reason|
|---|---|
|June 1, 2018|Create initial documentation.|
|August 17, 2018|Minor updates to docs to refer to 2018.3 version.|
|June 20, 2019|Minor updates to reflect 2019.2|
