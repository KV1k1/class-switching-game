using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Melee Swing")]
public class MeleeSwingSkill : Skill
{
    public float damage = 10f;
    public float range = 2.5f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        Transform p = user.transform;
        Vector3 center = p.position + p.forward * (range * 0.5f);
        SkillUtil.DamageSphere(center, range * 0.7f, damage, user.gameObject);
        Fx.SlashArc(p, range, Color.white);
        yield break;
    }
}