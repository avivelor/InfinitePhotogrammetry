using UnityEngine.XR;

namespace UnityEngine.Experimental.XR.MagicLeap
{
    public static class MagicLeapHeadUsages
    {
        public static InputFeatureUsage<float> confidence = new InputFeatureUsage<float>("MLHeadConfidence");

        public static InputFeatureUsage<float> FixationConfidence = new InputFeatureUsage<float>("MLFixationConfidence");
        public static InputFeatureUsage<float> EyeLeftCenterConfidence = new InputFeatureUsage<float>("MLEyeLeftCenterConfidence");
        public static InputFeatureUsage<float> EyeRightCenterConfidence = new InputFeatureUsage<float>("MLEyeRightCenterConfidence");

        public static InputFeatureUsage<uint> EyeCalibrationStatus = new InputFeatureUsage<uint>("MLEyeCalibrationStatus");
    }

    public static class MagicLeapControllerUsages
    {
        public static InputFeatureUsage<uint> ControllerType = new InputFeatureUsage<uint>("MLControllerType");
        public static InputFeatureUsage<uint> ControllerDOF = new InputFeatureUsage<uint>("MLControllerDOF");
        public static InputFeatureUsage<uint> ControllerCalibrationAccuracy = new InputFeatureUsage<uint>("MLControllerCalibrationAccuracy");

        public static InputFeatureUsage<float> ControllerTouch1Force = new InputFeatureUsage<float>("MLControllerTouch1Force");
        public static InputFeatureUsage<float> ControllerTouch2Force = new InputFeatureUsage<float>("MLControllerTouch2Force");

        // Was missing in CommonUsages
        public static InputFeatureUsage<bool> secondary2DAxisTouch = new InputFeatureUsage<bool>("Secondary2DAxisTouch");
    }

    public static class MagicLeapHandUsages
    {
        public static InputFeatureUsage<float> Confidence = new InputFeatureUsage<float>("MLHandConfidence");
        public static InputFeatureUsage<Vector3> NormalizedCenter = new InputFeatureUsage<Vector3>("MLHandNormalizedCenter");

        public static InputFeatureUsage<Vector3> WristCenter = new InputFeatureUsage<Vector3>("MLHandWristCenter");
        public static InputFeatureUsage<Vector3> WristUlnar = new InputFeatureUsage<Vector3>("MLHandWristUlnar");
        public static InputFeatureUsage<Vector3> WristRadial = new InputFeatureUsage<Vector3>("MLHandWristRadial");

        public static InputFeatureUsage<byte[]> KeyPoseConfidence = new InputFeatureUsage<byte[]>("MLHandKeyPoseConfidence");
        public static InputFeatureUsage<byte[]> KeyPoseConfidenceFiltered = new InputFeatureUsage<byte[]>("KeyPoseConfidenceFiltered");
    }
}