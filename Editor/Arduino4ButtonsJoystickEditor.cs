using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;



namespace ThreeLines.IOT.Arduino
{
    [CustomEditor(typeof(Arduino4ButtonsJoystick))]

    public class Arduino4ButtonsJoystickEditor : OdinEditor
    {
        const string UMLResourcesPath = "ThreeLinesIOT/Editor/Arduino4ButtonsJoystickUML";
        const string upPinEnumPropertyName = "upPin";
        const string downPinEnumPropertyName = "downPin";
        const string leftPinEnumPropertyName = "leftPin";
        const string rightPinEnumPropertyName = "rightPin";
        VisualElement root;
        
        VisualElement upPinHolder, 
            downPinHolder,
            leftPinHolder,
            rightPinHolder;
        Arduino4ButtonsJoystick script;
        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            var odinVE = new IMGUIContainer(()=>{
                Tree.Draw(true);
            });
            script = target as Arduino4ButtonsJoystick;
            var visualTree = Resources.Load<VisualTreeAsset>(UMLResourcesPath);
            if (visualTree != null)
            {
                visualTree.CloneTree(root);
            }
            SetupHolders();
            CreateProperatyFields();
            if (Application.isPlaying) {
                var read = script.GetJoystickDirection();
                ApplyJoyStickRead(read);
            }
            root.Add(odinVE);
            return root;
        }
        void SetupHolders()
        {
            upPinHolder = root.Q<VisualElement>(upPinEnumPropertyName);
            downPinHolder = root.Q<VisualElement>(downPinEnumPropertyName);
            leftPinHolder = root.Q<VisualElement>(leftPinEnumPropertyName);
            rightPinHolder = root.Q<VisualElement>(rightPinEnumPropertyName);
        }
        void CreateProperatyFields()
        {
            var upProperty = CreateProperatyField(upPinEnumPropertyName);
            upPinHolder.Add(upProperty);

            var downPropery = CreateProperatyField(downPinEnumPropertyName);
            downPinHolder.Add(downPropery);
            var leftProperty = CreateProperatyField(leftPinEnumPropertyName);
            leftPinHolder.Add(leftProperty);
            var rightProperty = CreateProperatyField(rightPinEnumPropertyName);
            rightPinHolder.Add(rightProperty);
        }
        
        void ApplyJoyStickRead(Vector2 joystickValue)
        {
            upPinHolder.Children().ElementAt(0).style.color = joystickValue.y > 0 ? Color.green : Color.white;
            downPinHolder.Children().ElementAt(0).style.color = joystickValue.y < 0 ? Color.green : Color.white;
            leftPinHolder.Children().ElementAt(0).style.color = joystickValue.x < 0 ? Color.green : Color.white;
            rightPinHolder.Children().ElementAt(0).style.color = joystickValue.x > 0 ? Color.green : Color.white;
        }
        
        PropertyField CreateProperatyField(string propertyName)
        {
            var property = serializedObject.FindProperty(propertyName);
            var propertyField = new PropertyField(property);
            propertyField.label = "";
            return propertyField;
        }
    }
}
