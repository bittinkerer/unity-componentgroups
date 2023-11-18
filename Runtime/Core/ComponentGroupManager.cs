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
        [SerializeField] private static Dictionary<string, ComponentGroup> _componentGroups = new();
        [SerializeField] private string _foo;
        [SerializeField] private List<string> _groups = new();

        private void Awake()
        {
            // Rebuild components if necessary
            Debug.LogWarning("Hello1");
            var components = this.gameObject.GetComponents<MonoBehaviour>();
        }

        private void OnEnable()
        {
            Debug.LogWarning("Hello1");
            var components = this.gameObject.GetComponents<MonoBehaviour>();
        }

        private void Start()
        {
            Debug.LogWarning("Hello1");
        }

        public string[] GetGroupNames() => _groups.ToArray();

        public void AddComponentGroup(string groupName)
        {
            var componentGroup = gameObject.AddComponent<ComponentGroup>();
            componentGroup.SetTypeName(groupName);
            _groups.Add(groupName);

        }

        public static ComponentGroup GetByName(string groupName) => 
            _componentGroups[groupName];
        public static ComponentGroup GetByInstanceId(int instanceId) => 
            _componentGroups.Values.Where(g => g.GetInstanceID() == instanceId).FirstOrDefault();

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
            //if (component)
            //{
            //    component.Manager = this;
            //}

            // associate group with groupName
            ObjectNamesUtility.SetTitleForType($"[{groupName.ToUpper()}]", component.GetType());
            _componentGroups[groupName] = component;
            _groups.Add(groupName);

        }
    }
}