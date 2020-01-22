using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.XR.MagicLeap.Remote;
#endif
using UnityEngine.Experimental;
using UnityEngine.Experimental.XR;
using UnityEngine.Jobs;
using UnityEngine.Lumin;
#if !NETFX_CORE && !NET_4_6 && !NET_STANDARD_2_0
using UnityEngine.XR.MagicLeap.Compatibility;
#endif

using UnityObject = UnityEngine.Object;

namespace UnityEngine.XR.MagicLeap.Rendering
{
    [AddComponentMenu("AR/Magic Leap/Magic Leap Camera")]
    [DefaultExecutionOrder(-15000)]
    [RequireComponent(typeof(Camera))]
    [UsesLuminPlatformLevel(2)]
    public sealed class MagicLeapCamera : MonoBehaviour
    {
        private Camera m_Camera;
#if ML_RENDERING_VALIDATION
        private Color m_PreviousClearColor;
#endif
        private List<Transform> _TransformList = new List<Transform>();
        private Unity.Jobs.JobHandle _Handle;

        [SerializeField]
        private Transform m_StereoConvergencePoint;
        [SerializeField]
        private FrameTimingHint m_FrameTimingHint;
        [SerializeField]
        private StabilizationMode m_StabilizationMode;
        [SerializeField]
        private float m_StabilizationDistance;
        [SerializeField]
        private bool m_ProtectedSurface;
        [SerializeField]
        private float m_SurfaceScale = 1f;

        public Transform stereoConvergencePoint
        {
            get { return m_StereoConvergencePoint; }
            set { m_StereoConvergencePoint = value; }
        }
        [Obsolete("Set frame timing hints via MagicLeapSettings.frameTimingHint instead")]
        public FrameTimingHint frameTimingHint
        {
            get { return m_FrameTimingHint; }
            set { m_FrameTimingHint = value; }
        }
        public StabilizationMode stabilizationMode
        {
            get { return m_StabilizationMode; }
            set { m_StabilizationMode = value; }
        }
        /// Expressed in Unity scene units.
        public float stabilizationDistance
        {
            get { return m_StabilizationDistance; }
            set { m_StabilizationDistance = value; }
        }
        public bool protectedSurface
        {
            get { return m_ProtectedSurface; }
            set { m_ProtectedSurface = value; }
        }
        [Obsolete("Use UnityEngine.XR.XRSettings.renderViewportScale instead")]
        public float surfaceScale
        {
            get { return m_SurfaceScale; }
            set { m_SurfaceScale = value; }
        }

        public bool enforceNearClip
        {
            get
            {
                return true;
            }
        }
        public bool enforceFarClip
        {
            get
            {
                return true;
            }
        }

        void Reset()
        {
            // allow obsolete usage for now.
#pragma warning disable 0618
            frameTimingHint = FrameTimingHint.Max_60Hz;
#pragma warning restore 0618
            stabilizationMode = StabilizationMode.FarClip;
            stabilizationDistance = (GetComponent<Camera>() != null) ? GetComponent<Camera>().farClipPlane : 100f;
        }

        void OnDestroy()
        {
        }

        void OnDisable()
        {
#pragma warning disable 0618
            RenderingSettings.useLegacyFrameParameters = true;
#pragma warning restore 0618
        }

        void OnEnable()
        {
#pragma warning disable 0618
            RenderingSettings.useLegacyFrameParameters = false;
#pragma warning restore 0618
        }

        void Awake()
        {
            m_Camera = GetComponent<Camera>();

#if PLATFORM_LUMIN && !UNITY_EDITOR
            RenderingSettings.useProtectedSurface = m_ProtectedSurface;
#endif
        }

