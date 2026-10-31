using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Double Slash")]
public class DoubleSlashSkill : Skill
{
    public float damage = 20f;
    public float range = 3f;
    public float delayBetweenHits = 0.25f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        Slash(user.transform, false);
        yield return new WaitForSeconds(delayBetweenHits);
        Slash(user.transform, true);   // second arc is mirrored
    }

    void Slash(Transform player, bool mirror)
    {
        Vector3 center = player.position + player.forward * (range * 0.5f);
        SkillUtil.DamageSphere(center, range * 0.7f, damage, player.gameObject);
        Fx.SlashArc(player, range, new Color(1f, 0.85f, 0.4f), mirror);
    }
}