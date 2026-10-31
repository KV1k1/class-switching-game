using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Projectile Shot")]
public class ProjectileSkill : Skill
{
    public float damage = 8f;
    public float speed = 25f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        Transform p = user.transform;
        Projectile.Spawn(p.position + p.forward * 0.8f, p.forward, speed, damage, Color.yellow);
        yield break;
    }
}