        void LateUpdate()
        {
            NativeArray<float>? distances = null;
            if (stabilizationMode == StabilizationMode.FurthestObject)
            {
                var taa = new TransformAccessArray(_TransformList.ToArray());
                distances = new NativeArray<float>(taa.length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var job = new RenderingJobs.CalculateDistancesJob(distances.Value, transform.position);
                _Handle = job.Schedule(taa);
                _TransformList.Clear();
            }

            RenderingSettings.cameraScale = RenderingUtility.GetParentScale(transform);
            ValidateFarClip();
            ValidateNearClip();

            RenderingSettings.farClipDistance = m_Camera.farClipPlane;
            RenderingSettings.nearClipDistance = m_Camera.nearClipPlane;
            RenderingSettings.focusDistance = actualStereoConvergence;
#pragma warning disable 0618
            RenderingSettings.surfaceScale = m_SurfaceScale;
#pragma warning disable 0618
#if ML_RENDERING_VALIDATION
            CheckClearColor();
#endif
            switch (stabilizationMode)
            {
                case StabilizationMode.Custom:
                    RenderingSettings.stabilizationDistance = ClampToClippingPlanes(stabilizationDistance);
                    break;
                case StabilizationMode.FarClip:
                    RenderingSettings.stabilizationDistance = m_Camera.farClipPlane;
                    break;
                case StabilizationMode.FurthestObject:
                    _Handle.Complete();
                    RenderingSettings.stabilizationDistance = distances.Max();
                    distances.Value.Dispose();
                    distances = null;
                    break;
            }
        }

        public void ValidateFarClip()
        {
            if (!m_Camera) return;
            var farClip = m_Camera.farClipPlane;
            var max = RenderingSettings.maxFarClipDistance;
            if (enforceFarClip && farClip > max)
            {
                MLWarnings.WarnedAboutFarClippingPlane.Trigger(farClip, max);
                m_Camera.farClipPlane = max;
            }
        }

        public void ValidateNearClip()
        {
            if (!m_Camera) return;
            var nearClip = m_Camera.nearClipPlane;
            var min = RenderingSettings.minNearClipDistance;
            if (enforceNearClip && nearClip < min)
            {
                MLWarnings.WarnedAboutNearClippingPlane.Trigger(nearClip, min);
                m_Camera.nearClipPlane = min;
            }
        }

        private float actualStereoConvergence
        {
            get
            {
                // Get Focus Distance and log warnings if not within the allowed value bounds.
                float focusDistance = m_Camera.stereoConvergence;
                bool hasStereoConvergencePoint = stereoConvergencePoint != null;
                if (hasStereoConvergencePoint)
                {
                    // From Unity documentation:
                    // Note that camera space matches OpenGL convention: camera's forward is the negative Z axis.
                    // This is different from Unity's convention, where forward is the positive Z axis.
                    Vector3 worldForward = new Vector3(0.0f, 0.0f, -1.0f);
                    Vector3 camForward = m_Camera.cameraToWorldMatrix.MultiplyVector(worldForward);
                    camForward = camForward.normalized;

                    // We are only interested in the focus object's distance to the camera forward tangent plane.
                    focusDistance = Vector3.Dot(stereoConvergencePoint.position - transform.position, camForward);
                }
#if ML_RENDERING_VALIDATION
                float nearClip = m_Camera.nearClipPlane;
                if (focusDistance < nearClip)
                {
                    MLWarnings.WarnedAboutSteroConvergence.Trigger(hasStereoConvergencePoint);
                    focusDistance = nearClip;
                }
#endif
                m_Camera.stereoConvergence = focusDistance;

                return focusDistance;
            }
        }
        public float ClampToClippingPlanes(float value)
        {
            return Mathf.Clamp(value,
                RenderingSettings.minNearClipDistance,
                RenderingSettings.maxFarClipDistance);
        }

#if ML_RENDERING_VALIDATION
        private void CheckClearColor()
        {
            bool isClearingCorrectly = false;
            if (m_Camera.clearFlags == CameraClearFlags.SolidColor)
            {
                Color color = m_Camera.backgroundColor;
                if (m_PreviousClearColor != color)
                {
                    MLWarnings.WarnedAboutClearColor.Reset();
                    isClearingCorrectly = color == Color.clear;
                    m_PreviousClearColor = color;
                }
            }
            if (!isClearingCorrectly)
            {
                MLWarnings.WarnedAboutClearColor.Trigger();
            }
        }
#endif

        private void UpdateTransformList(Transform transform)
        {
            if (stabilizationMode == StabilizationMode.FurthestObject)
                _TransformList.Add(transform);
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MagicLeapCamera))]
    class MagicLeapCameraEditor : Editor
    {
        private const string kDefineRenderingValidation = "ML_RENDERING_VALIDATION";

