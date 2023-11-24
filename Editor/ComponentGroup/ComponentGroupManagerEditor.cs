using Packages.Estenis.ComponentGroups_;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Packages.Estenis.ComponentGroupsEditor_
{
    [CustomEditor(typeof(ComponentGroupManager), true)]
    public class ComponentGroupManagerEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;
        private VisualElement _root;

        public ComponentGroupManager Target => target as ComponentGroupManager;

        public override VisualElement CreateInspectorGUI()
        {
            if(_inspectorXML == null )
            {
                return base.CreateInspectorGUI();
            }
            _root = new VisualElement(); 
            // Load and clone a visual tree from UXML
            _inspectorXML.CloneTree(_root); 

            var addGroupButton = _root.Query<Button>("AddGroupButton");
            addGroupButton.First().RegisterCallback<ClickEvent>(OnAddGroup);

            return _root;
        }

        private void OnAddGroup(ClickEvent evt)
        {
            // Get text field value 
            var groupName = _root.Q<TextField>("group-name").value;
            var cg = Target.AddComponentGroup(groupName);
        }

       
    }
}