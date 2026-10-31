using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Leap")]
public class LeapSkill : Skill
{
    public float distance = 6f;
    public float height = 3f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        PlayerMovement movement = user.GetComponent<PlayerMovement>();

        float g = Mathf.Abs(movement.gravity);
        float upSpeed = Mathf.Sqrt(2f * g * height);
        float airTime = 2f * upSpeed / g;
        Vector3 horizontal = user.transform.forward * (distance / airTime);

        movement.Launch(horizontal, upSpeed);
        user.comboWindowOpen = true;            // NEW: start the Q glow

        yield return new WaitForSeconds(0.1f);
        while (!movement.IsGrounded)
            yield return null;

        movement.EndLaunch();
        user.comboWindowOpen = false;           // NEW: stop the glow on landing

        Skill finisher = user.CurrentClass.landingFinisher;
        if (finisher != null)
        {
            user.PlaySkillAnim(finisher);          // NEW
            yield return finisher.Use(user);
        }
}
}