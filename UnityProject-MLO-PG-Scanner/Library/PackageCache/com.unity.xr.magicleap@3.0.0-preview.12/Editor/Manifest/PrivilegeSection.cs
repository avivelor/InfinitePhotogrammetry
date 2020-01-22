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
    public class PrivilegeSection : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<PrivilegeSection, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Category = new UxmlStringAttributeDescription { name = "category" };
            UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription { name = "label" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var lf = (PrivilegeSection)ve;
                var label = m_Label.GetValueFromBag(bag, cc);
                if (!string.IsNullOrEmpty(label))
                    lf.label = label;

                var category = m_Category.GetValueFromBag(bag, cc);
                if (!string.IsNullOrEmpty(category))
                {
                    Privilege.Category cat = Privilege.Category.Invalid;
                    if (Enum.TryParse(category, false, out cat))
                        lf.category = cat;
                }
            }
        }

        public static readonly string ussClassName = "ml-privilege-section";
        public static readonly string emptyVariantUssClassName = ussClassName + "--empty";
        public static readonly string headerUssClassName = ussClassName + "__header";
        public static readonly string bodyUssClassName = ussClassName + "__body";
        public static readonly string containerUssClassName = bodyUssClassName + "__container";
        public static readonly string listViewUssClassName = containerUssClassName + "__list-view";
        public static readonly string emptyLabelUssClassName = bodyUssClassName + "__empty-label";
        public static readonly string hiddenVariantEmptyLabelUssClassName = emptyLabelUssClassName + "--hidden";

        const int kItemSize = 15;

        static List<PrivilegeDescriptor> s_PrivilegeList;

        private Label m_Header;
        private VisualElement m_Body;
        private VisualElement m_BodyContainer;
        private ListView m_BodyListView;
        private Label m_BodyEmptyLabel;

        private Privilege.Category m_Category;
        private Func<string, bool> m_FilterCallback;
        private Func<string, bool> m_ForceCallback;
        private List<PrivilegeDescriptor> m_PrivilegeList;
        private Queue<Action> m_QueuedUpdates = new Queue<Action>();

        // FIXME :: This is kinda hacky, but it'll do until a more explict
        // way of binding the settings data can be completed.
        private MagicLeapManifestSettings m_Settings;

        public Privilege.Category category
        {
            get
            {
                return m_Category;
            }
            set
            {
                m_Category = value;
                m_PrivilegeList = GetPrivilegesFor(m_Category);
                m_BodyListView.style.height = m_PrivilegeList.Count * kItemSize;
                m_BodyListView.itemsSource = m_PrivilegeList;
                m_BodyEmptyLabel.EnableInClassList(hiddenVariantEmptyLabelUssClassName, m_PrivilegeList.Count > 0);
                EnableInClassList(emptyVariantUssClassName, m_PrivilegeList.Count > 0);
            }
        }

        public Func<string, bool> filterCallback
        {
            get
            {
                return m_FilterCallback;
            }
            set
            {
                m_FilterCallback = value;
                Refresh();
            }
        }

        public Func<string, bool> forceCallback
        {
            get
            {
                return m_ForceCallback;
            }
            set
            {
                m_ForceCallback = value;
                Refresh();
            }
        }

        public string label
        {
            get
            {
                return m_Header.text;
            }
            set
            {
                m_Header.text = value;
            }
        }

        public MagicLeapManifestSettings settings
        {
            get
            {
                return m_Settings;
            }
            set
            {
                m_Settings = value;
                Refresh();
            }
        }

        static PrivilegeSection()
        {
            s_PrivilegeList = PrivilegeParser.ParsePrivilegesFromHeader(Path.Combine(SDKUtility.sdkPath, PrivilegeParser.kPrivilegeHeaderPath)).ToList();
        }

        public PrivilegeSection()
            : this(null, Privilege.Category.Invalid)
        { }

        public PrivilegeSection(string label, Privilege.Category category)
        {
            m_Category = category;
            AddToClassList(ussClassName);
            AddToClassList(emptyVariantUssClassName);

            m_Header = new Label{ text = label };
            m_Header.AddToClassList(headerUssClassName);
            hierarchy.Add(m_Header);

            m_Body = new VisualElement();
            m_Body.AddToClassList(bodyUssClassName);
            hierarchy.Add(m_Body);

            m_BodyContainer = new VisualElement();
            m_BodyContainer.AddToClassList(containerUssClassName);
            m_Body.hierarchy.Add(m_BodyContainer);

            m_BodyListView = new ListView(null, kItemSize, MakeToggle, BindToggle);
            m_BodyListView.AddToClassList(listViewUssClassName);
            m_BodyContainer.hierarchy.Add(m_BodyListView);

            m_BodyEmptyLabel = new Label { text = "This section contains no privileges" };
            m_BodyEmptyLabel.AddToClassList(emptyLabelUssClassName);
            m_Body.hierarchy.Add(m_BodyEmptyLabel);
        }

        public void Refresh()
        {
            m_BodyListView.Refresh();
            this.schedule.Execute(() =>
                {
                    if (m_QueuedUpdates.Count > 0)
                        m_QueuedUpdates.Dequeue()();
                }).Every(50).Until(() => m_QueuedUpdates.Count == 0);
        }

        private static List<PrivilegeDescriptor> GetPrivilegesFor(Privilege.Category category)
        {
            return s_PrivilegeList
                .Where(p => p.category == category)
                .ToList();
        }

        private void BindToggle(VisualElement e, int i)
        {
            var t = (Toggle)e;
            if (m_PrivilegeList == null)
                return;
            t.label = m_PrivilegeList[i].name;
            t.tooltip = m_PrivilegeList[i].description;
            if (m_Settings != null)
            {
                SerializedProperty prop;
                bool forced = ShouldForce(t, m_PrivilegeList[i]);
                if (TryGetPropertyValue(t.label, out prop))
                {
                    t.BindProperty(prop);
                    if (ShouldFilter(t, m_PrivilegeList[i]))
                    {
                        m_QueuedUpdates.Enqueue(() => {
                            t.SetEnabled(false);
                            t.value = false;
                        });
                    }
                    else if (forced)
                    {
                        m_QueuedUpdates.Enqueue(() => {
                            t.SetEnabled(false);
                            t.value = true;
                        });
                    }
                }
                else
                {
                    // create a new serialized value.
                    var so = m_Settings.ToSerializedObject();
                    so.UpdateIfRequiredOrScript();
                    var privs = so.FindProperty("m_Privileges");
                    privs.InsertArrayElementAtIndex(privs.arraySize);
                    var el = privs.GetArrayElementAtIndex(privs.arraySize - 1);
                    el.FindPropertyRelative("name").stringValue = t.label;
                    var enabledProp = el.FindPropertyRelative("enabled");
                    enabledProp.boolValue = false;
                    t.BindProperty(enabledProp);
                    so.ApplyModifiedProperties();
                }
            }
        }

        private bool ShouldFilter(Toggle t, PrivilegeDescriptor p)
        {
            bool filter = false;
            // use the API level from the SDK data if available.
            if (p.apiLevel.HasValue)
                filter = filter || (p.apiLevel.Value > m_Settings.minimumAPILevel);
            if (filterCallback != null)
                filter = filter || filterCallback(t.label);
            return filter;
        }

        private bool ShouldForce(Toggle t, PrivilegeDescriptor p)
        {
            bool force = false;
            if (forceCallback != null)
                force = force || forceCallback(t.label);
            return force;
        }

        private Toggle MakeToggle()
        {
            var t = new Toggle();
            t.AddToClassList("reversed-toggle");
            return t;
        }

        private bool TryGetPropertyValue(string name, out SerializedProperty property)
        {
            property = null;
            if (m_Settings == null)
                return false;
            var so = m_Settings.ToSerializedObject();
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
                if (name == nameProp.stringValue)
                {
                    property = enabledProp;
                    return true;
                }
            }
            return false;
        }
    }
}