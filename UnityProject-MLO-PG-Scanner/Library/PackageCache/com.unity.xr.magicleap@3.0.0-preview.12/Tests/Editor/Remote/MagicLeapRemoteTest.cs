using UnityEngine.TestTools;

namespace UnityEditor.XR.MagicLeap.Testing
{
    public abstract class MagicLeapRemoteTest : IPostBuildCleanup, IPrebuildSetup
    {
        public void Cleanup()
        {}

        public void Setup()
        {}
    }
}