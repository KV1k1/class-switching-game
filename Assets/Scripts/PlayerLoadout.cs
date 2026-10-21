using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLoadout : MonoBehaviour
{
    public ClassData[] classes;
    public float switchDelay = 0.3f;
    public Animator modelAnimator;

    [Header("Switch glint")]
    public float flashDuration = 0.5f;
    public float flashIntensity = 2f;

    public bool comboWindowOpen;

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
        cooldownReady = new float[classes.Length, 3];

        // Collect body materials now, before any weapon exists
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
        ApplyClass(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= nextSwitchTime)
        {
            active = (active + 1) % classes.Length;
            nextSwitchTime = Time.time + switchDelay;
            comboWindowOpen = false;
            ApplyClass(true);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) TryUseSkill(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryUseSkill(1);
        if (Input.GetMouseButton(0))          TryUseSkill(2);
    }

    void ApplyClass(bool glint)
    {
        // Swap the weapon in the right hand
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

            SkillUtil.FlashDisc(transform.position, 1.8f, CurrentClass.color, 0.35f);         // NEW
            Fx.Burst(transform.position, CurrentClass.color, 20, 4f, 0.2f, 0.5f);             // NEW
        }
    }

    IEnumerator Flash(Color c)
    {
        float t = 0f;
        while (t < flashDuration)
        {
            float k = 1f - t / flashDuration;   // 1 -> 0
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
        return CurrentClass.autoAttack;
    }

    public float GetCooldownRemaining(int slot)
    {
        return Mathf.Max(0f, cooldownReady[active, slot] - Time.time);
    }

    public float GetCooldownDuration(int slot)
    {
        Skill s = GetSkill(slot);
        return s == null ? 0f : s.cooldown;
    }

        // Plays the animation named on a skill (does nothing if the field is empty)
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

        cooldownReady[active, slot] = Time.time + skill.cooldown;
        PlaySkillAnim(skill);                      // NEW
        StartCoroutine(skill.Use(this));
    }
}