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

        private void OnEnable()
        {
            Debug.LogWarning($"{nameof(ComponentGroupManagerEditor)}.{nameof(OnEnable)}");
        }

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

            //return inspector;
            return _root;
        }

        private void OnAddGroup(ClickEvent evt)
        {
            UnityEngine.Debug.Log("Clicked Add Group!!");
            
            // Get text field value 
            var groupName = _root.Q<TextField>("group-name").value;
            ((ComponentGroupManager)target).AddComponentGroup(groupName);
        }
    }
}