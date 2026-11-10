using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Hellfire Spiral Ultimate")]
public class HellfireSpiralUltimate : Skill
{
    [Header("Skill Properties")]
    public float duration = 2f;
    public float damage = 12f;
    public float bulletSpeed = 14f;

    [Header("Spiral Settings")]
    public float shotsPerSecond = 10f;
    public int streamCount = 4;
    public float spinSpeed = 180f;

    [Header("Visual Tuning")]
    public float fireballScale = 0.4f; 
    public GameObject explosionVfxPrefab; // FIXED: Added missing variable declaration

    public override IEnumerator Use(PlayerLoadout user)
    {
        PlayerMovement movement = user.GetComponent<PlayerMovement>();
        Transform p = user.transform;

        movement.Launch(Vector3.zero, 0f);

        float interval = 1f / shotsPerSecond;
        float elapsed = 0f;
        float currentAngleOffset = 0f;

        while (elapsed < duration)
        {
            for (int i = 0; i < streamCount; i++)
            {
                float streamAngle = (360f / streamCount) * i;
                float finalAngle = streamAngle + currentAngleOffset;

                Vector3 dir = Quaternion.Euler(0, finalAngle, 0) * p.forward;
                Vector3 spawnPos = p.position + (Vector3.up * 1f) + (dir * 0.8f);

                if (vfxPrefab != null)
                {
                    // 1. Instantiate the fireball clone
                    GameObject bulletObj = Instantiate(vfxPrefab, spawnPos, Quaternion.LookRotation(dir));

                    // 2. Apply your editable scale setting
                    bulletObj.transform.localScale = Vector3.one * fireballScale;

                    // 3. Initialize projectile behavior and assign the explosion
                    VfxProjectile proj = bulletObj.GetComponent<VfxProjectile>();
                    if (proj != null)
                    {
                        proj.Init(user.gameObject, damage, bulletSpeed);
                        proj.explosionVfxPrefab = explosionVfxPrefab; // No longer causes an error
                    }
                }
                else
                {
                    // Fallback to basic projectile if no VFX prefab is assigned
                    Projectile.Spawn(spawnPos, dir, bulletSpeed, damage, new Color(1f, 0.4f, 0f));
                }
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
            currentAngleOffset += spinSpeed * interval;
        }

        movement.EndLaunch();
    }
}