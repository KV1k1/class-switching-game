using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Circular Slash Ultimate")]
public class CircularSlashUltimate : Skill
{
    public float damage = 50f;
    public float range = 4f;
    public float delayBetweenSpins = 0.3f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        // First full circle slash
        SpinSlash(user.transform, false);
        yield return new WaitForSeconds(delayBetweenSpins);

        // Second full circle slash
        SpinSlash(user.transform, true);
    }

    void SpinSlash(Transform player, bool reverseFx)
    {
        SkillUtil.DamageSphere(player.position, range, damage, player.gameObject);

        // Generate the visual effect dynamically at the player's waist height
        GameObject fxObj = new GameObject("UltimateSlashFX");
        fxObj.transform.position = player.position + Vector3.up * 1f;

        ProceduralSlashFx fx = fxObj.AddComponent<ProceduralSlashFx>();
        fx.radius = range;
    }
}