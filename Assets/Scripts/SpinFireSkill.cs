using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Spin Fire")]
public class SpinFireSkill : Skill
{
    public int bullets = 18;
    public float duration = 0.4f;
    public float damage = 12f;
    public float bulletSpeed = 25f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        PlayerMovement movement = user.GetComponent<PlayerMovement>();
        Transform p = user.transform;

        movement.Launch(Vector3.zero, 0f);   // hold the player still while spinning

        float step = 360f / bullets;
        float wait = duration / bullets;

        for (int i = 0; i < bullets; i++)
        {
            Projectile.Spawn(p.position + p.forward * 0.8f, p.forward, bulletSpeed, damage, Color.magenta);
            p.Rotate(0, step, 0);
            yield return new WaitForSeconds(wait);
        }

        movement.EndLaunch();
    }
}