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

        //
        // For Components 
        public static string GetObjectTypeNameModified(UnityEngine.Object o, bool multiObjectEditing = false)
        {
            

            Debug.Log($"Running : {nameof(GetObjectTypeNameModified)}");
            if (o == null)
            {
                Debug.LogError($"{nameof(ObjectNamesUtility)}.{nameof(GetObjectTypeNameModified)} Error: Parameter, {nameof(o)} cannot be null.");
                return "SOMETHING_WENT_WRONG";
            }

            var getObjectTypeNameMethodInfo = typeof(ObjectNames).GetMethod(
                "GetObjectTypeName",
                 BindingFlags.NonPublic | BindingFlags.Static, 
                 null, 
                 new Type[] { typeof(UnityEngine.Object), typeof(bool) },
                 null);
            if(getObjectTypeNameMethodInfo == null)
            {
                Debug.LogError($"{nameof(ObjectNamesUtility)}.{nameof(GetObjectTypeNameModified)} Error: Could not get method for {nameof(ObjectNames)}.GetObjectTypeName.");
                return "SOMETHING_WENT_WRONG";
            }

            var getObjectTypeNameFunc = 
                (Func<UnityEngine.Object, bool, string>)Delegate.CreateDelegate(
                    typeof(Func<UnityEngine.Object, bool, string>), 
                    getObjectTypeNameMethodInfo);
            return getObjectTypeNameFunc(o, multiObjectEditing);

            
        }
    }
}