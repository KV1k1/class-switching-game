using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Bullet Barrage")]
public class BulletBarrageSkill : Skill
{
    public float duration = 2f;
    public float shotsPerSecond = 15f;
    public float coneAngle = 50f;
    public float damage = 6f;
    public float bulletSpeed = 25f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        PlayerMovement movement = user.GetComponent<PlayerMovement>();
        Transform p = user.transform;

        movement.Launch(Vector3.zero, 0f);   // reuses the lock: player can't move or turn

        float interval = 1f / shotsPerSecond;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float angle = Random.Range(-coneAngle * 0.5f, coneAngle * 0.5f);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * p.forward;
            Projectile.Spawn(p.position + dir * 0.8f, dir, bulletSpeed, damage, Color.cyan);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        movement.EndLaunch();
    }
}