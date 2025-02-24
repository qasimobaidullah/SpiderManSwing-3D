using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ResetBoxColliders : EditorWindow
{
    private GameObject selectedObject;
    private List<BoxCollider> boxColliders = new List<BoxCollider>();
    private Vector2 scrollPosition;

    [MenuItem("LadyBug/Box Collider List Window")]
    public static void ShowWindow()
    {
        GetWindow<ResetBoxColliders>("Box Collider List");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a GameObject to list all Box Colliders", EditorStyles.boldLabel);

        selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            EditorGUILayout.HelpBox(
                "No GameObject selected. Please select a GameObject in the hierarchy.",
                MessageType.Warning
            );
            return;
        }

        if (GUILayout.Button("List Box Colliders"))
        {
            FindBoxColliders();
        }

        GUILayout.Space(10f);

        EditorGUILayout.LabelField("Box Colliders:");

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (BoxCollider collider in boxColliders)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(collider, typeof(BoxCollider), true);
            if (GUILayout.Button("Reset"))
            {
                ResetBoxCollider(collider);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Select All Box Colliders"))
        {
            SelectAllBoxColliders();
        }
    }

    private void FindBoxColliders()
    {
        boxColliders.Clear();
        BoxCollider[] colliders = selectedObject.GetComponentsInChildren<BoxCollider>();
        foreach (BoxCollider collider in colliders)
        {
            boxColliders.Add(collider);
        }
    }

    private void ResetBoxCollider(BoxCollider collider)
    {
        Undo.RecordObject(collider, "Reset Box Collider");
        collider.center = Vector3.zero;
        collider.size = Vector3.one;
        EditorUtility.SetDirty(collider);
        Debug.Log("Box Collider reset.");
    }

    private void SelectAllBoxColliders()
    {
        List<Object> selectedObjects = new List<Object>();
        foreach (BoxCollider collider in boxColliders)
        {
            selectedObjects.Add(collider.gameObject);
        }
        Selection.objects = selectedObjects.ToArray();
    }
}
