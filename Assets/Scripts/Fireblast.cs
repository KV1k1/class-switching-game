using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Mage/Fireblast")]
public class Fireblast : Skill
{
    public float speed = 16f;
    public float damage = 20f;
    public float aoeRadius = 3.5f;

    [Header("Visual Tuning")]
    public float fireballScale = 0.8f;     // EDIT THIS IN INSPECTOR (e.g., 0.8 = 80% size, 0.5 = half size)
    public GameObject explosionVfxPrefab; // Drag the explosion VFX specific to Fireblast here!

    public override IEnumerator Use(PlayerLoadout user)
    {
        if (vfxPrefab != null)
        {
            Vector3 spawnPos = user.transform.position + Vector3.up * 1.2f + user.transform.forward * 0.8f;
            GameObject fireball = Instantiate(vfxPrefab, spawnPos, user.transform.rotation);

            // 1. Scale ONLY this Fireblast clone
            fireball.transform.localScale = Vector3.one * fireballScale;

            // 2. Configure VfxProjectile properties for Fireblast
            VfxProjectile proj = fireball.GetComponent<VfxProjectile>();
            if (proj != null)
            {
                proj.Init(user.gameObject, damage, speed);
                proj.aoeRadius = aoeRadius;
                
                // Override explosion VFX to use Fireblast's dedicated explosion
                proj.explosionVfxPrefab = explosionVfxPrefab;
            }
        }

        yield break;
    }
}