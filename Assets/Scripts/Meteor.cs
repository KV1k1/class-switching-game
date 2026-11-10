using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Mage/Meteor")]
public class MeteorUltimate : Skill
{
    public float castDistanceAhead = 6f;
    public float impactRadius = 6f;
    public float delay = 1.5f;          // Telegraph warning time
    public float impactDelay = 0.3f;    // Time from VFX spawn until the visual explosion hits the ground
    public float damage = 50f;
    
    [Header("Visual Tuning")]
    public float vfxScale = 0.5f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        Vector3 impactCenter = user.transform.position + (user.transform.forward * castDistanceAhead);
        impactCenter.y = 0.05f;

        // 1. Telegraph Warning Disc
        SkillUtil.FlashDisc(impactCenter, impactRadius, new Color(1f, 0.1f, 0f, 0.6f), delay);

        yield return new WaitForSeconds(delay);

        // 2. Spawn Visual Effect
        GameObject explosion = null;
        if (vfxPrefab != null)
        {
            explosion = Instantiate(vfxPrefab, impactCenter, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * vfxScale;
            Destroy(explosion, 3.5f);
        }

        // 3. Wait for the visual impact frame
        if (impactDelay > 0f)
        {
            yield return new WaitForSeconds(impactDelay);
        }

        // 4. Apply Damage & Screen Shake
        SkillUtil.DamageSphere(impactCenter, impactRadius, damage, user.gameObject);
        
        if (VfxJuice.Instance != null)
        {
            VfxJuice.Instance.ShakeCamera(0.25f, 0.45f);
        }
        else
        {
            Debug.LogWarning("Meteor landed, but VfxJuice.Instance was not found in the active scene!");
        }
    }
}