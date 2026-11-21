using System.Collections;
using UnityEngine;

public abstract class Skill : ScriptableObject
{
    public Sprite icon;        // NEW: Field for the UI image
    public float cooldown = 1f; //[cite: 14]
    public string animation;
    public GameObject vfxPrefab;   //[cite: 14]

    public abstract IEnumerator Use(PlayerLoadout user); //[cite: 14]
}