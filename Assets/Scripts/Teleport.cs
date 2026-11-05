using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Mage/Teleport")]
public class Teleport : Skill
{
    public float teleportDistance = 5f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        Fx.Burst(user.transform.position, Color.magenta, 10, 2f, 0.2f, 0.5f);

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

        Fx.Burst(user.transform.position, Color.magenta, 10, 2f, 0.2f, 0.5f);
        yield break;
    }
}