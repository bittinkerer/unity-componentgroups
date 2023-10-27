using Packages.Estenis.ComponentGroups_;
using Packages.Estenis.UnityExts_;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Packages.Estenis.ComponentGroupsEditor_
{
    [CustomEditor(typeof(ComponentGroup), true)]
    public class ComponentGroupEditor : Editor
    {
        [SerializeField] private VisualTreeAsset _editorAsset;
        [SerializeField] private VisualTreeAsset _componentAsset;

        private TemplateContainer _componentTemplate;

        public override VisualElement CreateInspectorGUI()
        {
            var root = _editorAsset.CloneTree();
            _componentTemplate = _componentAsset.CloneTree();

            var componentObjectField = _componentTemplate.Q<ObjectField>();
            componentObjectField.RegisterValueChangedCallback(OnComponentChanged);

            // Set up groups
            var groupsListView = root.Q<ListView>();
            //groupsListView.makeItem = _componentAsset.CloneTree;
            groupsListView.makeItem = () =>
            {
                var tc = _componentAsset.Instantiate();
                tc.Q<ObjectField>().RegisterValueChangedCallback(evt =>
                {
                    var component = (Component)evt.newValue;
                    component.HideInInspector();
                    Debug.Log("Is this working or what..");
                });
                return tc;
            };

            groupsListView.bindItem += OnGroupsItemBound;
            groupsListView.unbindItem += OnGroupsItemUnBound;
            groupsListView.itemsAdded += GroupsListView_itemsAdded;
            groupsListView.itemsSourceChanged += GroupsListView_itemsSourceChanged;
            groupsListView.itemIndexChanged += GroupsListView_itemIndexChanged;

            groupsListView.TrackPropertyValue(
                new SerializedObject( target ).FindProperty("_components"), OnComponentsChanged);

            // Set up focus


            return root;
        }

        private void OnComponentChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            Debug.Log($"{nameof(OnComponentChanged)}");
        }

        private void GroupsListView_itemsSourceChanged()
        {
            Debug.Log("Items Source changed.");
        }

        private void OnComponentsChanged(SerializedProperty property)
        {
            Debug.Log("Components changed.");
        }

        private void GroupsListView_itemIndexChanged(int arg1, int arg2)
        {
            Debug.Log("Item index changed");
        }

        private void GroupsListView_itemsAdded(System.Collections.Generic.IEnumerable<int> addedItems)
        {
            Debug.Log($"Groups item added:  {string.Join(',', addedItems.ToArray())}");
            _componentTemplate.Query<ObjectField>()
                .ForEach(of =>
                {
                    of.RegisterValueChangedCallback(OnComponentChanged);
                    of.RegisterCallback<ChangeEvent<ObjectField>>(OnComponentAChange);
                });
            var foo = _componentTemplate.Query<ObjectField>();
            //last.RegisterValueChangedCallback(OnComponentChanged);
        }

        private void OnComponentAChange(ChangeEvent<ObjectField> evt)
        {
            Debug.Log("COMOI");
        }

        private void OnGroupsItemBound(VisualElement element, int index)
        {
            Debug.Log("Groups item bound");
        }

        private void OnGroupsItemUnBound(VisualElement element, int index)
        {
            Debug.Log("Groups item unbound");
        }

    }
}