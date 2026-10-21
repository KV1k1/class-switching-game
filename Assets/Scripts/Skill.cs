using System.Collections;
using UnityEngine;

public abstract class Skill : ScriptableObject
{
    public float cooldown = 1f;
    public string animation;   // name of the Animator state to play, e.g. "Slash"

    public abstract IEnumerator Use(PlayerLoadout user);
}