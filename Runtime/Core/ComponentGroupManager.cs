using System;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.Estenis.ComponentGroups_
{
    [DisallowMultipleComponent]
    public class ComponentGroupManager : MonoBehaviour
    {
        [SerializeField] private Dictionary<string, BaseComponentGroup> _componentGroups = new();

        public void AddGroup(string groupName)
        {
            Func<string, string, BaseComponentGroup> createTypeInstanceFunc = 
                (module, type) => TypeCreator.CreateTypeInstance<BaseComponentGroup>(module, type);
            Func<BaseComponentGroup, Component> addComponentAction = cg => gameObject.AddComponent(cg.GetType());

            // check if already exists
            if(_componentGroups.TryGetValue(groupName, out var componentGroup))
            {
                createTypeInstanceFunc = (module, type) => componentGroup;
                if(this.gameObject.GetComponent(componentGroup.GetType()) != null)
                {
                    addComponentAction = cg => null;
                }
            }

            // create type and instance
            var group = createTypeInstanceFunc("ComponentGroupModule", groupName);
            if(group == null)
            {
                UnityEngine.Debug.LogError($"Group Type could NOT be created for group {groupName}");
                return;
            }

            // add group to GameObject
            var component = addComponentAction(group);

            // associate group with groupName
            ObjectNamesUtility.SetTitleForType($"[{groupName.ToUpper()}]", group.GetType());

        }
    }
}