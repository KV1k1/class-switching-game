using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Warrior/Double Slash")]
public class DoubleSlashSkill : Skill
{
    public float damage = 15f;
    public float range = 2.5f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        // 1. SPAWN SLASH VFX
        if (vfxPrefab != null)
        {
            // Spawn slightly in front of the player at chest height
            Vector3 spawnPos = user.transform.position + Vector3.up * 1.2f + user.transform.forward * 1f;
            GameObject vfx = Instantiate(vfxPrefab, spawnPos, user.transform.rotation);
            
            // Auto-destroy the particle object after its animation finishes
            Destroy(vfx, 1.5f);
        }

        // 2. DAMAGE LOGIC
        Vector3 hitCenter = user.transform.position + user.transform.forward * (range * 0.5f) + Vector3.up * 1f;
        SkillUtil.DamageSphere(hitCenter, range, damage, user.gameObject);

        yield break;
    }
}