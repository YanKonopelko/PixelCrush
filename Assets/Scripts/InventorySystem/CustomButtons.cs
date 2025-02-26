#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ItemSerializier))]
[CanEditMultipleObjects]
public class CustomButtons : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ItemSerializier myScript = (ItemSerializier)target;
        if (GUILayout.Button("As Json"))
        {
            myScript.AsJson();
        }
        if (GUILayout.Button("From Json"))
        {
            myScript.FromJson();
        }
    }
}
#endif