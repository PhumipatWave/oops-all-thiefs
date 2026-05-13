using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class StartupSceneLoader
{
    private const string MenuPath = "Tools/StartupSceneLoader/Enable";
    private const string PrefKey = "StartupSceneLoaderEnabled";

    static StartupSceneLoader()
    {
        EditorApplication.delayCall += UpdateMenuCheckmark;
        EditorApplication.playModeStateChanged += LoadStartupScene;
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        bool isEnabled = EditorPrefs.GetBool(PrefKey, true);
        EditorPrefs.SetBool(PrefKey, !isEnabled);
        UpdateMenuCheckmark();
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleValidate()
    {
        UpdateMenuCheckmark();
        return true;
    }

    private static void UpdateMenuCheckmark()
    {
        bool isEnabled = EditorPrefs.GetBool(PrefKey, true);
        Menu.SetChecked(MenuPath, isEnabled);

    }

    private static void LoadStartupScene(PlayModeStateChange state)
    {
        if (!EditorPrefs.GetBool(PrefKey, true))
            return;

        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorSceneManager.LoadScene(0);
        }
    }
}
