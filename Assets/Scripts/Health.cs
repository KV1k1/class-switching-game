using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 50f;
    public bool destroyOnDeath = true;

    public float Current { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<float> Damaged;          // sends the damage amount
    public static event Action<Health> AnyDied;  // fires whenever anything dies

    protected virtual void Awake()
    {
        Current = maxHealth;
        if (!(this is PlayerHealth) && GetComponent<HitFeedback>() == null)   // NEW
            gameObject.AddComponent<HitFeedback>();                           // NEW
    }

    public virtual void TakeDamage(float amount)
    {
        if (IsDead) return;

        Current -= amount;
        Damaged?.Invoke(amount);

        if (Current <= 0f)
        {
            Current = 0f;
            IsDead = true;
            AnyDied?.Invoke(this);
            if (destroyOnDeath) Destroy(gameObject);
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        Current = Mathf.Min(maxHealth, Current + amount);
    }
}