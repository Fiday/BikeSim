
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomAttributes
{
    [CustomPropertyDrawer(typeof(SceneSelectorAttribute))]
    public class SceneSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginProperty(position, label, property);

                List<string> sceneNames = new List<string>();
                
                Scene[] scenes = SceneManager.GetAllScenes();

                sceneNames.AddRange(scenes.Select(s => s.name).ToList());
                string propertyString = property.stringValue;
                int index = 0;
                
                for (int i = 0; i < sceneNames.Count; i++)
                {
                    if (sceneNames[i] == propertyString)
                    {
                        index = i;
                        break;
                    }
                }

                //Draw the popup box with the current selected index
                index = EditorGUI.Popup(position, label.text, index, sceneNames.ToArray());

                //Adjust the actual string value of the property based on the selection
                property.stringValue = sceneNames[index];


                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}

#endif