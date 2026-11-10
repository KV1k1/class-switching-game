using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Circular Slash Ultimate")]
public class CircularSlashUltimate : Skill
{
    public float damage = 50f;
    public float range = 6f;                 // Radius of the large hit circle[cite: 15]
    public float delayBetweenSpins = 0.25f;  // Delay between the two slashes[cite: 15]

    [Header("VFX Tuning")]
    public float vfxScale = 2.5f;            // Size multiplier for the slash prefab

    public override IEnumerator Use(PlayerLoadout user)
    {
        // First full circle slash
        SpinSlash(user, false);
        
        yield return new WaitForSeconds(delayBetweenSpins);

        // Second full circle slash (flipped 180° for visual variation)
        SpinSlash(user, true);
    }

    void SpinSlash(PlayerLoadout user, bool reverseFx)
    {
        Vector3 spawnPos = user.transform.position + Vector3.up * 1f;

        // 1. Apply AOE damage in the enlarged circle[cite: 15]
        SkillUtil.DamageSphere(user.transform.position, range, damage, user.gameObject);

        // 2. Camera impact shake


        // 3. Spawn and scale custom VFX prefab
        if (vfxPrefab != null)
        {
            Quaternion rotation = user.transform.rotation;
            if (reverseFx)
            {
                rotation *= Quaternion.Euler(0, 180f, 0); // Flip second spin direction
            }

            GameObject vfxObj = Instantiate(vfxPrefab, spawnPos, rotation);
            
            // Enlarge the slash prefab into a wide circle around the player
            vfxObj.transform.localScale = Vector3.one * vfxScale;

            Destroy(vfxObj, 2f);
        }
        else
        {
            // Fallback: Use procedural slash if no prefab is assigned[cite: 15]
            GameObject fxObj = new GameObject("UltimateSlashFX");
            fxObj.transform.position = spawnPos;

            ProceduralSlashFx fx = fxObj.AddComponent<ProceduralSlashFx>();
            fx.radius = range;
        }
    }
}