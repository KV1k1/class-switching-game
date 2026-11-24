using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : Health
{
    public float healOnKill = 5f;
    public float invulnerableTime = 0.5f;   // brief protection after each hit

    float invulnerableUntil;
    PlayerMovement movement;
    Animator animator;

    protected override void Awake()
    {
        base.Awake();
        destroyOnDeath = false;   // the player stays in the scene when dead
        movement = GetComponent<PlayerMovement>();
        animator = GetComponentInChildren<Animator>();
        if (GetComponent<PlayerExperience>() == null)
            gameObject.AddComponent<PlayerExperience>();
    }

    void OnEnable()  { AnyDied += OnSomethingDied; }
    void OnDisable() { AnyDied -= OnSomethingDied; }

    void OnSomethingDied(Health who)
    {
        if (who == this) return;   // we only heal when OTHER things die
        Heal(healOnKill);
    }

    public override void TakeDamage(float amount)
    {
        if (IsDead || Time.time < invulnerableUntil) return;
        invulnerableUntil = Time.time + invulnerableTime;

        base.TakeDamage(amount);

        if (IsDead)
        {
            GetComponent<PlayerLoadout>().enabled = false;
            movement.enabled = false;
            string deathSummary = "Press R to restart";
            if (SceneManager.GetActiveScene().name == "SampleScene")
            {
                ExpeditionRunLog runLog = GameProgression.FinishRunLog(false);
                deathSummary = runLog.ToDisplayString() + "\nRun failed — press R to restart";
            }
            GameUI.Instance.ShowEnd("YOU DIED", deathSummary, new Color(0.8f, 0.1f, 0.1f));
            Time.timeScale = 0f;
        }
        else if (!movement.movementLocked && animator != null &&
                 animator.HasState(0, Animator.StringToHash("Damaged")))
        {
            animator.CrossFadeInFixedTime("Damaged", 0.05f);   // plays only if you've made this state
        }
    }
}
