using System.Collections.Generic;
using System.Linq;
using Core.Economy;
using TowerDefense.Agents;
using TowerDefense.Economy;
using TowerDefense.Level;
using TowerDefense.Towers;
using TowerDefense.Towers.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CampaignLevelBootstrap : MonoBehaviour
{
    static CampaignLevelBootstrap s_Instance;

    LevelManager m_LevelManager;
    Level1DifficultyConfig m_DifficultyConfig;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == Level1GameSession.MenuSceneName || sceneName == Level1GameSession.LevelSceneName)
        {
            return;
        }

        if (Object.FindObjectOfType<LevelManager>() == null)
        {
            return;
        }

        if (Object.FindObjectOfType<CampaignLevelBootstrap>() != null)
        {
            return;
        }

        new GameObject("CampaignLevelBootstrap").AddComponent<CampaignLevelBootstrap>();
    }

    void Awake()
    {
        s_Instance = this;
        string sceneName = SceneManager.GetActiveScene().name;
        if (!Level1GameSession.ConsumeLevelStartRequest(sceneName))
        {
            SceneManager.LoadScene(Level1GameSession.MenuSceneName);
            return;
        }

        if (!CampaignProgression.IsSceneUnlocked(sceneName))
        {
            SceneManager.LoadScene(Level1GameSession.MenuSceneName);
            return;
        }

        m_LevelManager = FindObjectOfType<LevelManager>();
        if (m_LevelManager == null)
        {
            return;
        }

        m_DifficultyConfig = Level1GameSession.currentConfig;
        ApplyDifficultyToLevel();
        Debug.Log(
            "[CampaignLevelBootstrap] " + sceneName + " running at " + m_DifficultyConfig.displayName + ".",
            this);
    }

    void OnDestroy()
    {
        if (s_Instance == this)
        {
            s_Instance = null;
        }
    }

    public static void ApplyDifficultyToSpawnedAgent(Agent agent)
    {
        if (s_Instance == null || agent == null)
        {
            return;
        }

        s_Instance.ConfigureAgent(agent);
    }

    void ApplyDifficultyToLevel()
    {
        ScaleCurrency();
        ScaleHomeBases();
        FilterTowerLibrary();
        ScaleWaves();

        Agent[] existingAgents = FindObjectsOfType<Agent>();
        foreach (var agent in existingAgents)
        {
            ConfigureAgent(agent);
        }
    }

    void ScaleCurrency()
    {
        if (m_LevelManager.currency == null)
        {
            return;
        }

        int scaledCurrency = Mathf.RoundToInt(m_LevelManager.startingCurrency * GetTemplateCurrencyMultiplier());
        m_LevelManager.currency.SetCurrency(Mathf.Max(0, scaledCurrency));
    }

    void ScaleHomeBases()
    {
        if (m_LevelManager.homeBases == null)
        {
            return;
        }

        float multiplier = GetTemplateHomeBaseHealthMultiplier();
        foreach (var homeBase in m_LevelManager.homeBases)
        {
            if (homeBase == null || homeBase.configuration == null)
            {
                continue;
            }

            float scaledHealth = Mathf.Max(1f, homeBase.configuration.maxHealth * multiplier);
            homeBase.configuration.SetMaxHealth(scaledHealth, scaledHealth);
            homeBase.configuration.SetHealth(scaledHealth);
        }
    }

    void FilterTowerLibrary()
    {
        if (m_LevelManager.towerLibrary == null)
        {
            return;
        }

        var clonedLibrary = ScriptableObject.Instantiate(m_LevelManager.towerLibrary);
        var sourceList = m_LevelManager.towerLibrary.configurations ?? new List<Tower>();
        var clonedList = new List<Tower>(sourceList);

        if (m_DifficultyConfig.difficulty == Level1Difficulty.Epic && clonedList.Count > 2)
        {
            clonedList = clonedList
                .Where(tower => tower != null)
                .OrderBy(tower => tower.purchaseCost)
                .Take(2)
                .ToList();
        }

        clonedLibrary.configurations = clonedList;
        clonedLibrary.OnAfterDeserialize();
        m_LevelManager.towerLibrary = clonedLibrary;
    }

    void ScaleWaves()
    {
        if (m_LevelManager.waveManager == null || m_LevelManager.waveManager.waves == null)
        {
            return;
        }

        foreach (Wave wave in m_LevelManager.waveManager.waves)
        {
            if (wave == null || wave.spawnInstructions == null || wave.spawnInstructions.Count == 0)
            {
                continue;
            }

            wave.spawnInstructions = ScaleSpawnInstructions(wave.spawnInstructions);
        }
    }

    List<SpawnInstruction> ScaleSpawnInstructions(List<SpawnInstruction> originalInstructions)
    {
        var scaledInstructions = new List<SpawnInstruction>();
        if (originalInstructions == null || originalInstructions.Count == 0)
        {
            return scaledInstructions;
        }

        float accumulator = 0f;
        float countMultiplier = m_DifficultyConfig.enemyCountMultiplier;
        float delayMultiplier = m_DifficultyConfig.spawnIntervalMultiplier;

        foreach (var instruction in originalInstructions)
        {
            accumulator += countMultiplier;
            int copies = Mathf.FloorToInt(accumulator);
            accumulator -= copies;

            for (int copyIndex = 0; copyIndex < copies; copyIndex++)
            {
                float scaledDelay = instruction.delayToSpawn * delayMultiplier;
                if (copyIndex > 0)
                {
                    scaledDelay = Mathf.Max(0.08f, scaledDelay * 0.35f);
                }

                scaledInstructions.Add(new SpawnInstruction
                {
                    agentConfiguration = instruction.agentConfiguration,
                    delayToSpawn = scaledDelay,
                    startingNode = instruction.startingNode
                });
            }
        }

        if (scaledInstructions.Count == 0)
        {
            SpawnInstruction first = originalInstructions[0];
            scaledInstructions.Add(new SpawnInstruction
            {
                agentConfiguration = first.agentConfiguration,
                delayToSpawn = Mathf.Max(0.08f, first.delayToSpawn * delayMultiplier),
                startingNode = first.startingNode
            });
        }

        return scaledInstructions;
    }

    void ConfigureAgent(Agent agent)
    {
        if (agent == null || agent.configuration == null)
        {
            return;
        }

        var state = agent.GetComponent<CampaignDifficultyAgentState>();
        if (state == null)
        {
            state = agent.gameObject.AddComponent<CampaignDifficultyAgentState>();
        }

        if (!state.hasBaseValues)
        {
            state.baseMaxHealth = agent.configuration.maxHealth;
            state.baseMoveSpeed = agent.navMeshNavMeshAgent != null ? agent.navMeshNavMeshAgent.speed : 0f;

            var baseLoot = agent.GetComponent<LootDrop>();
            state.baseLoot = baseLoot == null ? 0 : baseLoot.lootDropped;
            state.hasBaseValues = true;
        }

        float scaledHealth = Mathf.Max(1f, state.baseMaxHealth * m_DifficultyConfig.enemyHealthMultiplier);
        agent.configuration.SetMaxHealth(scaledHealth, scaledHealth);
        agent.configuration.SetHealth(scaledHealth);

        if (agent.navMeshNavMeshAgent != null)
        {
            agent.navMeshNavMeshAgent.speed = state.baseMoveSpeed * m_DifficultyConfig.enemySpeedMultiplier;
        }

        var lootDrop = agent.GetComponent<LootDrop>();
        if (lootDrop != null)
        {
            int scaledLoot = Mathf.RoundToInt(state.baseLoot * m_DifficultyConfig.rewardMultiplier);
            lootDrop.lootDropped = Mathf.Max(1, scaledLoot);
        }
    }

    float GetTemplateCurrencyMultiplier()
    {
        switch (m_DifficultyConfig.difficulty)
        {
            case Level1Difficulty.Easy:
                return 1.15f;
            case Level1Difficulty.Hard:
                return 0.9f;
            case Level1Difficulty.Epic:
                return 0.75f;
            default:
                return 1f;
        }
    }

    float GetTemplateHomeBaseHealthMultiplier()
    {
        switch (m_DifficultyConfig.difficulty)
        {
            case Level1Difficulty.Easy:
                return 1.15f;
            case Level1Difficulty.Hard:
                return 0.92f;
            case Level1Difficulty.Epic:
                return 0.82f;
            default:
                return 1f;
        }
    }
}
