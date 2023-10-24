using Packages.Estenis.UnityExts_;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.Estenis.ComponentGroups_
{
    public class ComponentGroup : MonoBehaviour, ITypeNameProvider
    {
        [SerializeField] private string _typeName = "ComponentGroup";
        public ObservableList<Component> _components = new ();

        public ComponentGroup()
        {
            _components.CollectionChanged += _components_CollectionChanged;
        }

        private void Start()
        {
            
        }

        private void _components_CollectionChanged(
            object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.Log($"Collection has changed. {e.Action}");
        }

        public void SetTypeName(string typeName)
        {
            _typeName = typeName;
        }

        public string TypeName => _typeName;
    }
}