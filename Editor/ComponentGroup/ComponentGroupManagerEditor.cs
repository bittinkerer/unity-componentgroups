using Packages.Estenis.ComponentGroups_;
using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine.UIElements;

namespace Packages.Estenis.ComponentGroupsEditor_
{
    [CustomEditor(typeof(ComponentGroupManager), true)]
    public class ComponentGroupManagerEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;


        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new VisualElement();            

            // Load and clone a visual tree from UXML
            _inspectorXML.CloneTree(inspector);

            var addGroupButton = inspector.Query<Button>("AddGroupButton");
            addGroupButton.First().RegisterCallback<ClickEvent>(OnAddGroup);

            //return inspector;
            return inspector;
        }

        private void OnAddGroup(ClickEvent evt)
        {
            UnityEngine.Debug.Log("Clicked Add Group!!");
            // Get text field value 
            ((ComponentGroupManager)target).AddGroup("test_1");
        }
    }
}