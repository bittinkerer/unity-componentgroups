
using Packages.Estenis.UnityExts_;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

namespace Packages.Estenis.ComponentGroups_
{
    public class ComponentGroup : MonoBehaviour, ITypeNameProvider
    {
        [SerializeField] public bool _focus;
        [SerializeField] public bool _show;
        [SerializeField] public ViewMode _selectedVisibility;
        [SerializeField] private string _typeName = "ComponentGroup";
        [SerializeField] public List<ComponentData> _components = new();
        [SerializeField] [HideInInspector] public List<ComponentData> _componentsCopy = new();        

        public void SetTypeName(string typeName)
        {
            _typeName = typeName;
        }

        public string TypeName => _typeName;
    }

    [Serializable]
    public class ComponentData : IEquatable<ComponentData>, INullable
    {
        [SerializeField] public Component _component;

        public override int GetHashCode()
        {
            return _component.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ComponentData);
        }

        public bool IsNull => _component == null;

        public bool Equals(ComponentData other)
        {
            if (_component == null && other == null) { return true; }
            if( _component == null || other == null) { return false; }
            return _component.Equals(other._component);
        }
    }

}