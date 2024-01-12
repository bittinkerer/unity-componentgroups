using Packages.Estenis.ComponentGroups_;
using Packages.Estenis.UnityExts_;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public readonly string ComponentGroupModuleName = "ComponentGroupModule";

        public ComponentGroup Target => target as ComponentGroup;

        //
        // The following types should not be part of a group and should not be changed from group changes
        private string[] _groupExceptions = { "Transform", "ComponentFilter"};

        private IViewStrategy View => ViewContext.GetView(Target._selectedVisibility);


        public override VisualElement CreateInspectorGUI()
        {
            // Check if this is a copy-paste from different GO
            if(Target._components.Count > 0)
            {
                var componentsInTargetNotInGO = GetComponentsNotInGO(Target._components);
                CreateComponentsInGO(componentsInTargetNotInGO);
            }

            if (Application.isPlaying)
            {
                _editorAsset = 
                    AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                        "Packages/com.estenis.componentgroups/Editor/UI/UXML/ComponentGroupUXML.uxml");
                _componentAsset = 
                    AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                        "Packages/com.estenis.componentgroups/Editor/UI/UXML/ComponentUXML.uxml");
            }

            var root = _editorAsset.Instantiate();

            // Set-up View Dropdown
            var ddViewOptions = root.Q<DropdownField>("dd-view-options");
            ddViewOptions.RegisterValueChangedCallback(OnViewOptionsChanged);

            // Set up Name-Set and Name-Unlock Button events
            var setNameBtn = root.Q<UnityEngine.UIElements.Button>("SetGroupName");
            setNameBtn.clicked += () => SetNameBtn_clicked(root);
            var unlockNameBtn = root.Q<UnityEngine.UIElements.Button>("UnlockGroupName");
            unlockNameBtn.clicked += () => UnlockNameBtn_clicked(root);
            var refreshBtn = root.Q<Button>("refresh");
            refreshBtn.clicked += () => RefreshView_clicked(root);

            // Set up group components
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

        private void RefreshView_clicked(TemplateContainer root)
        {
            View.ShowGOComponents(Target);
            if (target)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void UnlockNameBtn_clicked(TemplateContainer root)
        {
            var setNameBtn = root.Q<UnityEngine.UIElements.Button>("SetGroupName");
            var unlockNameBtn = root.Q<UnityEngine.UIElements.Button>("UnlockGroupName");
            var nameTxt = root.Q<TextField>("group-name");
            nameTxt.isReadOnly = true;
            setNameBtn.style.display = DisplayStyle.Flex;
            unlockNameBtn.style.display = DisplayStyle.None;
        }

        private void SetNameBtn_clicked(TemplateContainer root)
        {
            var setNameBtn = root.Q<UnityEngine.UIElements.Button>("SetGroupName");
            var unlockNameBtn = root.Q<UnityEngine.UIElements.Button>("UnlockGroupName");
            var nameTxt = root.Q<TextField>("group-name");
            nameTxt.isReadOnly = false;
            setNameBtn.style.display = DisplayStyle.None;
            unlockNameBtn.style.display = DisplayStyle.Flex;
        }

        private void OnViewOptionsChanged(ChangeEvent<string> evt)
        {
            if (evt.previousValue == evt.newValue) return;

            Target._selectedVisibility = Enum.Parse<ViewMode>(evt.newValue.ToUpper());
            View.ShowGOComponents(Target);
            if (target)
            {
                EditorUtility.SetDirty(target);
            }
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
                    if (prop.GetSetMethod() != null)
                    {
                        prop.SetValue(newComponent, prop.GetValue(component._component));
                    }
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

            var dropdownButton = tc.Q<UnityEngine.UIElements.Button>("btn-expand");
            dropdownButton.clicked += () => DropdownButton_onClick(tc);
            var collapseButton = tc.Q<UnityEngine.UIElements.Button>("btn-collapse");
            collapseButton.clicked += () => CollapseButton_clicked(tc);

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

                // Add component inspector under the component details element
                if (evt.newValue != null)
                {
                    var parentElement = tc.Q("component-details");
                    parentElement.Add(new InspectorElement(evt.newValue));
                }

                EditorUtility.SetDirty(target);
            });
            return tc;
        }

        private void CollapseButton_clicked(TemplateContainer rootVisualElement)
        {
            rootVisualElement.Q("btn-collapse").style.display = DisplayStyle.None;
            rootVisualElement.Q("btn-expand").style.display = DisplayStyle.Flex;
            rootVisualElement.Q("component-details").style.display = DisplayStyle.None;
        }

        private void DropdownButton_onClick(TemplateContainer rootVisualElement)
        {
            rootVisualElement.Q("btn-collapse").style.display = DisplayStyle.Flex;
            rootVisualElement.Q("btn-expand").style.display = DisplayStyle.None;
            rootVisualElement.Q("component-details").style.display = DisplayStyle.Flex;
        }

        private void OnComponentChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            
        }

        private void GroupsListView_itemsSourceChanged()
        {
            
        }

        private void GroupsListView_itemIndexChanged(int arg1, int arg2)
        {
            
        }

        private void GroupsListView_itemsAdded(System.Collections.Generic.IEnumerable<int> addedItems)
        {
            foreach (var index in addedItems)
            {
                Target._components[index] = new ComponentData();
            }
        }

        //
        // Will bind to UI from backend data
        private void OnGroupsItemBound(VisualElement element, int index)
        {
            var componentField = element.Q<ObjectField>();
            
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
            
        }

    }
}