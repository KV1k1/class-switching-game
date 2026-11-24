using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Mage/Teleport")]
public class Teleport : Skill
{
    public float teleportDistance = 5f;
    [Range(0.1f, 1f)] public float comboSwitchWindow = 0.45f;
    public float teleportVfxScale = 0.8f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        ClassData mageClass = user.CurrentClass;
        Vector3 start = user.transform.position;
        SpawnTeleportVfx(start, user);

        // Raycast from chest height so it doesn't hit the floor by mistake
        Vector3 rayStart = user.transform.position + Vector3.up * 1f;
        Vector3 dest = user.transform.position + (user.transform.forward * teleportDistance);

        if (Physics.Raycast(rayStart, user.transform.forward, out RaycastHit hit, teleportDistance))
        {
            dest = hit.point - (user.transform.forward * 0.5f);
            dest.y = user.transform.position.y; // Keep feet on the ground level
        }

        // BYPASS CHARACTER CONTROLLER LOCK
        CharacterController cc = user.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        user.transform.position = dest;

        if (cc != null) cc.enabled = true;

        SpawnTeleportVfx(user.transform.position, user);

        // Teleport opens a brief switch window. The finisher is chosen from the
        // class switched into, so each destination class gets its own combo.
        user.comboWindowOpen = true;
        float windowEnds = Time.time + comboSwitchWindow;
        while (Time.time < windowEnds && user.CurrentClass == mageClass)
            yield return null;

        bool switchedDuringWindow = user.CurrentClass != mageClass;
        user.comboWindowOpen = false;
        if (!switchedDuringWindow) yield break;

        Skill finisher = GetTeleportFinisher(user.CurrentClass);
        if (finisher == null) yield break;

        SpawnTeleportVfx(user.transform.position, user);
        user.PlaySkillAnim(finisher);
        yield return finisher.Use(user);
    }

    static Skill GetTeleportFinisher(ClassData destinationClass)
    {
        if (destinationClass == null) return null;

        // These are the existing class-pair switch finishers:
        // Warrior's spin finisher and Gunner's fire-spin finisher.
        if (destinationClass.className == "Warrior")
            return destinationClass.jumpFinisher ?? destinationClass.landingFinisher;
        if (destinationClass.className == "Gunner")
            return destinationClass.landingFinisher ?? destinationClass.jumpFinisher;

        // Keep the system extensible for future classes with a configured
        // switch finisher.
        return destinationClass.landingFinisher ?? destinationClass.jumpFinisher;
    }

    void SpawnTeleportVfx(Vector3 position, PlayerLoadout user)
    {
        if (vfxPrefab == null)
        {
            Fx.Burst(position, Color.magenta, 14, 3f, 0.2f, 0.5f);
            return;
        }

        GameObject effect = Instantiate(vfxPrefab, position + Vector3.up * 0.25f,
            Quaternion.LookRotation(user.transform.forward));
        effect.transform.localScale = Vector3.one * teleportVfxScale;
        Destroy(effect, 3f);
    }
}
