using Core.Health;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleEnemyGoal : MonoBehaviour
{
    [SerializeField] SimpleHomeBaseHealth homeBase;
    [SerializeField, Min(1)] int damagePerEnemy = 1;

    Collider m_Collider;

    public void Configure(SimpleHomeBaseHealth targetBase, int enemyDamage)
    {
        homeBase = targetBase;
        damagePerEnemy = Mathf.Max(1, enemyDamage);
        EnsureTrigger();
    }

    void Awake()
    {
        EnsureTrigger();
    }

    void OnTriggerEnter(Collider other)
    {
        var mover = other.GetComponentInParent<NPCMover>();
        if (mover == null)
        {
            return;
        }

        var damageable = mover.GetComponent<DamageableBehaviour>();
        if (damageable != null && damageable.isDead)
        {
            return;
        }

        int incomingDamage = damagePerEnemy;
        var targetable = mover.GetComponent<SimpleEnemyTargetable>();
        if (targetable != null)
        {
            incomingDamage = Mathf.Max(1, targetable.goalDamage);
        }

        if (homeBase != null)
        {
            homeBase.ApplyDamage(incomingDamage);
        }

        if (damageable != null)
        {
            damageable.Remove();
        }
        else
        {
            Destroy(mover.gameObject);
        }
    }

    void EnsureTrigger()
    {
        if (m_Collider == null)
        {
            m_Collider = GetComponent<Collider>();
        }

        if (m_Collider != null)
        {
            m_Collider.isTrigger = true;
        }
    }
}
