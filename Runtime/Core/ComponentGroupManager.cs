using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Packages.Estenis.ComponentGroups_
{
    [DisallowMultipleComponent]
    public class ComponentGroupManager : MonoBehaviour
    {
        public readonly string ComponentGroupModuleName = "ComponentGroupModule";
        [SerializeField] private Dictionary<string, ComponentGroup> _componentGroups = new();
        [SerializeField] private string _foo;
        [SerializeField] private List<string> _groups = new();

        public string[] GetGroupNames() => _groups.ToArray();

        public void AddComponentGroup(string groupName)
        {
            var componentGroup = gameObject.AddComponent<ComponentGroup>();
            _groups.Add(groupName);

        }

        public void AddGroup(string groupName)
        {
            Func<string, string, Type> createTypeFunc = 
                (module, type) => TypeCreator.CreateType<ComponentGroup>(module, type);
            Func<Type, ComponentGroup> addComponentAction = ty => (ComponentGroup)gameObject.AddComponent(ty);

            // check if already exists
            if(_componentGroups.TryGetValue(groupName, out var componentGroup))
            {
                createTypeFunc = (module, type) => componentGroup.GetType();
                if(this.gameObject.GetComponent(componentGroup.GetType()) != null)
                {
                    addComponentAction = cg => null;
                }
            }

            // create type and instance
            var groupType = createTypeFunc(ComponentGroupModuleName, groupName);
            if(groupType == null)
            {
                UnityEngine.Debug.LogError($"Group Type could NOT be created for group {groupName}");
                return;
            }

            // add group to GameObject
            var component = addComponentAction(groupType);

            // associate group with groupName
            ObjectNamesUtility.SetTitleForType($"[{groupName.ToUpper()}]", component.GetType());
            _componentGroups[groupName] = component;
            _groups.Add(groupName);

        }
    }
}