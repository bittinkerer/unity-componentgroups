using Packages.Estenis.ComponentGroups_;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Packages.Estenis.ComponentGroupsEditor_
{
    [CustomEditor(typeof(ComponentGroup), true)]
    public class ComponentGroupEditor : Editor
    {
        [SerializeField] private VisualTreeAsset _editorAsset;
        [SerializeField] private VisualTreeAsset _componentAsset;

        public override VisualElement CreateInspectorGUI()
        {
            var root = _editorAsset.CloneTree();            

            // Set up groups
            var groupsListView = root.Q<ListView>();
            groupsListView.makeItem = _componentAsset.CloneTree;
            groupsListView.bindItem += OnGroupsItemBound;
            groupsListView.unbindItem += OnGroupsItemUnBound;
            groupsListView.itemsAdded += GroupsListView_itemsAdded;

            // Set up focus


            return root;
        }

        

        private void GroupsListView_itemsAdded(System.Collections.Generic.IEnumerable<int> addedItems)
        {
            Debug.Log($"Groups item added:  {string.Join(',', addedItems.ToArray())}");
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