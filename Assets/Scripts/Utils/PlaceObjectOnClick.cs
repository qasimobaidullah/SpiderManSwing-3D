using UnityEditor;
using UnityEngine;

public class PlaceObjectOnClick : EditorWindow
{
    private GameObject _prefab;
    private Vector3 _spawnPosition;
    private bool _objectSpawned = false;

    [MenuItem("LadyBug/Click to Place Object")]
    public static void ShowWindow()
    {
        GetWindow(typeof(PlaceObjectOnClick));
    }

    private void OnGUI()
    {
        _prefab =
            EditorGUILayout.ObjectField("Prefab to Place:", _prefab, typeof(GameObject), false)
            as GameObject;
        EditorGUILayout.Space(10);

        if (!_objectSpawned)
        {
            if (GUILayout.Button("Activate Click to Place"))
            {
                SceneView.duringSceneGui += OnSceneGUI;
                _objectSpawned = true;
            }
        }
        else
        {
            if (GUILayout.Button("Deactivate Click to Place"))
            {
                SceneView.duringSceneGui -= OnSceneGUI;
                _objectSpawned = false;
            }
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        _objectSpawned = false;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_prefab == null)
        {
            Debug.LogWarning("Please select a prefab to place");
            return;
        }

        // Get the mouse position in the scene view
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;

        // If the mouse is clicked and the raycast hits a surface, spawn the prefab at the hit point
        if (e.type == EventType.MouseDown && e.button == 0 && Physics.Raycast(ray, out hit))
        {
            _spawnPosition = hit.point;
            GameObject newObject =
                PrefabUtility.InstantiatePrefab(_prefab, hit.transform.parent) as GameObject;
            newObject.transform.position = _spawnPosition;
            Debug.Log("Spawned " + newObject.name + " at " + _spawnPosition);
        }
    }
}
