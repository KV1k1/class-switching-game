using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Slash Down")]
public class SlashDownSkill : Skill
{
    public float damage = 40f;
    public float radius = 3.5f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        Vector3 pos = user.transform.position;
        SkillUtil.DamageSphere(pos, radius, damage, user.gameObject);
        SkillUtil.FlashDisc(pos, radius, Color.red, 0.2f);
        Fx.Burst(pos + Vector3.up * 0.3f, new Color(1f, 0.5f, 0.2f), 30, 8f, 0.3f, 0.5f);   // NEW        
        yield break;
    }
}