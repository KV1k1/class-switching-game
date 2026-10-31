using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Sword Swirl")]
public class SwordSwirlSkill : Skill
{
    public float damage = 15f;
    public float radius = 4f;
    public int ticks = 3;
    public float tickDelay = 0.15f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        for (int i = 0; i < ticks; i++)
        {
            Vector3 pos = user.transform.position;
            SkillUtil.DamageSphere(pos, radius, damage, user.gameObject);
            SkillUtil.FlashDisc(pos, radius, new Color(1f, 0.5f, 0f), tickDelay);
            yield return new WaitForSeconds(tickDelay);
        }
    }
}