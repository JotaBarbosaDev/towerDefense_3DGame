using ActionGameFramework.Health;
using Core.Health;
using UnityEngine;

public class SimpleEnemyTargetable : Targetable
{
    [Min(1)] public float maxHealth = 12f;
    [Min(1)] public int goalDamage = 1;
    [Min(0)] public int currencyReward = 1;
    [SerializeField] CapsuleCollider hitCollider;
    bool m_IsInitialized;
    bool m_RewardGranted;

    protected override void Awake()
    {
        if (configuration == null)
        {
            configuration = new Damageable();
        }

        EnsureCollider();
        ApplyHealthConfiguration();
        base.Awake();
        died += OnDied;
        m_IsInitialized = true;
    }

    public override void Remove()
    {
        base.Remove();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        died -= OnDied;
    }

    public void Configure(float health, EnemyMovementKind movementKind, int enemyGoalDamage, int enemyCurrencyReward)
    {
        maxHealth = Mathf.Max(1f, health);
        goalDamage = Mathf.Max(1, enemyGoalDamage);
        currencyReward = Mathf.Max(0, enemyCurrencyReward);
        EnsureCollider();
        gameObject.layer = movementKind == EnemyMovementKind.Air ? 14 : 11;

        if (m_IsInitialized)
        {
            ApplyHealthConfiguration();
            configuration.SetHealth(maxHealth);
        }
    }

    public void FitColliderToVisual(Bounds visualBounds)
    {
        EnsureCollider();

        float radius = Mathf.Clamp(Mathf.Max(visualBounds.extents.x, visualBounds.extents.z) * 0.45f, 0.45f, 1.5f);
        float height = Mathf.Clamp(visualBounds.size.y, 1.2f, 4.8f);
        Vector3 localCenter = transform.InverseTransformPoint(visualBounds.center);

        hitCollider.radius = radius;
        hitCollider.height = Mathf.Max(height, radius * 2f);
        hitCollider.center = new Vector3(localCenter.x, localCenter.y, localCenter.z);
    }

    void Reset()
    {
        EnsureCollider();
    }

    void EnsureCollider()
    {
        if (hitCollider == null)
        {
            hitCollider = GetComponent<CapsuleCollider>();
        }

        if (hitCollider == null)
        {
            hitCollider = gameObject.AddComponent<CapsuleCollider>();
        }

        hitCollider.isTrigger = false;
        hitCollider.center = new Vector3(0f, 0.8f, 0f);
        hitCollider.height = 1.8f;
        hitCollider.radius = 0.7f;
    }

    void ApplyHealthConfiguration()
    {
        configuration.SetMaxHealth(maxHealth, maxHealth);
        configuration.alignment = null;
    }

    void OnDied(DamageableBehaviour damageable)
    {
        if (m_RewardGranted || currencyReward <= 0)
        {
            return;
        }

        m_RewardGranted = true;

        if (SimpleCurrencyManager.instance != null)
        {
            SimpleCurrencyManager.instance.AddCurrency(currencyReward);
        }
    }
}
