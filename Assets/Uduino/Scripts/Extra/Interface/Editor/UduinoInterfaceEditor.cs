using UnityEngine;
using UnityEditor;

namespace Uduino
{
    [CustomEditor(typeof(UduinoInterface_Serial))]
    public class UduinoInterfaceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Interface", EditorStyles.boldLabel);
            DrawDefaultInspector();
        }
    }
}