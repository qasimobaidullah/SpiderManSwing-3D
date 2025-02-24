#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Diagnostics;
using UnityEditor.SceneManagement;
using System.Collections;

public class LadyBugUtility : EditorWindow, IPreprocessBuildWithReport
{
    static Component[] copiedComponents;
    static string TagX;

    [MenuItem("LadyBug/Copy all components %&C")] //Control+alt+c
    static void Copy()
    {
        copiedComponents = Selection.activeGameObject.GetComponents<Component>();
        TagX = Selection.activeGameObject.tag;
        UnityEngine.Debug.Log(Selection.activeGameObject.GetComponents<Component>());
        UnityEngine.Debug.Log($"GameObject Component Copied");
    }

    [MenuItem("LadyBug/Paste all components %&V")] //Control+alt+v
    static void Paste()
    {
        foreach (var targetGameObject in Selection.gameObjects)
        {
            if (!targetGameObject || copiedComponents == null)
                continue;
            foreach (var copiedComponent in copiedComponents)
            {
                if (!copiedComponent)
                    continue;
                UnityEditorInternal.ComponentUtility.CopyComponent(copiedComponent);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetGameObject);
                targetGameObject.tag = TagX;
            }
        }
    }

    [MenuItem("LadyBug/Remove all missing scripts %&F12")] //Control+alt+F12
    public static void Remove()
    {
        var objs = Resources.FindObjectsOfTypeAll<GameObject>();
        int count = objs.Sum(GameObjectUtility.RemoveMonoBehavioursWithMissingScript);
        UnityEngine.Debug.Log($"Removed {count} missing scripts");
    }

    /// <summary>
    /// Delete Prefs Editor Script
    /// </summary>
    [MenuItem("LadyBug/Delete Prefs")]
    public static void DeletePrefs()
    {
        PlayerPrefs.DeleteAll();
        UnityEngine.Debug.Log("All Prefs Cleared");
    }

    /// <summary>
    /// Tracks scroll position.
    /// </summary>

    private Vector2 scrollPos;

    /// Initialize window state.
    /// </summary>
    // [MenuItem("LadyBug/Scene View")]
    internal static void Init()
    {
        // EditorWindow.GetWindow() will return the open instance of the specified window or create a new
        // instance if it can't find one. The second parameter is a flag for creating the window as a
        // Utility window; Utility windows cannot be docked like the Scene and Game view windows.
        var window = (LadyBugUtility)GetWindow(typeof(LadyBugUtility), false, "Scene View");
        window.position = new Rect(
            window.position.xMin + 100f,
            window.position.yMin + 100f,
            400f,
            400f
        );
    }

    /// <summary>
    /// Called on GUI events.
    /// </summary>
    internal void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, false, false);

        GUILayout.Label("Scenes In Build", EditorStyles.boldLabel);
        for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            var scene = EditorBuildSettings.scenes[i];
            if (scene.enabled)
            {
                var sceneName = Path.GetFileNameWithoutExtension(scene.path);
                var pressed = GUILayout.Button(
                    i + ": " + sceneName,
                    new GUIStyle(GUI.skin.GetStyle("Button")) { alignment = TextAnchor.MiddleLeft }
                );
                if (pressed)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scene.path);
                    }
                }
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// --------Warning On Build
    /// </summary>
    /// <param name="report"></param>
    ///
    public int callbackOrder
    {
        get { return 0; }
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (EditorUserBuildSettings.buildAppBundle)
        {
            EditorUtility.DisplayDialog(
                "Warning...!",
                "BuildAppBundle Mode is on make sure you are making android App bundle",
                "Continue"
            );
        }
    }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        if (EditorUserBuildSettings.buildAppBundle)
        {
            EditorUtility.DisplayDialog(
                "Warning...!",
                "BuildAppBundle Mode is on make sure you are making android App bundle",
                "Continue"
            );
        }
    }

    [MenuItem("LadyBug/PS-Export")]
    static void Export()
    {
        //		AssetDatabase.ExportPackage (AssetDatabase.GetAllAssetPaths(),PlayerSettings.productName + ".unitypackage",ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets);

        string[] projectContent = new string[]
        {
            "ProjectSettings/TagManager.asset",
            "ProjectSettings/InputManager.asset",
        };
        AssetDatabase.ExportPackage(
            projectContent,
            "Game_ProjectSettings.unitypackage",
            ExportPackageOptions.Interactive
                | ExportPackageOptions.Recurse
                | ExportPackageOptions.IncludeDependencies
        );
        UnityEngine.Debug.Log("Project Exported");
    }

    //[MenuItem("LadyBug/Turn-Off-Tutorial")]
    static void TurnOffTutorial()
    {
        PlayerPrefs.SetInt("isTutorial", 1);
        UnityEngine.Debug.Log("Tutorial Deactivated");
    }
}

#endif