        private static GUIContent kRenderingValidationText = new GUIContent("Runtime Rendering Validation", "Enable runtime checks for multiple camera settings");
        private static GUIContent kStereoConvergencePointText = new GUIContent("Stereo Convergence Point", "Transform you want to be the focus point of the camera");
        private static GUIContent kFrameTimingHintText = new GUIContent("Frame Timing Hint", "Select the frame timing hint render setting");
        private static GUIContent kStabilizationModeText = new GUIContent("Stabilization Mode", "Select the distance at which Stabilization mode activates");
        private static GUIContent kStabilizationDistanceText = new GUIContent("Stabilization Distance", "Custom value for Stabilization Distance");
        private static GUIContent kProtectedSurfaceText = new GUIContent("Protected Surface", "Content for this app is protected and should not be recorded or captured");
        private static GUIContent kSurfaceScaleText = new GUIContent("Surface Scale", "Scale Factor for the Render Surfaces");

#if ML_RENDERING_VALIDATION
        SerializedProperty previousClearColorProp;
#endif
        SerializedProperty stereoConvergenceProp;
        SerializedProperty stereoConvergencePointProp;
        SerializedProperty frameTimingHintProp;
        SerializedProperty stabilizationModeProp;
        SerializedProperty stabilizationDistanceProp;
        SerializedProperty protectedSurfaceProp;
        SerializedProperty surfaceScaleProp;

        AnimBool showDistanceField;

        private bool renderingValidationEnabled
        {
            get { return IsDefineSet(kDefineRenderingValidation); }
            set { ToggleDefine(kDefineRenderingValidation, value); }
        }

        private GameObject gameObject { get { return (target as MagicLeapCamera).gameObject; } }

        void OnEnable()
        {
#if ML_RENDERING_VALIDATION
            previousClearColorProp = serializedObject.FindProperty("m_PreviousClearColor");
#endif
            stereoConvergencePointProp = serializedObject.FindProperty("m_StereoConvergencePoint");

            frameTimingHintProp = serializedObject.FindProperty("m_FrameTimingHint");
            stabilizationModeProp = serializedObject.FindProperty("m_StabilizationMode");
            stabilizationDistanceProp = serializedObject.FindProperty("m_StabilizationDistance");
            protectedSurfaceProp = serializedObject.FindProperty("m_ProtectedSurface");
            surfaceScaleProp = serializedObject.FindProperty("m_SurfaceScale");

            showDistanceField = new AnimBool(stabilizationModeProp.enumValueIndex == (int)StabilizationMode.Custom);
        }

        public override void OnInspectorGUI()
        {
            //var rect = GUILayoutUtility.GetRect(kRenderingValidationText, EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label);
            renderingValidationEnabled = EditorGUILayout.Toggle(kRenderingValidationText, renderingValidationEnabled, GUILayout.ExpandWidth(true));

            serializedObject.Update();

            EditorGUILayout.ObjectField(stereoConvergencePointProp, typeof(Transform), kStereoConvergencePointText);

            bool foundLoader = MagicLeapLoader.assetInstance.IsEnabledForPlatform(EditorUserBuildSettings.activeBuildTarget);
            using (new EditorGUI.DisabledScope(foundLoader))
                EditorGUILayout.PropertyField(frameTimingHintProp, kFrameTimingHintText);
            if (foundLoader)
                EditorGUILayout.HelpBox("Frame Timing Hint is now set on the Magic Leap Settings panel in Player Settings -> XR", MessageType.Info);
            EditorGUILayout.PropertyField(stabilizationModeProp, kStabilizationModeText);
            showDistanceField.target = stabilizationModeProp.enumValueIndex == (int)StabilizationMode.Custom;
            using (var group = new EditorGUILayout.FadeGroupScope(showDistanceField.faded))
            if (group.visible)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(stabilizationDistanceProp, kStabilizationDistanceText);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(protectedSurfaceProp, kProtectedSurfaceText);

            using (new EditorGUI.DisabledScope(foundLoader))
                EditorGUILayout.Slider(surfaceScaleProp, 0f, 1f, kSurfaceScaleText);
            if (foundLoader)
                EditorGUILayout.HelpBox("Surface scale is now controlled via XRSettings.renderViewportScale", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
        }

        private bool IsDefineSet(string define)
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Lumin).Contains(define);
        }

        private void ToggleDefine(string define, bool active)
        {
            if (active)
            {
                if (IsDefineSet(define)) return;
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Lumin);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Lumin, string.Format("{0};{1}", defines, define));
            }
            else
            {
                if (!IsDefineSet(define)) return;
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Lumin).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Lumin, string.Join(";", defines.Where(d => d.Trim() != define).ToArray()));
            }
        }
    }
#endif
}