using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
static class ProjectStartupScene
{
    const string SessionKey = "ProjectStartupScene.OpenedMainMenu";
    const string TargetScenePath = "Assets/Game/Scenes/MainMenu.unity";

    static ProjectStartupScene()
    {
        EditorApplication.delayCall += OpenMainMenuOnStartup;
    }

    static void OpenMainMenuOnStartup()
    {
        if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (SessionState.GetBool(SessionKey, false))
        {
            return;
        }

        SessionState.SetBool(SessionKey, true);

        if (EditorSceneManager.GetActiveScene().path == TargetScenePath)
        {
            return;
        }

        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScenePath);
        if (sceneAsset == null)
        {
            Debug.LogWarning("[ProjectStartupScene] Could not find MainMenu scene at " + TargetScenePath + ".");
            return;
        }

        EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
    }
}
