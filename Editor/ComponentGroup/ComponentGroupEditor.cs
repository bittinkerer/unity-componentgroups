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

        private void CreateComponentsInGO(List<ComponentData> components)
        {
            foreach (var component in components.Where(co => !co.IsNull))
            {
                // create
                var newComponent = Target.gameObject.AddComponent(component._component.GetType());

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

            Target._selectedVisibility = (VisibilityMode)changeEvent.newValue;
            switch (changeEvent.newValue)
            {
                case 0:
                    OnVisibilityDefault();
                    break;
                case 1:
                    OnVisibilityFocus();
                    break;
                case 2:
                    OnVisibilityShow();
                    break;
                default:
                    break;
            }
            if (target)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void OnVisibilityDefault()
        {
            var componentsInGoNotInGroup = Target.gameObject.GetComponents<Component>()
                .Where(c => !_groupExceptions.Any(s => s == c.GetType().Name)
                        && !Target._components.Any(co => co._component == c)
                        && c != Target
                        && (c && c != null));
            foreach (var component in componentsInGoNotInGroup)
            {
                component.UnhideInInspector();
            }
            foreach (var componentData in Target._components)
            {
                componentData._component.HideInInspector();
            }
            if (target)
            {
                EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Show all components in GO including group components
        /// </summary>
        private void OnVisibilityShow()
        {
            var componentsInGoNotInGroup = Target.gameObject.GetComponents<Component>()
                .Where(c => !_groupExceptions.Any(s => s == c.GetType().Name)
                        && !Target._components.Any(co => co._component == c)
                        && c != Target
                        && (c && c != null));
            foreach (var component in componentsInGoNotInGroup)
            {
                component.UnhideInInspector();
            }
            foreach (var componentData in Target._components)
            {
                componentData._component.UnhideInInspector();
            }
            if (target)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void OnVisibilityFocus()
        {
            var componentsInGoNotInGroup = Target.gameObject.GetComponents<Component>()
                .Where(c => !_groupExceptions.Any(s => s == c.GetType().Name)
                        && !Target._components.Any(co => co._component == c)
                        && c != Target
                        && (c && c != null));
            
            foreach (var component in componentsInGoNotInGroup)
            {
                component.HideInInspector();
            }
            foreach (var componentData in Target._components)
            {
                componentData._component.UnhideInInspector();
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
            //Debug.LogWarning($"{nameof(ComponentGroupEditor)}.{nameof(OnUpdate)}");
            bool changesMade = false;
            if (Target._selectedVisibility == VisibilityMode.DEFAULT)
            {
                List<ListDifference<ComponentData>> diffs = Target._componentsCopy.Differences(Target._components);
                if (diffs.Count == 0)
                {
                    return;
                }

                foreach (var diff in diffs)
                {
                    var component = diff.Item._component;
                    if (diff.ListChangeType == ListChangeType.ADDED)
                    {
                        changesMade = true;
                        HandleOnComponentAdded(component);
                    }
                    else if (diff.ListChangeType == ListChangeType.REMOVED)
                    {
                        changesMade = true;
                        HandleOnComponentRemoved(component);
                    }
                }
            }

            if (changesMade)
            {
                Target._componentsCopy = new(Target._components);
                if (target)
                {
                    EditorUtility.SetDirty(target);
                }
            }
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
            // Adjust visibility based on selected visibility mode
            if (Target._selectedVisibility == VisibilityMode.DEFAULT)
            {
                component.UnhideInInspector();
            }
        }

        private void HandleOnComponentAdded(Component component)
        {
            if (component == null)
            {
                return;
            }
            // Adjust visibility based on selected visibility mode
            if (Target._selectedVisibility == VisibilityMode.DEFAULT)
            {
                component.HideInInspector();
            }
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
                component.UnhideInInspector();
                Debug.Log($"[{Time.time}] Groups item Unbound for {component.name}");
            }
            else
            {
                Debug.Log($"[{Time.time}] Groups item Unbound for null component");
            }
        }

    }
}