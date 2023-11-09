using Packages.Estenis.ComponentGroups_;
using Packages.Estenis.UnityExts_;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Packages.Estenis.ComponentGroupsEditor_
{
    [CustomEditor(typeof(ComponentGroup), true)]
    public class ComponentGroupEditor : Editor
    {
        [SerializeField] private VisualTreeAsset _editorAsset;
        [SerializeField] private VisualTreeAsset _componentAsset;

        public ComponentGroup Target => target as ComponentGroup;

        //
        // The following types should not be part of a group and should not be changed from group changes
        private string[] _groupExceptions = { "Transform", "ComponentFilter"};

        private IViewStrategy View => ViewContext.GetView(Target._selectedVisibility);


        public override VisualElement CreateInspectorGUI()
        {
            Debug.Log($"Creating Inspector for {Target.GetType().Name}");

            // Check if this is a copy-paste from different GO
            if(Target._components.Count > 0)
            {
                var componentsInTargetNotInGO = GetComponentsNotInGO(Target._components);
                CreateComponentsInGO(componentsInTargetNotInGO);
            }

            // Keep track of copy for changes detected to components list
            OnUpdate();
            //Target._componentsCopy = new(Target._components);

            // Set up update loop
            EditorApplication.update += OnUpdate;

            if (Application.isPlaying)
            {
                _editorAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.estenis.componentgroups/Editor/UI/UXML/ComponentGroupUXML.uxml");
                _componentAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.estenis.componentgroups/Editor/UI/UXML/ComponentUXML.uxml");
            }

            var root = _editorAsset.Instantiate();

            // Visibility Radio buttons
            var visibilityField = root.Q<RadioButtonGroup>("_visibility");
            visibilityField.value = (int)Target._selectedVisibility;
            visibilityField.RegisterCallback<ChangeEvent<int>>(ce => HandleVisibilityChange(ce, visibilityField));
            
            Debug.Log($"Enable view {((ViewMode)visibilityField.value)}");

            // Set up groups
            var groupsListView = root.Q<ListView>();
            groupsListView.itemsSource = Target._components;

            //
            // Event Handling
            groupsListView.makeItem = MakeItem;
            groupsListView.bindItem += OnGroupsItemBound;
            groupsListView.unbindItem += OnGroupsItemUnBound;
            groupsListView.itemsAdded += GroupsListView_itemsAdded;
            groupsListView.itemsSourceChanged += GroupsListView_itemsSourceChanged;
            groupsListView.itemIndexChanged += GroupsListView_itemIndexChanged;

            // Set up focus

            return root;
        }

        private void CreateComponentsInGO(List<ComponentData> components)
        {
            foreach (var component in components.Where(co => !co.IsNull))
            {
                // create
                var newComponent = Target.gameObject.AddComponent(component._component.GetType());
                if(!newComponent || newComponent == null)
                {
                    Debug.LogWarning($"{nameof(ComponentGroup)}.{nameof(CreateComponentsInGO)} could not create component {component._component.GetType().Name}");
                    return;
                }

                // copy values
                foreach (var field in newComponent.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                {
                    field.SetValue(newComponent, field.GetValue(component._component));
                }

                foreach (var prop in newComponent.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(p => p.Module.Name != "UnityEngine.CoreModule.dll"))
                {
                    prop.SetValue(newComponent, prop.GetValue(component._component));
                }

                // update reference
                component._component = newComponent;
            }
        }

        //
        // Event Handlers
        //

        private void HandleVisibilityChange(ChangeEvent<int> changeEvent, RadioButtonGroup radioButtonGroup)
        {
            if(changeEvent.newValue == changeEvent.previousValue)
            {
                return;
            }

            Target._selectedVisibility = (ViewMode)changeEvent.newValue;
            View.ShowGOComponents(Target);

            if (target)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private List<ComponentData> GetComponentsNotInGO(List<ComponentData> components) =>
            components
                .Where(grcd => !Target.gameObject.GetComponents<Component>().Any(goco => goco == grcd._component)) 
                .ToList();
               
        /// <summary>
        /// Depending on mode it will show/hide components
        /// </summary>
        private void OnUpdate()
        {
            // check for diffs
            List<ListDifference<ComponentData>> diffs = Target._componentsCopy.Differences(Target._components);
            if(diffs.Count == 0)
            {
                return;
            }

            // update components shown
            View.ShowGOComponents(Target);
            //View.UpdateGoComponentsOnDiffs(diffs);

            if(target)
            {
                EditorUtility.SetDirty(target);
            }

            // sync copy
            Target._componentsCopy = new(Target._components);
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
                
                View.UpdateGOComponentsOnDiffs(
                    new List<ListDifference<Component>> {
                        new ListDifference<Component> { ListChangeType = ListChangeType.ADDED, Item = (Component)evt.newValue },
                        new ListDifference<Component> { ListChangeType = ListChangeType.REMOVED, Item = (Component)evt.previousValue },
                    });
                Target._components[index]._component = (Component)evt.newValue;

                // Add component inspector under the component selector
                if (evt.newValue != null)
                {
                    var parentElement = tc.Q("component-details");
                    parentElement.Add(new InspectorElement(evt.newValue));
                }

                EditorUtility.SetDirty(target);
            });
            return tc;
        }

        private void OnComponentChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            Debug.Log($"[{Time.time}] {nameof(OnComponentChanged)}");
        }

        private void GroupsListView_itemsSourceChanged()
        {
            Debug.Log("[{Time.time}] Items Source changed.");
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

            if (componentField?.value)
            {                
                Debug.Log($"[{Time.time}] Groups item Unbound for {componentField.value.name}");
            }
            else
            {
                Debug.Log($"[{Time.time}] Groups item Unbound for null component");
            }

            componentField.userData = index;    // order matter, this statement needs to happen before assignment of value
            componentField.value = Target._components[index]._component;
            
        }

        /// <summary>
        /// Unbinds element from data
        /// NOTE: Be-careful, unbinds happens for more than just removal of item from visual element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index"></param>
        private void OnGroupsItemUnBound(VisualElement element, int index)
        {
            var component = (Component)element.Q<ObjectField>()?.value;
            if (component)
            {
                Debug.Log($"[{Time.time}] Groups item Unbound for {component.name}");
                // @TODO: Should update Target._components ??
            }
            else
            {
                Debug.Log($"[{Time.time}] Groups item Unbound for null component");
            }
        }

    }
}