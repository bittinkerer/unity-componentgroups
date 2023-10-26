
using Packages.Estenis.UnityExts_;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.Estenis.ComponentGroups_
{
    public class ComponentGroup : MonoBehaviour, ITypeNameProvider
    {
        [SerializeField] public bool _focus;
        [SerializeField] private string _typeName = "ComponentGroup";
        [SerializeField] public List<ComponentData> _components = new();

        


        public void SetTypeName(string typeName)
        {
            _typeName = typeName;
        }

        public string TypeName => _typeName;
    }

    [Serializable]
    public class ComponentData
    {
        public Component _component;
        
    }

}