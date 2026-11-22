using UnityEngine;

public class HubResident : MonoBehaviour
{
    public float interactionRange = 3f;
    string residentName;
    string dialogue;
    Transform player;
    bool inRange;
    TextMesh nameTag;

    public void Initialize(string displayName, string greeting)
    {
        residentName = displayName;
        dialogue = greeting;
        nameTag = GetComponentInChildren<TextMesh>();
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    void LateUpdate()
    {
        if (nameTag != null && Camera.main != null)
            nameTag.transform.forward = Camera.main.transform.forward;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        bool nowInRange = offset.sqrMagnitude <= interactionRange * interactionRange;
        if (nowInRange)
        {
            inRange = true;
            if (GameUI.Instance != null)
                GameUI.Instance.SetPickupPrompt("Press E to speak with " + residentName);
            if (Time.timeScale > 0f && Input.GetKeyDown(KeyCode.E) && GameUI.Instance != null)
            {
                bool firstMeeting = GameProgression.RegisterNpcMet(residentName);
                GameUI.Instance.ShowNotification(residentName + "\n" +
                    (firstMeeting ? dialogue : "Good to see you again.") );
            }
        }
        else if (inRange)
        {
            inRange = false;
            if (GameUI.Instance != null) GameUI.Instance.SetPickupPrompt("");
        }
    }

    void OnDestroy()
    {
        if (inRange && GameUI.Instance != null) GameUI.Instance.SetPickupPrompt("");
    }
}
