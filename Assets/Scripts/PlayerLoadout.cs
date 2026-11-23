using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLoadout : MonoBehaviour
{
    public ClassData[] classes;
    public int maxClassSlots = 3;
    public float switchDelay = 0.3f;
    public Animator modelAnimator;

    [Header("Switch glint")]
    public float flashDuration = 0.5f;
    public float flashIntensity = 2f;

    public bool comboWindowOpen;
    public ClassData LastSwitchedFrom { get; private set; }
    public ClassData LastSwitchedTo { get; private set; }
    bool gunnerJumpComboActive;
    ClassData gunnerJumpSourceClass;
    ClassData gunnerJumpTargetClass;

    int active;
    float nextSwitchTime;
    float[,] cooldownReady;
    List<Material> bodyMaterials = new List<Material>();
    GameObject currentWeapon;
    PlayerMovement movement;
    Coroutine flashRoutine;

    public ClassData CurrentClass => classes[active];

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        cooldownReady = new float[classes.Length, 4];

        foreach (Renderer r in modelAnimator.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.materials)
            {
                if (!m.HasProperty("_EmissionColor")) continue;
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", Color.black);
                bodyMaterials.Add(m);
            }
        }
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "SampleScene")
            GameProgression.BeginRun();
        classes = GameProgression.RestoreEquippedClasses(classes, maxClassSlots);
        cooldownReady = new float[classes.Length, 4];
        ApplyClass(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        // Race/Class Swap: Q
        if (Input.GetKeyDown(KeyCode.Q) && (Time.time >= nextSwitchTime || comboWindowOpen))
        {
            LastSwitchedFrom = CurrentClass;
            active = (active + 1) % classes.Length;
            LastSwitchedTo = CurrentClass;
            if (gunnerJumpComboActive && LastSwitchedFrom == gunnerJumpSourceClass)
            {
                gunnerJumpTargetClass = LastSwitchedTo;
            }
            nextSwitchTime = Time.time + switchDelay;
            comboWindowOpen = false;
            ApplyClass(true);
        }

        // Spells: 1, 2, 3
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryUseSkill(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryUseSkill(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryUseSkill(2); // Ultimate Skill

        // Scheme 1 attacks with right click; scheme 2 attacks with left click.
        int attackButton = InputSchemeSettings.Current == InputScheme.ClickToMove ? 1 : 0;
        if (Input.GetMouseButton(attackButton)) TryUseSkill(3);
    }

    void ApplyClass(bool glint)
    {
        if (currentWeapon != null) Destroy(currentWeapon);
        if (CurrentClass.weaponPrefab != null)
        {
            Transform hand = modelAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            currentWeapon = Instantiate(CurrentClass.weaponPrefab, hand);
            currentWeapon.transform.localPosition = CurrentClass.weaponPosition;
            currentWeapon.transform.localRotation = Quaternion.Euler(CurrentClass.weaponRotation);
            currentWeapon.transform.localScale = CurrentClass.weaponScale;
        }

        if (glint)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(Flash(CurrentClass.color));

            SkillUtil.FlashDisc(transform.position, 1.8f, CurrentClass.color, 0.35f);
            Fx.Burst(transform.position, CurrentClass.color, 20, 4f, 0.2f, 0.5f);
        }
    }

    IEnumerator Flash(Color c)
    {
        float t = 0f;
        while (t < flashDuration)
        {
            float k = 1f - t / flashDuration;
            Color e = c * (k * flashIntensity);
            foreach (Material m in bodyMaterials) m.SetColor("_EmissionColor", e);
            t += Time.deltaTime;
            yield return null;
        }
        foreach (Material m in bodyMaterials) m.SetColor("_EmissionColor", Color.black);
    }

    Skill GetSkill(int slot)
    {
        if (slot == 0) return CurrentClass.skill1;
        if (slot == 1) return CurrentClass.skill2;
        if (slot == 2) return CurrentClass.skill3;
        return CurrentClass.autoAttack;
    }

    public float GetCooldownRemaining(int slot)
    {
        return Mathf.Max(0f, cooldownReady[active, slot] - Time.time);
    }

    public float GetCooldownDuration(int slot)
    {
        Skill s = GetSkill(slot);
        return s == null ? 0f : s.cooldown * GameProgression.GetClassCooldownMultiplier(CurrentClass.className);
    }

    public float ScaleSkillDamage(float damage)
    {
        return damage * GameProgression.GetClassSkillDamageMultiplier(CurrentClass.className);
    }

    public void PlaySkillAnim(Skill s)
    {
        if (s == null || string.IsNullOrEmpty(s.animation) || modelAnimator == null) return;
        modelAnimator.CrossFadeInFixedTime(s.animation, 0.08f);
    }

    void TryUseSkill(int slot)
    {
        if (movement.movementLocked) return;

        Skill skill = GetSkill(slot);
        if (skill == null) return;
        if (Time.time < cooldownReady[active, slot]) return;

        // Turn character to face mouse location instantly before executing attack
        if (movement != null) movement.FaceMouse();

        cooldownReady[active, slot] = Time.time + GetCooldownDuration(slot);
        PlaySkillAnim(skill);
        StartCoroutine(skill.Use(this));
    }

    public bool AcquireClass(ClassData newClass)
    {
        if (newClass == null || System.Array.IndexOf(classes, newClass) >= 0)
            return false;

        if (classes.Length < maxClassSlots)
        {
            int previousCount = classes.Length;
            System.Array.Resize(ref classes, previousCount + 1);
            classes[previousCount] = newClass;
            ResizeCooldowns(previousCount);
            active = previousCount;
        }
        else
        {
            ClassData droppedClass = classes[active];
            classes[active] = newClass;
            for (int slot = 0; slot < 4; slot++) cooldownReady[active, slot] = 0f;

            if (droppedClass != null)
                DroppedClassPickup.Spawn(droppedClass, transform.position + transform.right * 3.5f);
        }

        nextSwitchTime = Time.time + switchDelay;
        comboWindowOpen = false;
        GameProgression.RegisterClassAcquired(newClass);
        ApplyClass(true);
        if (GameUI.Instance != null)
            GameUI.Instance.ShowNotification($"EQUIPPED: {newClass.className}!\nPress Q to switch classes");
        return true;
    }

    void ResizeCooldowns(int previousCount)
    {
        float[,] newCooldowns = new float[classes.Length, 4];
        for (int classIndex = 0; classIndex < previousCount; classIndex++)
        {
            for (int skillIndex = 0; skillIndex < 4; skillIndex++)
                newCooldowns[classIndex, skillIndex] = cooldownReady[classIndex, skillIndex];
        }
        cooldownReady = newCooldowns;
    }

    public void BeginGunnerJumpCombo(ClassData sourceClass)
    {
        gunnerJumpComboActive = true;
        gunnerJumpSourceClass = sourceClass;
        gunnerJumpTargetClass = null;
    }

    public ClassData EndGunnerJumpCombo()
    {
        gunnerJumpComboActive = false;
        gunnerJumpSourceClass = null;
        ClassData targetClass = gunnerJumpTargetClass;
        gunnerJumpTargetClass = null;
        return targetClass;
    }
}
