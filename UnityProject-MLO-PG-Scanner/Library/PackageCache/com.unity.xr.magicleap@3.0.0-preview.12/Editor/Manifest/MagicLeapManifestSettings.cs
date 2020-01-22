using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.XR.MagicLeap
{
    // Create a new type of Settings Asset.
    public class MagicLeapManifestSettings : ScriptableObject
    {
        public const string kDefaultManifestPath = "Assets/Plugins/Lumin/manifest.xml";
        public const string kDefaultSettingsPath = "Assets/Plugins/Lumin/MagicLeapManifestSettings.asset";

        [SerializeField]
        private int m_MinimumAPILevel;

        [SerializeField]
        private Privilege[] m_Privileges;

        public int minimumAPILevel
        {
            get
            {
                return m_MinimumAPILevel;
            }
            set
            {
                Undo.RecordObject(this, "Changed Minimum API Level");
                m_MinimumAPILevel = value;
            }
        }

        public IEnumerable<string> requiredPermissions
        {
            get
            {
                return m_Privileges.Where(p => p.enabled).Select(p => p.name);
            }
        }

        public bool TryGetPrivilegeRequested(string name, out bool isRequested)
        {
            isRequested = false;
            foreach (var priv in m_Privileges)
            {
                if (priv.name == name)
                {
                    isRequested = priv.enabled;
                    return true;
                }
            }
            return false;
        }

        public static bool customManifestExists
        {
            get { return File.Exists(kDefaultManifestPath); }
        }

        public static MagicLeapManifestSettings GetOrCreateSettings(string path = kDefaultSettingsPath)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var settings = AssetDatabase.LoadAssetAtPath<MagicLeapManifestSettings>(path);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<MagicLeapManifestSettings>();
                settings.m_MinimumAPILevel = 4;
                settings.m_Privileges = new Privilege[1] { new Privilege { name = "LowLatencyLightwear", enabled = true } };
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal SerializedObject ToSerializedObject()
        {
            return new SerializedObject(this);
        }
    }

    // Register a SettingsProvider using UIElements for the drawing framework:
    static class MagicLeapManifestSettingsRegister
    {
        const string AssetRoot = "Packages/com.unity.xr.magicleap/Editor/Manifest";
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("MagicLeap/", SettingsScope.Project)
            {
                label = "Manifest Settings",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    rootElement.Add(new ManifestEditor { settingsAsset = MagicLeapManifestSettings.GetOrCreateSettings() });
                },
                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Number", "Some String" })
            };

            return provider;
        }
    }
}