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
    public class PlatformLevelSelector : PopupField<int>
    {
        public new class UxmlFactory : UxmlFactory<PlatformLevelSelector, UxmlTraits> {}

        public new class UxmlTraits : PopupField<int>.UxmlTraits
        {
            UxmlIntAttributeDescription m_Value = new UxmlIntAttributeDescription { name = "value" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var lf = (PlatformLevelSelector)ve;
                int val = 0;
                if (!m_Value.TryGetValueFromBag(bag, cc, ref val))
                    val = GetChoices().Last();
                lf.SetValueWithoutNotify(val);
            }
        }

        public PlatformLevelSelector() 
            : this(null) { }

        public PlatformLevelSelector(string label)
            : base(label, GetChoices().ToList(), 0)
        {
        }

        public static IEnumerable<int> GetChoices()
        {
            int min = SDKUtility.pluginAPILevel;
            int max = SDKUtility.sdkAPILevel;
            for (int i = min; i <= max; i++)
                yield return i;
        }

        public static int EnsureValidValue(int input)
        {
            var max = GetChoices().Max();
            var min = GetChoices().Min();
            if (input < min)
                return min;
            if (input > max)
                return max;
            return input;
        }
    }
}