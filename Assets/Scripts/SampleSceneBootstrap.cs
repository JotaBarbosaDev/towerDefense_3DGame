using Core.Health;
using Core.Utilities;
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
    public SimpleEnemyArchetype[] enemyArchetypes;

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
        ClearExistingEnemies();
        ConfigureGoal();
        Debug.Log("[SampleSceneBootstrap] Sample scene defense systems ready.", this);
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

    void ClearExistingEnemies()
    {
        var movers = FindObjectsOfType<NPCMover>();
        foreach (var mover in movers)
        {
            Destroy(mover.gameObject);
        }
    }

    void ConfigureGoal()
    {
        GameObject homeBaseObject = GameObject.Find(homeBaseObjectName);
        if (homeBaseObject == null)
        {
            Debug.LogError("[SampleSceneBootstrap] Home base object not found: " + homeBaseObjectName, this);
            return;
        }

        GameObject goalObject = GameObject.Find(goalObjectName);
        if (goalObject == null)
        {
            Debug.LogError("[SampleSceneBootstrap] Goal object not found: " + goalObjectName, this);
            return;
        }

        var homeBase = homeBaseObject.GetComponent<SimpleHomeBaseHealth>();
        if (homeBase == null)
        {
            homeBase = homeBaseObject.AddComponent<SimpleHomeBaseHealth>();
        }

        homeBase.maxHealth = Mathf.Max(1, homeBaseHealth);
        homeBase.ResetHealth();

        var goal = goalObject.GetComponent<SimpleEnemyGoal>();
        if (goal == null)
        {
            goal = goalObject.AddComponent<SimpleEnemyGoal>();
        }

        goal.Configure(homeBase, damagePerEnemy);
    }

    void AttachHealthBar(SimpleEnemyTargetable enemy, GameObject visual)
    {
        if (enemy == null || healthBarPrefab == null)
        {
            return;
        }

        var healthBar = enemy.GetComponentInChildren<HealthVisualizer>();
        if (healthBar == null)
        {
            healthBar = Instantiate(healthBarPrefab, enemy.transform);
            healthBar.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        }

        healthBar.damageableBehaviour = enemy;
        healthBar.AssignDamageable(enemy.configuration);
        healthBar.UpdateHealth(enemy.configuration.normalisedHealth);

        Bounds visualBounds = GetRenderableBounds(visual);
        float heightOffset = Mathf.Clamp(visualBounds.max.y - enemy.transform.position.y + 0.35f, 1.6f, 4.5f);
        healthBar.transform.localPosition = new Vector3(0f, heightOffset, 0f);
    }

    void DisableVisualBehaviours(GameObject visual)
    {
        if (visual == null)
        {
            return;
        }

        foreach (var behaviour in visual.GetComponentsInChildren<Behaviour>(true))
        {
            behaviour.enabled = false;
        }

        foreach (var collider in visual.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }
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

    void ApplyVisualTint(GameObject visual, Color tint)
    {
        if (visual == null || tint == Color.white)
        {
            return;
        }

        var renderers = visual.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            var materials = renderer.materials;
            foreach (var material in materials)
            {
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty("_Color"))
                {
                    material.color = tint;
                }

                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", tint);
                }
            }
        }
    }

    void NormalizeVisualScale(GameObject visual, Vector3 desiredSize)
    {
        if (visual == null)
        {
            return;
        }

        Bounds bounds = GetRenderableBounds(visual);
        float currentMajorSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float targetMajorSize = Mathf.Max(desiredSize.x, desiredSize.y, desiredSize.z);
        if (currentMajorSize <= 0.001f || targetMajorSize <= 0.001f)
        {
            visual.transform.localScale = desiredSize;
            return;
        }

        float scaleFactor = targetMajorSize / currentMajorSize;
        visual.transform.localScale = Vector3.one * scaleFactor;
    }

    void LiftVisualToGround(GameObject visual, float groundY)
    {
        if (visual == null)
        {
            return;
        }

        Bounds bounds = GetRenderableBounds(visual);
        float lift = groundY - bounds.min.y;
        visual.transform.position += Vector3.up * lift;
    }

    Bounds GetRenderableBounds(GameObject visual)
    {
        if (visual == null)
        {
            return new Bounds(Vector3.zero, Vector3.one);
        }

        var renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(visual.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
}
