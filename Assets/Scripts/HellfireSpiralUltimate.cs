using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Hellfire Spiral Ultimate")]
public class HellfireSpiralUltimate : Skill
{
    [Header("Skill Properties")]
    public float duration = 1f;
    public float damage = 15f;
    public float bulletSpeed = 5f;

    [Header("Spiral Settings")]
    public float shotsPerSecond = 1f; // How fast EACH stream fires
    public int streamCount = 2;        // Number of bullet streams (e.g., a 6-point star)
    public float spinSpeed = 36f;     // How fast the spiral rotates (degrees per second)
    public Color ultimateColor = new Color(1f, 0.4f, 0f); // Glowing fiery orange

    public override IEnumerator Use(PlayerLoadout user)
    {
        PlayerMovement movement = user.GetComponent<PlayerMovement>();
        Transform p = user.transform;

        // Lock the player in place while they channel the ultimate
        movement.Launch(Vector3.zero, 0f);

        float interval = 1f / shotsPerSecond;
        float elapsed = 0f;
        float currentAngleOffset = 0f;

        while (elapsed < duration)
        {
            // Fire a bullet for each of the streams (360 degrees divided by stream count)
            for (int i = 0; i < streamCount; i++)
            {
                // Space the streams evenly, then add the rotating offset
                float streamAngle = (360f / streamCount) * i;
                float finalAngle = streamAngle + currentAngleOffset;

                // Calculate the direction vector for this specific bullet
                Vector3 dir = Quaternion.Euler(0, finalAngle, 0) * p.forward;

                // Spawn the projectile (adding Vector3.up * 1f if you need to shoot from chest/gun height)
                Vector3 spawnPos = p.position + (Vector3.up * 1f) + (dir * 0.8f);
                Projectile.Spawn(spawnPos, dir, bulletSpeed, damage, ultimateColor);
            }

            // Wait for the next bullet tick
            yield return new WaitForSeconds(interval);
            elapsed += interval;

            // Rotate the offset for the next wave, creating the spiral shape
            currentAngleOffset += spinSpeed * interval;
        }

        // Release the player back to normal movement
        movement.EndLaunch();
    }
}