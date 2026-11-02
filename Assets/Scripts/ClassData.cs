using UnityEngine;

[CreateAssetMenu(menuName = "Classes/Class Data")]
public class ClassData : ScriptableObject
{
    public string className;
    public Color color = Color.white;   // glint color when switching to this class

    [Header("Weapon in hand")]
    public GameObject weaponPrefab;
    public Vector3 weaponPosition;
    public Vector3 weaponRotation;
    public Vector3 weaponScale = Vector3.one;

    [Header("Skills")]
    public Skill autoAttack;
    public Skill skill1;
    public Skill skill2;
    public Skill skill3;
    public Skill landingFinisher;
    public Skill jumpFinisher;
}