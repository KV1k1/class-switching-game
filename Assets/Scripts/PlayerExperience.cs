using UnityEngine;
using UnityEngine.SceneManagement;

public enum RunPassive
{
    Vitality,
    Fleetfoot,
    Siphon,
    Bounty
}

public class PlayerExperience : MonoBehaviour
{
    public int currentLevel = 1;
    public int currentExperience;
    public int experienceToNextLevel = 50;
    [Range(1f, 3f)] public float levelRequirementMultiplier = 1.35f;
    [Range(1f, 3f)] public float experienceGainMultiplier = 1f;

    public int PendingLevelChoices { get; private set; }

    PlayerHealth playerHealth;
    PlayerMovement movement;
    int startingLevel;
    int startingExperienceToNextLevel;
    float startingExperienceGainMultiplier;
    float startingMaxHealth;
    float startingHealOnKill;
    float startingMovementSpeed;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        movement = GetComponent<PlayerMovement>();
        startingLevel = currentLevel;
        startingExperienceToNextLevel = experienceToNextLevel;
        startingExperienceGainMultiplier = experienceGainMultiplier;
        startingMaxHealth = playerHealth != null ? playerHealth.maxHealth : 0f;
        startingHealOnKill = playerHealth != null ? playerHealth.healOnKill : 0f;
        startingMovementSpeed = movement != null ? movement.speed : 0f;
    }

    void Start()
    {
        if (IsExpedition) ResetForRun();
    }

    static bool IsExpedition => SceneManager.GetActiveScene().name == "SampleScene";

    void OnEnable()
    {
        Health.AnyDied += OnHealthDied;
    }

    void OnDisable()
    {
        Health.AnyDied -= OnHealthDied;
    }

    void OnHealthDied(Health deadHealth)
    {
        if (!IsExpedition || deadHealth == null || deadHealth is PlayerHealth) return;
        if (deadHealth.experienceReward > 0) GainExperience(deadHealth.experienceReward);
        GameProgression.AwardRunMasteryResource(deadHealth.masteryResourceReward);
    }

    public void ResetForRun()
    {
        currentLevel = startingLevel;
        currentExperience = 0;
        experienceToNextLevel = startingExperienceToNextLevel;
        experienceGainMultiplier = startingExperienceGainMultiplier;
        PendingLevelChoices = 0;

        if (playerHealth != null)
        {
            playerHealth.maxHealth = startingMaxHealth;
            playerHealth.healOnKill = startingHealOnKill;
            playerHealth.Heal(startingMaxHealth);
        }

        if (movement != null) movement.speed = startingMovementSpeed;
    }

    public void GainExperience(int amount)
    {
        if (!IsExpedition || amount <= 0) return;

        currentExperience += Mathf.Max(1, Mathf.RoundToInt(amount * experienceGainMultiplier));
        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            currentLevel++;
            PendingLevelChoices++;
            experienceToNextLevel = Mathf.CeilToInt(experienceToNextLevel * levelRequirementMultiplier);
        }
    }

    public void ChoosePassive(RunPassive passive)
    {
        if (!IsExpedition) return;

        switch (passive)
        {
            case RunPassive.Vitality:
                playerHealth.maxHealth += 20f;
                playerHealth.Heal(20f);
                break;
            case RunPassive.Fleetfoot:
                movement.speed += 0.75f;
                break;
            case RunPassive.Siphon:
                playerHealth.healOnKill += 2f;
                break;
            case RunPassive.Bounty:
                experienceGainMultiplier += 0.2f;
                break;
        }

        PendingLevelChoices = Mathf.Max(0, PendingLevelChoices - 1);
    }
}
