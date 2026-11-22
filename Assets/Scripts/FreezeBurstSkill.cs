using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Mage/Frost Burst")]
public class FreezeBurstSkill : Skill
{
    public float radius = 7f;
    public float freezeDuration = 2.5f;
    public float damage = 14f;
    public float vfxScale = 1f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        Vector3 center = user.transform.position;
        Color frostColor = new Color(0.35f, 0.82f, 1f);
        if (GameUI.Instance != null)
            GameUI.Instance.ShowNotification("MAGE COMBO: FROST BURST");
        SkillUtil.FlashDisc(center, radius, frostColor, 0.35f);
        SpawnFrostVfx(center + Vector3.up * 0.2f);

        HashSet<EnemyChase> frozenEnemies = new HashSet<EnemyChase>();
        foreach (Collider hit in Physics.OverlapSphere(center, radius))
        {
            EnemyChase enemy = hit.GetComponentInParent<EnemyChase>();
            if (enemy == null || !frozenEnemies.Add(enemy)) continue;

            enemy.FreezeFor(freezeDuration);
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null) enemyHealth.TakeDamage(damage);
            SpawnFrostVfx(enemy.transform.position + Vector3.up * 0.8f);
        }

        yield break;
    }

    void SpawnFrostVfx(Vector3 position)
    {
        if (vfxPrefab == null)
        {
            Fx.Burst(position, new Color(0.35f, 0.82f, 1f), 18, 4f, 0.2f, 0.7f);
            return;
        }

        GameObject effect = Instantiate(vfxPrefab, position, Quaternion.identity);
        effect.transform.localScale = Vector3.one * vfxScale;
        Destroy(effect, freezeDuration + 0.75f);
    }
}
