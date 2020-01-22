using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace UnityEditor.XR.MagicLeap
{
    public class ManifestEditor : VisualElement
    {
        const string kAssetRoot = "Packages/com.unity.xr.magicleap/Editor/Manifest";
        //const string kCustomManifestWarning = "Detected a custom manifest in the project.\nThe custom manifest will override these settings unless it is removed.";
        const string kCustomManifestWarning = "Detected a custom manifest at Assets/Plugins/Lumin/manifest.xml.\nThis manifest will be used instead and will override UI settings for\nPrivileges, Minimum API level and other publishing selections.";
        const int kItemSize = 15;

        private MagicLeapManifestSettings m_SettingsAsset;
        private SerializedObject m_SerializedSettings;

        private Dictionary<string, Privilege.Category> m_PrivilegeList;

        private PlatformLevelSelector m_APILevel;

        private IVisualElementScheduledItem m_ManifestPoller;

        public MagicLeapManifestSettings settingsAsset
        {
            get
            {
                return m_SettingsAsset;
            }
            set
            {
                m_SettingsAsset = value;
                UpdateSettings();
            }
        }

        private SerializedObject serializedSettings
        {
            get
            {
                return m_SerializedSettings;
            }
        }

        private bool canEdit => !MagicLeapManifestSettings.customManifestExists;

        public ManifestEditor()
        {
            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(kAssetRoot + "/ManifestEditor.uss");
            styleSheets.Add(styleSheet);

            // Import UXML
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(kAssetRoot + "/ManifestEditor.uxml");
            tree.CloneTree(this);

            m_APILevel = this.Q<PlatformLevelSelector>("api-label");
            m_APILevel.RegisterValueChangedCallback(RefreshLists);

            TextField cmw = this.Q<TextField>("custom-manifest-warning");
            cmw.focusable = false;

            m_ManifestPoller = this.schedule.Execute(() =>
                {
                    cmw.value = kCustomManifestWarning;
                    cmw.EnableInClassList("warning--hidden", canEdit);
                    this.Query<VisualElement>(null, "lockable").ForEach(ve => ve.SetEnabled(canEdit));
                }).Every(500L);

            cmw.value = kCustomManifestWarning;
            cmw.EnableInClassList("warning--hidden", canEdit);
            this.Query<VisualElement>(null, "lockable").ForEach(ve => ve.SetEnabled(canEdit));
        }

        private void RefreshLists(ChangeEvent<int> evt)
        {
            this.schedule.Execute(() =>
            {
                this.Query<PrivilegeSection>().ForEach(ps => ps.Refresh());
                MarkDirtyRepaint();
            });
        }

        private void UpdateSettings()
        {
            if (m_SettingsAsset == null)
            {
                this.Unbind();
                m_SerializedSettings = null;
                return;
            }
            else
                m_SerializedSettings = m_SettingsAsset.ToSerializedObject();
                {
                    m_SerializedSettings.UpdateIfRequiredOrScript();
                    // automatically bump the platform API the lowest possible version.
                    var prop = m_SerializedSettings.FindProperty("m_MinimumAPILevel");
                    prop.intValue = PlatformLevelSelector.EnsureValidValue(prop.intValue);
                    m_SerializedSettings.ApplyModifiedProperties();
                }
                this.Bind(serializedSettings);

            this.Query<PrivilegeSection>().ForEach(ps => {
                ps.filterCallback = FilterPrivilege;
                ps.forceCallback = ForcePrivilege;
                ps.settings = m_SettingsAsset;
            });
        }

        private bool FilterPrivilege(string name)
        {
            var level = this.m_APILevel.value;
            // added in platform level 4
            if (level < 4)
            {
                if (name == "CoarseLocation")
                    return true;
            }
            // removed in platform level 4
            if (level >= 4)
            {
                if (name == "Occlusion")
                    return true;
            }
            return false;
        }

        private bool ForcePrivilege(string name)
        {
            if (name == "LowLatencyLightwear")
                return true;
            return false;
        }

        private IEnumerable<SerializedProperty> GetPrivilegeProperties()
        {
            var so = serializedSettings;
            var iter = so.FindProperty("m_Privileges");
            var sz = iter.arraySize;
            for (int i = 0; i < sz; i++)
            {
                var e = iter.GetArrayElementAtIndex(i);
                var nameProp = e.FindPropertyRelative("name");
                if (nameProp == null)
                    continue;
                var enabledProp = e.FindPropertyRelative("enabled");
                if (enabledProp == null)
                    continue;
                yield return e;
            }
        }
    }

    class TestWindow : EditorWindow
    {
        //[MenuItem("Magic Leap/Manifest Test")]
        static void Init()
        {
            TestWindow window = EditorWindow.GetWindow<TestWindow>();
            window.Show();
        }

        void OnEnable()
        {
            rootVisualElement.Add(new ManifestEditor { settingsAsset = MagicLeapManifestSettings.GetOrCreateSettings() });
        }
    }
}
