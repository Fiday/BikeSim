#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(BikeVRInput))]
    public class BikeVRInputEditor : UnityEditor.Editor 
    {
        public override void OnInspectorGUI()
        {
            //button
            if (GUILayout.Button("Recenter Steering"))
            {
                Debug.Log("Steering recentered");
                ((BikeVRInput)target).RecenterSteering();
            }
            DrawDefaultInspector();
        }
    }
}
#endif