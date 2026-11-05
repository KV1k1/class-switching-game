using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Mage/Meteor")]
public class MeteorUltimate : Skill
{
    public float castDistanceAhead = 6f;
    public float impactRadius = 6f;
    public float delay = 2f;
    public float damage = 50f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        Vector3 impactCenter = user.transform.position + (user.transform.forward * castDistanceAhead);
        impactCenter.y = 0.1f; // Flatten to floor

        // Telegraph the danger zone using existing SkillUtil[cite: 11]
        SkillUtil.FlashDisc(impactCenter, impactRadius, Color.red, delay);

        yield return new WaitForSeconds(delay);

        // Meteor lands
        SkillUtil.DamageSphere(impactCenter, impactRadius, damage, user.gameObject);
        Fx.Burst(impactCenter, new Color(1f, 0.3f, 0f), 40, impactRadius, 1f, 1f);
    }
}