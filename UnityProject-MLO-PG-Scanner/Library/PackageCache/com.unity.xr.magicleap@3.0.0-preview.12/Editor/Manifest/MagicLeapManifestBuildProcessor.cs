using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using UnityEngine;

namespace UnityEditor.XR.MagicLeap
{
    class MagicLeapManifestBuildProcessor : IPostprocessBuildWithReport, IPreprocessBuildWithReport
    {
        const string kManifestTemplate = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<manifest xmlns:ml=""magicleap"">
  <application ml:sdk_version=""1.0"">
    <component ml:binary_name=""bin/Player.elf"" ml:name="".fullscreen"" ml:type=""Fullscreen"">
      <icon ml:model_folder=""Icon/Model"" ml:portal_folder=""Icon/Portal"" />
    </component>
  </application>
</manifest>";
        static XNamespace kML = "magicleap";
        const string kManifestExistsWarning = "Detected a custom manifest at {0}, this manifest will be used instead.";
        const string kManifestFailureError = "The manifest.xml file contained unexpected or deprecated content. Please correct the manifest file and try again. \n https://creator.magicleap.com/learn/guides/declare-your-manifest";
        const string kMinAPILevelAttributeName = "min_api_level";

        private bool m_WroteManifest = false;

        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (m_WroteManifest)
                File.Delete(MagicLeapManifestSettings.kDefaultManifestPath);
        }
        public void OnPreprocessBuild(BuildReport report)
        {
            //Debug.LogFormat("PreprocessBuild : Manifest");
            var path = MagicLeapManifestSettings.kDefaultManifestPath;
            if (MagicLeapManifestSettings.customManifestExists)
            {
                Debug.LogWarningFormat(kManifestExistsWarning, path);
                return;
            }
            MergeToCustomManifest(MagicLeap.MagicLeapManifestSettings.GetOrCreateSettings(), path);
        }

        private XDocument GetManifestTemplate(string path)
        {
            return XDocument.Load(new StringReader(kManifestTemplate.Trim()));
        }

        private void MergeToCustomManifest(MagicLeapManifestSettings ctx, string path)
        {
            m_WroteManifest = false;
            try
            {
                XDocument customManifest = GetManifestTemplate(path);
                customManifest.Declaration = new XDeclaration("1.0", "utf-8", null);

                // Find "manifest, ml:package" attribute and set to PlayerSettings.applicationIdentifier
                XElement manifestElement = customManifest.Element("manifest");
                if (manifestElement != null)
                {
                    manifestElement.SetAttributeValue(kML + "package", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Lumin));
                    manifestElement.SetAttributeValue(kML + "version_code", PlayerSettings.Lumin.versionCode);
                    manifestElement.SetAttributeValue(kML + "version_name", PlayerSettings.Lumin.versionName);
                }

                // Find "application, ml:visible_name" attribute and set to PlayerSettings.productName
                XElement applicationElement = (from node in customManifest.Descendants("application")
                    select node).SingleOrDefault();
                if (applicationElement != null)
                {
                    applicationElement.SetAttributeValue(kML + "visible_name", PlayerSettings.productName);

                    XAttribute minAPILevelAttribute = applicationElement.Attribute(kML + kMinAPILevelAttributeName);

                    // When the minimum API level is not specified, it is assumed to be 1 by the package manager.
                    if (minAPILevelAttribute == null)
                        applicationElement.SetAttributeValue(kML + kMinAPILevelAttributeName, ctx.minimumAPILevel);
                    else
                        minAPILevelAttribute.Value = ctx.minimumAPILevel.ToString();
                }

                // Find "component, ml:visible_name" attribute and set to PlayerSettings.productName
                // Find "component, ml:binary_name" attribute and set to "bin/Player.elf"
                IEnumerable<XElement> componentElements = from node in customManifest.Descendants("component")
                    where (string)node.Attribute(kML + "name") == ".fullscreen"
                    select node;

                if (Enumerable.Count(componentElements) > 1)
                {
                    Debug.LogError("Custom Manifest contained more than one component with name \".fullscreen\". Only one .fullscreen component is allowed.");
                    throw new System.Exception();
                }

                XElement componentElement = componentElements.FirstOrDefault();

                if (componentElement != null)
                {
                    componentElement.SetAttributeValue(kML + "name", ".fullscreen");
                    componentElement.SetAttributeValue(kML + "visible_name", PlayerSettings.productName);
                    componentElement.SetAttributeValue(kML + "binary_name", "bin/Player.elf");
                    componentElement.SetAttributeValue(kML + "type", PlayerSettings.Lumin.isChannelApp ? "ScreensImmersive" : "Fullscreen");

                    // Find "icon, model_folder" and set to "Icon/Model"
                    // Find or Add "icon, portal_folder" and set to "Icon/Portal"
                    XElement iconElement = (from node in componentElement.Descendants("icon")
                        select node).SingleOrDefault();
                    if (iconElement != null)
                    {
                        iconElement.SetAttributeValue(kML + "model_folder", "Icon/Model");
                        iconElement.SetAttributeValue(kML + "portal_folder", "Icon/Portal");
                    }
                }

                SetPrivileges(applicationElement, ctx.requiredPermissions.ToArray());

                customManifest.Save(path);
                m_WroteManifest = true;
            }
            catch (System.Exception e)
            {
                throw new Exception(kManifestFailureError, e);
            }
        }

        private static void AddPrivilegesIfMissing(XElement appElement, params string[] privs)
        {
            if (appElement == null || privs == null)
                return;

            foreach (var priv in privs)
            {
                XElement privElement = appElement.Elements("uses-privilege")
                    .Where(n => (string)n.Attribute(kML + "name") == priv).FirstOrDefault();
                if (privElement == null)
                {
                    appElement.Add(new XElement("uses-privilege", CreateAttribute("name", priv)));
                }
            }
        }

        private static void SetPrivileges(XElement appElement, params string[] privs)
        {
            if (appElement == null || privs == null)
                return;

            appElement.Elements("uses-privilege").Remove();

            foreach (var priv in privs)
            {
                appElement.Add(new XElement("uses-privilege", CreateAttribute("name", priv)));
            }
        }

        private static XAttribute CreateAttribute(string key, object value)
        {
            return new XAttribute(kML + key, value);
        }
    }
}