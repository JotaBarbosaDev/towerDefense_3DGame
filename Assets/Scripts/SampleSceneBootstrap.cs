using System.Collections;
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

        if ((enemyArchetypes == null || enemyArchetypes.Length == 0) &&
            buggyVisualPrefab != null &&
            tankVisualPrefab != null &&
            helicopterVisualPrefab != null &&
            bossVisualPrefab != null)
        {
            enemyArchetypes = BuildDefaultEnemyArchetypes();
        }
    }

    void Start()
    {
        EnsureDefaultReferences();
        ClearExistingEnemies();
        ConfigureGoal();
        StartCoroutine(SpawnLevelOneWaves());
        Debug.Log("[SampleSceneBootstrap] Sample scene defense systems ready with 2 waves.", this);
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

    IEnumerator SpawnLevelOneWaves()
    {
        yield return new WaitForSeconds(Mathf.Max(waveStartDelay, 8f));
        yield return StartCoroutine(SpawnWave(BuildWaveOneArchetypes()));
        yield return new WaitForSeconds(9f);
        yield return StartCoroutine(SpawnWave(BuildWaveTwoArchetypes()));
    }

    IEnumerator SpawnWave(SimpleEnemyArchetype[] archetypes)
    {
        if (archetypes == null || archetypes.Length == 0)
        {
            yield break;
        }

        foreach (var archetype in archetypes)
        {
            if (archetype.visualPrefab == null || archetype.count <= 0)
            {
                continue;
            }

            for (int i = 0; i < archetype.count; i++)
            {
                SpawnEnemy(archetype, i + 1);
                yield return new WaitForSeconds(archetype.spawnInterval);
            }
        }
    }

    void SpawnEnemy(SimpleEnemyArchetype archetype, int index)
    {
        GameObject startObject = GameObject.Find("START");
        GameObject endObject = GameObject.Find(goalObjectName);
        if (startObject == null || endObject == null)
        {
            return;
        }

        Vector3 spawnPosition = startObject.transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = startObject.transform.position.y;

        GameObject enemyObject = new GameObject(archetype.displayName + "_" + index);
        enemyObject.transform.position = spawnPosition;
        enemyObject.transform.rotation = startObject.transform.rotation;

        var mover = enemyObject.AddComponent<NPCMover>();
        mover.destino = endObject.transform;

        var agent = enemyObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = archetype.moveSpeed;
        agent.angularSpeed = 120f;
        agent.acceleration = 12f;
        agent.radius = 0.6f;
        agent.height = 1.8f;
        agent.baseOffset = 0f;

        var enemy = enemyObject.AddComponent<SimpleEnemyTargetable>();
        enemy.Configure(
            archetype.health,
            archetype.movementKind,
            archetype.goalDamage,
            archetype.currencyReward);

        GameObject visual = Instantiate(archetype.visualPrefab, enemyObject.transform);
        DisableVisualBehaviours(visual);
        visual.transform.localPosition = Vector3.zero;
        NormalizeVisualScale(visual, archetype.visualScale);
        LiftVisualToGround(visual, enemyObject.transform.position.y);
        visual.transform.localPosition += archetype.visualOffset;

        if (archetype.movementKind == EnemyMovementKind.Air)
        {
            visual.transform.localPosition += Vector3.up * 1.1f;
        }

        enemy.FitColliderToVisual(GetRenderableBounds(visual));
        ApplyVisualTint(visual, archetype.visualTint);
        AttachHealthBar(enemy, visual);
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

    SimpleEnemyArchetype[] BuildDefaultEnemyArchetypes()
    {
        return CombineArchetypeArrays(BuildWaveOneArchetypes(), BuildWaveTwoArchetypes());
    }

    SimpleEnemyArchetype[] CombineArchetypeArrays(SimpleEnemyArchetype[] first, SimpleEnemyArchetype[] second)
    {
        int firstCount = first == null ? 0 : first.Length;
        int secondCount = second == null ? 0 : second.Length;
        var combined = new SimpleEnemyArchetype[firstCount + secondCount];

        for (int i = 0; i < firstCount; i++)
        {
            combined[i] = first[i];
        }

        for (int i = 0; i < secondCount; i++)
        {
            combined[firstCount + i] = second[i];
        }

        return combined;
    }

    SimpleEnemyArchetype[] BuildWaveOneArchetypes()
    {
        return new[]
        {
            new SimpleEnemyArchetype
            {
                displayName = "Hoverbuggy_Scout",
                visualPrefab = buggyVisualPrefab,
                movementKind = EnemyMovementKind.Ground,
                count = 5,
                health = 6f,
                moveSpeed = 4.8f,
                goalDamage = 1,
                currencyReward = 25,
                spawnInterval = 0.5f,
                visualScale = Vector3.one * 2f,
                visualTint = new Color(1f, 0.95f, 0.85f)
            },
            new SimpleEnemyArchetype
            {
                displayName = "Hoverbuggy_Striker",
                visualPrefab = buggyVisualPrefab,
                movementKind = EnemyMovementKind.Ground,
                count = 2,
                health = 12f,
                moveSpeed = 5.1f,
                goalDamage = 2,
                currencyReward = 40,
                spawnInterval = 0.7f,
                visualScale = Vector3.one * 2.15f,
                visualTint = new Color(1f, 0.72f, 0.38f)
            },
            new SimpleEnemyArchetype
            {
                displayName = "Hovertank_Bruiser",
                visualPrefab = tankVisualPrefab,
                movementKind = EnemyMovementKind.Ground,
                count = 1,
                health = 24f,
                moveSpeed = 2f,
                goalDamage = 2,
                currencyReward = 65,
                spawnInterval = 1.35f,
                visualScale = Vector3.one * 1.8f,
                visualTint = new Color(0.78f, 0.86f, 0.9f)
            },
            new SimpleEnemyArchetype
            {
                displayName = "Hovercopter_Raider",
                visualPrefab = helicopterVisualPrefab,
                movementKind = EnemyMovementKind.Air,
                count = 2,
                health = 14f,
                moveSpeed = 3.2f,
                goalDamage = 1,
                currencyReward = 55,
                spawnInterval = 1f,
                visualScale = Vector3.one * 1.55f,
                visualTint = new Color(0.82f, 0.94f, 1f)
            }
        };
    }

    SimpleEnemyArchetype[] BuildWaveTwoArchetypes()
    {
        return new[]
        {
            new SimpleEnemyArchetype
            {
                displayName = "Hoverbuggy_Scout",
                visualPrefab = buggyVisualPrefab,
                movementKind = EnemyMovementKind.Ground,
                count = 4,
                health = 7f,
                moveSpeed = 5f,
                goalDamage = 1,
                currencyReward = 30,
                spawnInterval = 0.45f,
                visualScale = Vector3.one * 2f,
                visualTint = new Color(1f, 0.95f, 0.85f)
            },
            new SimpleEnemyArchetype
            {
                displayName = "Hovertank_Bruiser",
                visualPrefab = tankVisualPrefab,
                movementKind = EnemyMovementKind.Ground,
                count = 2,
                health = 30f,
                moveSpeed = 2.1f,
                goalDamage = 2,
                currencyReward = 75,
                spawnInterval = 1.15f,
                visualScale = Vector3.one * 1.9f,
                visualTint = new Color(0.78f, 0.86f, 0.9f)
            },
            new SimpleEnemyArchetype
            {
                displayName = "Hovercopter_Raider",
                visualPrefab = helicopterVisualPrefab,
                movementKind = EnemyMovementKind.Air,
                count = 3,
                health = 18f,
                moveSpeed = 3.3f,
                goalDamage = 1,
                currencyReward = 65,
                spawnInterval = 0.85f,
                visualScale = Vector3.one * 1.6f,
                visualTint = new Color(0.82f, 0.94f, 1f)
            },
            new SimpleEnemyArchetype
            {
                displayName = "Hovercopter_Gunship",
                visualPrefab = helicopterVisualPrefab,
                movementKind = EnemyMovementKind.Air,
                count = 2,
                health = 26f,
                moveSpeed = 2.8f,
                goalDamage = 2,
                currencyReward = 90,
                spawnInterval = 1.05f,
                visualScale = Vector3.one * 1.75f,
                visualTint = new Color(1f, 0.88f, 0.55f)
            },
            new SimpleEnemyArchetype
            {
                displayName = "Hoverboss_Siege",
                visualPrefab = bossVisualPrefab,
                movementKind = EnemyMovementKind.Ground,
                count = 1,
                health = 90f,
                moveSpeed = 1.2f,
                goalDamage = 4,
                currencyReward = 180,
                spawnInterval = 1.9f,
                visualScale = Vector3.one * 2.4f,
                visualTint = new Color(0.95f, 0.62f, 0.48f)
            }
        };
    }
}
