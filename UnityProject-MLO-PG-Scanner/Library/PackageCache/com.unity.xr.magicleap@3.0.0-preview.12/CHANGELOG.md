# Changelog

## [3.0.0-preview.12] - 2019-08-23
- Fix an issue on OSX that caused ML Remote to intermittenly fail to initialize

## [3.0.0-preview.11] - 2019-08-20
- Fix an issue where the ML Remote check was wrong on OSX

## [3.0.0-preview.10] - 2019-08-13
- Stop scaling the near clip plane
- Revert the "LOD" setting change from 3.0.0-preview.7
- Fix Issue 1174014: Play in Editor in PC mode with Magic Leap loader and AR gestures will crash Unity Editor
- Add an explicit binary check when looking to launch ML Remote
- Allow for loading of gesture subsystem via XR Management

## [3.0.0-preview.9] - 2019-07-25
- Fix an issue with Yamato promotion pipeline

## [3.0.0-preview.8] - 2019-07-12
- Update package description to include note about legacy XR
- Set the default frame timing hint to 60Hz
- Deprecate the "LOD" setting on MLSpatialMapper; it's now exposed as "Density" to be consistent with the XR Mesh subsystem API in 2019.3

## [3.0.0-preview.7] - 2019-07-09
- Update documentation for 2019.2
- Robustify Meshing
- Fix an issue where the frame timing hint was being incorrectly overridden
- Disable support for Legacy XR

## [3.0.0-preview.6] - 2019-06-20
- Fix an issue with meshing causing settings values to use garbage data
- Add support for determining the origin controller of a touchpas gesture event
- Fix a couple issues around proper handling of multiple controllers
- Properly support standalone subsystems that depend on the perception system
- Add support for standalone Planes, Raycast, and ReferencePoint subsystems
- Update Gestures documentation
- Fix a type collision with MagicLeap's Unity framework
- Add initial support for custom MagicLeap settings when using XR SDK
- Fix a couple issues that arise when using XR SDK, ML Remote, and repeatedly going in and out of playmode
- Bump Legacy Input Helpers to 1.3.2
- Fix an issue where timeouts from the ML Graphics API would cause the XR Display subsystem to shutdown
- Add support for multipass rendering on Lumin hardware and on ML Remote on Windows
- Fix an issue where XRSettings.renderViewportScale wasn't being propagated to ML's Graphics API

## [3.0.0-preview.5] - 2019-06-11
- Fix the native controller api loader to properly reference `ml_perception_client` instead of `ml_input`
- Fix an issue that prevented the Display provider from properly initializing in Editor using ML Remote
- Disable some old testing menu items
- Fix a couple cases where the UnityMagicLeap plugin would crash because it couldn't load the ML Remote libraries
- Add Multipass support for ML Remote on OSX
- Fix a bug where ML Remote / Zero Iteration on device would silently fail when using the XR SDK implementation
- Add some native support for managing controller feedback

## [3.0.0-preview.4] - 2019-05-20
- Update yamato configuration
- Improve how various ML input devices are handled via XR Input
- Simplify ML Remote library loading in the native plugin

## [3.0.0-preview.3] - 2019-05-18
- Update third party notices

## [3.0.0-preview.2] - 2019-05-17

## [3.0.0-preview.1] - 2019-05-17
- Add support for Unity 2019.2
- Add support for XR Display Subsystem
- Remove disabled clipping plane enforcement toggles
- Add support for hand tracking
- Add Manifest Editor UI
- Update package to build against 0.20.0 MLSDK
- Add support for starting / stopping ML Remote server headlessly via the Unity TestRunner
- Add standalone Gestures subsystem
- Do not fail when requesting confidence for a zero-vertex mesh
- Don't generate colliders for point cloud style meshes

## [2.0.0-preview.14] - 2019-03-05
- Initial Production release
- Fix a number of issues causing instabilty when using ML Remote