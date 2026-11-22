using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Gunner Jump")]
public class GunnerJumpSkill : Skill
{
    public float distance = 5f;
    public float height = 2.5f;
    public float smokeRadius = 3.5f;
    public float smokeDamagePerTick = 5f;
    public float smokeDuration = 4f;
    public float smokeTickRate = 0.5f;
    public GameObject frostComboVfxPrefab;

    public override IEnumerator Use(PlayerLoadout user)
    {
        PlayerMovement movement = user.GetComponent<PlayerMovement>();
        ClassData startClass = user.CurrentClass;

        // Pass vfxPrefab to SmokeZone.Spawn
        SmokeZone.Spawn(user.transform.position, smokeRadius, smokeDamagePerTick, smokeDuration, smokeTickRate, vfxPrefab);

        float g = Mathf.Abs(movement.gravity);
        float upSpeed = Mathf.Sqrt(2f * g * height);
        float airTime = 2f * upSpeed / g;
        Vector3 horizontal = -user.transform.forward * (distance / airTime);

        movement.Launch(horizontal, upSpeed);
        user.BeginGunnerJumpCombo(startClass);
        user.comboWindowOpen = true;

        float minimumAirEnd = Time.time + 0.1f;
        while (Time.time < minimumAirEnd || !movement.IsGrounded)
            yield return null;

        movement.EndLaunch();
        user.comboWindowOpen = false;

        ClassData switchedToClass = user.EndGunnerJumpCombo();
        if (switchedToClass == null && user.CurrentClass != startClass)
            switchedToClass = user.CurrentClass;

        if (switchedToClass != null)
        {
            Skill finisher = switchedToClass.jumpFinisher;
            bool temporaryFinisher = false;
            if (switchedToClass.className == "Mage")
            {
                FreezeBurstSkill frostFinisher = finisher as FreezeBurstSkill;
                if (frostFinisher == null)
                {
                    FreezeBurstSkill[] frostSkills = Resources.FindObjectsOfTypeAll<FreezeBurstSkill>();
                    if (frostSkills.Length > 0) frostFinisher = frostSkills[0];
                }

                // GunnerJump is loaded for this action, so its VFX reference
                // keeps the frost effect available even if Mage's finisher
                // asset has not been loaded by Unity yet.
                if (frostFinisher == null)
                {
                    frostFinisher = ScriptableObject.CreateInstance<FreezeBurstSkill>();
                    frostFinisher.name = "Runtime Mage Frost Burst";
                    frostFinisher.vfxScale = 0.65f;
                    temporaryFinisher = true;
                }

                if (frostFinisher.vfxPrefab == null)
                    frostFinisher.vfxPrefab = frostComboVfxPrefab;
                if (string.IsNullOrEmpty(frostFinisher.animation))
                    frostFinisher.animation = "Meteor";
                finisher = frostFinisher;
            }

            if (finisher != null)
            {
                user.PlaySkillAnim(finisher);
                yield return finisher.Use(user);
                if (temporaryFinisher) Destroy(finisher);
            }
        }
    }
}
