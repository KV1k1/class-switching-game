using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Mage/Chain Zap")]
public class ChainZapAttack : Skill
{
    public float initialRange = 8f;
    public float bounceRange = 5f;
    public int maxTargets = 3;
    public float damage = 8f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        Vector3 currentPos = user.transform.position + Vector3.up * 1f; // Start at chest height
        List<GameObject> hitTargets = new List<GameObject>();

        for (int i = 0; i < maxTargets; i++)
        {
            GameObject target = FindNearestEnemy(currentPos, hitTargets, i == 0 ? initialRange : bounceRange);
            if (target == null) break;

            hitTargets.Add(target);
            Vector3 targetPos = target.transform.position + Vector3.up * 1f;
            target.GetComponent<Health>()?.TakeDamage(damage);

            // CREATE REAL VISUAL LINE
            GameObject lineObj = new GameObject("ZapVisual");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default")); // Glowing unlit material
            lr.startColor = Color.cyan;
            lr.endColor = Color.blue;
            lr.startWidth = 0.2f;
            lr.endWidth = 0.05f;
            lr.SetPosition(0, currentPos);
            lr.SetPosition(1, targetPos);
            Destroy(lineObj, 0.15f); // Destroy visual after a split second

            currentPos = targetPos;
            yield return new WaitForSeconds(0.1f);
        }
    }

    GameObject FindNearestEnemy(Vector3 center, List<GameObject> ignoreList, float radius)
    {
        GameObject bestTarget = null;
        float bestDist = Mathf.Infinity;

        foreach (Collider hit in Physics.OverlapSphere(center, radius))
        {
            if (!hit.CompareTag("Enemy") || ignoreList.Contains(hit.gameObject)) continue;

            float dist = Vector3.Distance(center, hit.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestTarget = hit.gameObject;
            }
        }
        return bestTarget;
    }
}