using Packages.Estenis.UnityExts_;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Packages.Estenis.ComponentGroups_
{
    public class ObjectNamesUtility
    {
        public static Dictionary<Type, string> GetInternalInspectorTitlesCache()
        {
            Type inspectorTitlesType =
                typeof(ObjectNames)
                    .GetNestedType("InspectorTitles", BindingFlags.Static | BindingFlags.NonPublic);
            var inspectorTitlesField = inspectorTitlesType.GetField(
                "s_InspectorTitles",
                BindingFlags.Static | BindingFlags.NonPublic);
            return (Dictionary<Type, string>)inspectorTitlesField.GetValue(null);
        }

        public static void SetTitleForType(string title, Type type)
        {
            var titlesCache = GetInternalInspectorTitlesCache();
            if (titlesCache.ContainsKey(type))
            {
                UnityEngine.Debug.LogWarning($"Inspector Titles Cache already contains {type.Name}");
                return;
            }
            titlesCache.Add(type, title);
        }

    }
}