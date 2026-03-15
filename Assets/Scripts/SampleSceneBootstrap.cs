using Core.Health;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SampleSceneBootstrap : MonoBehaviour
{
    const string HealthBarPrefabPath =
        "Assets/UnityTechnologies/TowerDefenseTemplate/Prefabs/UI/HealthBar.prefab";
    const string BuggyVisualPrefabPath =
        "Assets/UnityTechnologies/TowerDefenseTemplate/Models/Units/Buggy.fbx";
    const string TankVisualPrefabPath =
        "Assets/UnityTechnologies/TowerDefenseTemplate/Models/Units/HoverTank_Base.fbx";
    const string HelicopterVisualPrefabPath =
        "Assets/UnityTechnologies/TowerDefenseTemplate/Models/Units/Helicopter.fbx";
    const string BossVisualPrefabPath =
        "Assets/UnityTechnologies/TowerDefenseTemplate/Models/Units/BossTank_Base.fbx";

    public string homeBaseObjectName = "HeadQuarters";
    public string goalObjectName = "END";
    [Min(1)] public int homeBaseHealth = 10;
    [Min(1)] public int damagePerEnemy = 1;
    [Min(0)] public int startingCurrency = 700;
    public HealthVisualizer healthBarPrefab;
    [Min(0f)] public float waveStartDelay = 1.5f;
    [Min(0f)] public float spawnRadius = 0.75f;
    public GameObject buggyVisualPrefab;
    public GameObject tankVisualPrefab;
    public GameObject helicopterVisualPrefab;
    public GameObject bossVisualPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        if (FindObjectOfType<SampleSceneBootstrap>() != null)
        {
            return;
        }

        if (GameObject.Find("START") == null ||
            GameObject.Find("END") == null ||
            GameObject.Find("HeadQuarters") == null)
        {
            return;
        }

        var bootstrapObject = new GameObject("SampleSceneBootstrap_Auto");
        bootstrapObject.AddComponent<SampleSceneBootstrap>();
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        EnsureDefaultReferences();
    }

    void Start()
    {
        EnsureDefaultReferences();
        Debug.Log("[SampleSceneBootstrap] Sample scene entrypoint active.", this);
    }

    void EnsureDefaultReferences()
    {
#if UNITY_EDITOR
        if (healthBarPrefab == null)
        {
            healthBarPrefab = LoadPrefabComponent<HealthVisualizer>(HealthBarPrefabPath);
        }

        if (buggyVisualPrefab == null)
        {
            buggyVisualPrefab = LoadPrefabAsset(BuggyVisualPrefabPath);
        }

        if (tankVisualPrefab == null)
        {
            tankVisualPrefab = LoadPrefabAsset(TankVisualPrefabPath);
        }

        if (helicopterVisualPrefab == null)
        {
            helicopterVisualPrefab = LoadPrefabAsset(HelicopterVisualPrefabPath);
        }

        if (bossVisualPrefab == null)
        {
            bossVisualPrefab = LoadPrefabAsset(BossVisualPrefabPath);
        }
#endif
    }

#if UNITY_EDITOR
    static GameObject LoadPrefabAsset(string assetPath)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
    }

    static T LoadPrefabComponent<T>(string assetPath) where T : Component
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            return null;
        }

        return prefab.GetComponent<T>();
    }
#endif
}
