using UnityEngine;

public class SimpleHomeBaseHealth : MonoBehaviour
{
    [Min(1)] public int maxHealth = 10;

    [SerializeField] int m_CurrentHealth;

    public int currentHealth
    {
        get { return m_CurrentHealth; }
    }

    public bool isDestroyed
    {
        get { return m_CurrentHealth <= 0; }
    }

    public void ResetHealth()
    {
        m_CurrentHealth = maxHealth;
    }

    public void ApplyDamage(int damage)
    {
        if (damage <= 0 || isDestroyed)
        {
            return;
        }

        m_CurrentHealth = Mathf.Max(0, m_CurrentHealth - damage);
        Debug.Log("[SimpleHomeBaseHealth] Base health: " + m_CurrentHealth + "/" + maxHealth, this);

        if (isDestroyed)
        {
            Debug.LogWarning("[SimpleHomeBaseHealth] Base destroyed.", this);
        }
    }
}
