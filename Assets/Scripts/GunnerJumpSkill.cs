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

    public override IEnumerator Use(PlayerLoadout user)
    {
        PlayerMovement movement = user.GetComponent<PlayerMovement>();
        ClassData startClass = user.CurrentClass;

        SmokeZone.Spawn(user.transform.position, smokeRadius, smokeDamagePerTick, smokeDuration, smokeTickRate);

        float g = Mathf.Abs(movement.gravity);
        float upSpeed = Mathf.Sqrt(2f * g * height);
        float airTime = 2f * upSpeed / g;
        Vector3 horizontal = -user.transform.forward * (distance / airTime);

        movement.Launch(horizontal, upSpeed);
        user.comboWindowOpen = true;            // NEW

        yield return new WaitForSeconds(0.1f);
        while (!movement.IsGrounded)
            yield return null;

        movement.EndLaunch();
        user.comboWindowOpen = false;           // NEW

        if (user.CurrentClass != startClass)
        {
            Skill finisher = user.CurrentClass.jumpFinisher;
            if (finisher != null)
            {
                user.PlaySkillAnim(finisher);      // NEW
                yield return finisher.Use(user);
            }
        }
    }
}