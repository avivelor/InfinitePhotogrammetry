namespace Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;
    using UnityEngine.TestTools;

    using UnityEditor;
    using UnityEditor.XR.MagicLeap;

    public class PrivilegeParserTests
    {
        const string kAssetRoot = "Packages/com.unity.xr.magicleap/Tests/Editor/Manifest";
        const string kHeaderTemplate = "Privileges_{0}.txt";

        [Test]
        public void CanParseHeaderFromSDK20()
        {
            var privs = PrivilegeParser.ParsePrivilegesFromHeader(GetHeaderForVersion("0.20.0"));
            Assert.That(privs, Is.Not.Empty);
        }

        [Test]
        public void CanParseHeaderFromSDK21()
        {
            var privs = PrivilegeParser.ParsePrivilegesFromHeader(GetHeaderForVersion("0.21.0"));
            Assert.That(privs, Is.Not.Empty);
        }

        [Test]
        public void CanParseHeaderFromCurrentSDK()
        {
            if (!SDKUtility.sdkAvailable)
                Assert.Ignore("Cannot locate Lumin SDK");
            var path = Path.Combine(SDKUtility.sdkPath, PrivilegeParser.kPrivilegeHeaderPath);
            var privs = PrivilegeParser.ParsePrivilegesFromHeader(path);
            Assert.That(privs, Is.Not.Empty);
        }

        private static string GetHeaderForVersion(string version)
        {
            return Path.Combine(kAssetRoot, string.Format(kHeaderTemplate, version));
        }
    }
}