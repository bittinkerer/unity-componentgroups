using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Packages.Estenis.ComponentGroups_
{
    [DisallowMultipleComponent]
    public class ComponentGroupManager : MonoBehaviour
    {
        public readonly string ComponentGroupModuleName = "ComponentGroupModule";
        [SerializeField] private static Dictionary<string, ComponentGroup> _componentGroups = new();
        [SerializeField] private string _foo;
        [SerializeField] private List<string> _groups = new();

        public string[] GetGroupNames() => _groups.ToArray();

        public ComponentGroup AddComponentGroup(string groupName)
        {
            var componentGroup = gameObject.AddComponent<ComponentGroup>();
            componentGroup.SetTypeName(groupName);
            _groups.Add(groupName);

            return componentGroup;
        }

    }
}