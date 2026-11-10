using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Leap")]
public class LeapSkill : Skill
{
    public float distance = 6f;
    public float height = 3f;

    [Header("Landing Impact")]
    public float landingDamage = 35f;
    public float landingRadius = 4.5f;
    public float vfxScale = 1.5f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        PlayerMovement movement = user.GetComponent<PlayerMovement>();

        float g = Mathf.Abs(movement.gravity);
        float upSpeed = Mathf.Sqrt(2f * g * height);
        float airTime = 2f * upSpeed / g;
        Vector3 horizontal = user.transform.forward * (distance / airTime);

        movement.Launch(horizontal, upSpeed);
        user.comboWindowOpen = true;

        yield return new WaitForSeconds(0.1f);
        while (!movement.IsGrounded)
            yield return null;

        movement.EndLaunch();
        user.comboWindowOpen = false;

        // 1. Landing Impact Location
        Vector3 landingPos = user.transform.position;
        landingPos.y = 0.05f; // Keep flush with the ground

        // 2. Spawn Landing Explosion VFX
        if (vfxPrefab != null)
        {
            GameObject explosion = Instantiate(vfxPrefab, landingPos, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * vfxScale;
            Destroy(explosion, 3f);
        }

        // 3. Deal Landing AOE Damage & Screen Shake
        SkillUtil.DamageSphere(landingPos, landingRadius, landingDamage, user.gameObject);

        if (VfxJuice.Instance != null)
        {
            VfxJuice.Instance.ShakeCamera(0.2f, 0.35f);
        }

        // 4. Trigger Class Combo Landing Finisher (if active)
        Skill finisher = user.CurrentClass.landingFinisher;
        if (finisher != null)
        {
            user.PlaySkillAnim(finisher);
            yield return finisher.Use(user);
        }
    }
}