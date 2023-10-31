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

        public ComponentGroup Target => target as ComponentGroup;

        private TemplateContainer _componentTemplate;

        public override VisualElement CreateInspectorGUI()
        {
            if (Application.isPlaying)
            {
                _editorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.estenis.componentgroups/Editor/UI/UXML/ComponentGroupUXML.uxml");
                _componentAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.estenis.componentgroups/Editor/UI/UXML/ComponentUXML.uxml");
            }

            var root = _editorAsset.Instantiate();

            // Set up groups
            var groupsListView = root.Q<ListView>();
            groupsListView.itemsSource = Target._components;

            groupsListView.makeItem = MakeItem;
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

        private TemplateContainer MakeItem()
        {
            var tc          = _componentAsset.Instantiate();
            int lastIndex   = Target._components.Count - 1;
            var objectField = tc.Q<ObjectField>();

            tc.Q<ObjectField>().RegisterValueChangedCallback(evt =>
            {
                int index = objectField?.userData == null 
                                ? lastIndex
                                : (objectField.userData as int?).Value;
                Target._components[index]._component = (Component)evt.newValue;
                HandleOnComponentRemoved((Component)evt.previousValue);
                HandleOnComponentAdded(Target._components[index]._component);
                EditorUtility.SetDirty(target);
            });
            return tc;
        }

        private void HandleOnComponentRemoved(Component component)
        {
            if(component == null)
            {
                return;
            }
            component.UnhideInInspector();
        }

        private void HandleOnComponentAdded(Component component)
        {
            if (component == null)
            {
                return;
            }
            component.HideInInspector();
        }

        private void OnComponentChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            Debug.Log($"[{Time.time}] {nameof(OnComponentChanged)}");
        }

        private void GroupsListView_itemsSourceChanged()
        {
            Debug.Log("[{Time.time}] Items Source changed.");
        }

        private void OnComponentsChanged(SerializedProperty property)
        {
            Debug.Log($"[{Time.time}] Components changed.");
        }

        private void GroupsListView_itemIndexChanged(int arg1, int arg2)
        {
            Debug.Log($"[{Time.time}] Item index changed");
        }

        private void GroupsListView_itemsAdded(System.Collections.Generic.IEnumerable<int> addedItems)
        {
            Debug.Log($"[{Time.time}] Groups item added:  {string.Join(',', addedItems.ToArray())}");
            foreach (var index in addedItems)
            {
                Target._components[index] = new ComponentData();
            }
        }

        //
        // Will bind to UI from backend data
        private void OnGroupsItemBound(VisualElement element, int index)
        {
            Debug.Log($"[{Time.time}] Groups item bound");
            var componentField = element.Q<ObjectField>();
            componentField.userData = index;    // order matter, this statement needs to happen before assignment of value
            componentField.value = Target._components[index]._component;
            
        }

        private void OnGroupsItemUnBound(VisualElement element, int index)
        {
            Debug.Log($"[{Time.time}] Groups item Unbound");
        }

    }
}