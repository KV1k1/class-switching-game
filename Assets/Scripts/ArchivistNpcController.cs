using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class ArchivistNpcController : MonoBehaviour
{
    static ArchivistNpcController activeArchivist;
    const float InteractionRange = 3.5f;
    const float WalkSpeed = 1.1f;
    const float PatrolRadius = 1.8f;

    Animator animator;
    AnimationClip idleClip;
    AnimationClip walkingClip;
    AnimationClip greetClip;
    AnimationPlayableOutput animationOutput;
    PlayableGraph animationGraph;
    AnimationClipPlayable currentClipPlayable;
    AnimationClip currentClip;
    bool currentClipLoops;
    Transform player;
    Vector3 patrolCenter;
    Vector3 patrolTarget;
    float nextPatrolTime;
    bool patrolEnabled;
    bool greeting;
    bool inRange;
    bool interactionEnabled;
    bool hubResident;
    bool questOfferPending;
    TextMesh nameTag;

    public static ArchivistNpcController SpawnForFirstExpedition(Health playerHealth)
    {
        if (GameProgression.ArchivistRecruited || playerHealth == null) return null;
        ArchivistNpcController existing = FindExistingArchivist();
        if (existing != null) return existing;

        ArenaBuilder assets = Object.FindFirstObjectByType<ArenaBuilder>();
        if (!HasRequiredAssets(assets)) return null;

        Transform playerTransform = playerHealth.transform;
        Vector3 position = new Vector3(0f, 0f, 4f);
        Vector3 towardPlayer = playerTransform.position - position;
        towardPlayer.y = 0f;
        Quaternion facing = towardPlayer.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(towardPlayer.normalized)
            : Quaternion.identity;
        ArchivistNpcController npc = Spawn(assets, position, facing);
        npc.Configure(assets, playerTransform, false, false, true);
        return npc;
    }

    public static ArchivistNpcController SpawnForHub(Vector3 position)
    {
        ArchivistNpcController existing = FindExistingArchivist();
        if (existing != null)
        {
            existing.transform.position = position;
            existing.Configure(Object.FindFirstObjectByType<ArenaBuilder>(), FindPlayer(), true, false, true);
            return existing;
        }

        ArenaBuilder assets = Object.FindFirstObjectByType<ArenaBuilder>();
        if (!HasRequiredAssets(assets)) return null;

        ArchivistNpcController npc = Spawn(assets, position, Quaternion.identity);
        npc.Configure(assets, FindPlayer(), true, false, true);
        return npc;
    }

    public static ArchivistNpcController SpawnAtReturnPortal(Vector3 portalPosition, Transform playerTransform)
    {
        ArenaBuilder assets = Object.FindFirstObjectByType<ArenaBuilder>();
        if (!HasRequiredAssets(assets)) return null;

        Vector3 right = playerTransform != null ? playerTransform.right : Vector3.right;
        Vector3 position = portalPosition + right * 4f;
        position.y = 0f;
        Quaternion facing = playerTransform != null
            ? Quaternion.LookRotation(playerTransform.position - position)
            : Quaternion.identity;

        ArchivistNpcController npc = FindExistingArchivist();
        if (npc == null)
            npc = Spawn(assets, position, facing);
        else
        {
            npc.transform.position = position;
            npc.transform.rotation = facing;
            AlignFeetToGround(npc.transform, position.y);
        }
        npc.Configure(assets, playerTransform, false, true, false);
        return npc;
    }

    static ArchivistNpcController FindExistingArchivist()
    {
        if (activeArchivist != null) return activeArchivist;
        activeArchivist = Object.FindFirstObjectByType<ArchivistNpcController>();
        return activeArchivist;
    }

    static bool HasRequiredAssets(ArenaBuilder assets)
    {
        if (assets != null && assets.archivistPrefab != null && assets.archivistIdleClip != null &&
            assets.archivistWalkingClip != null && assets.archivistGreetClip != null)
            return true;

        Debug.LogError("Archivist prefab or animation clips are not assigned on ArenaBuilder.");
        return false;
    }

    static ArchivistNpcController Spawn(ArenaBuilder assets, Vector3 position, Quaternion rotation)
    {
        GameObject character = Object.Instantiate(assets.archivistPrefab, position, rotation);
        character.name = "The Archivist";
        character.transform.localScale = Vector3.one * 2f;
        AlignFeetToGround(character.transform, position.y);
        activeArchivist = character.AddComponent<ArchivistNpcController>();
        return activeArchivist;
    }

    static void AlignFeetToGround(Transform character, float groundHeight)
    {
        SkinnedMeshRenderer[] renderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (renderers.Length == 0) return;

        float lowestPoint = float.PositiveInfinity;
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if (renderer != null && renderer.enabled)
                lowestPoint = Mathf.Min(lowestPoint, renderer.bounds.min.y);
        }

        if (!float.IsInfinity(lowestPoint))
            character.position += Vector3.up * (groundHeight - lowestPoint);
    }

    static Transform FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        return playerObject != null ? playerObject.transform : null;
    }

    void Configure(ArenaBuilder assets, Transform playerTransform, bool isHubResident,
        bool greetAtSpawn, bool canInteract)
    {
        if (animationGraph.IsValid()) animationGraph.Destroy();
        currentClipPlayable = default;
        currentClip = null;

        animator = GetComponentInChildren<Animator>();
        idleClip = assets.archivistIdleClip;
        walkingClip = assets.archivistWalkingClip;
        greetClip = assets.archivistGreetClip;
        player = playerTransform != null ? playerTransform : FindPlayer();
        patrolCenter = transform.position;
        patrolEnabled = !greetAtSpawn;
        interactionEnabled = canInteract;
        hubResident = isHubResident;
        greeting = greetAtSpawn;

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animationGraph = PlayableGraph.Create("Archivist Animation");
            animationGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            animationOutput = AnimationPlayableOutput.Create(animationGraph, "Archivist", animator);
        }

        CreateNameTag();
        if (greetAtSpawn)
        {
            greeting = true;
            PlayClip(greetClip, false);
        }
        else
        {
            PlayIdle();
            ChoosePatrolTarget();
        }
    }

    void Update()
    {
        if (currentClipLoops && currentClipPlayable.IsValid() && currentClip != null && currentClip.length > 0f &&
            currentClipPlayable.GetTime() >= currentClip.length)
        {
            currentClipPlayable.SetTime(currentClipPlayable.GetTime() % currentClip.length);
        }

        if (greeting && currentClipPlayable.IsValid() && currentClipPlayable.GetTime() >= greetClip.length)
        {
            greeting = false;
            PlayIdle();
        }

        if (patrolEnabled && !greeting)
            UpdatePatrol();

        if (interactionEnabled)
            UpdateInteraction();
    }

    void UpdatePatrol()
    {
        Vector3 toTarget = patrolTarget - transform.position;
        toTarget.y = 0f;
        if (toTarget.magnitude > 0.15f)
        {
            transform.position = Vector3.MoveTowards(transform.position, patrolTarget, WalkSpeed * Time.deltaTime);
            if (toTarget.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(toTarget.normalized);
            if (currentClip != walkingClip) PlayClip(walkingClip, true);
        }
        else
        {
            if (currentClip != idleClip) PlayIdle();
            if (Time.time >= nextPatrolTime) ChoosePatrolTarget();
        }
    }

    void ChoosePatrolTarget()
    {
        Vector2 offset = Random.insideUnitCircle * PatrolRadius;
        patrolTarget = patrolCenter + new Vector3(offset.x, 0f, offset.y);
        nextPatrolTime = Time.time + Random.Range(1.2f, 2.4f);
    }

    void UpdateInteraction()
    {
        if (player == null) return;

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        bool nowInRange = offset.sqrMagnitude <= InteractionRange * InteractionRange;
        if (nowInRange)
        {
            inRange = true;
            if (GameUI.Instance != null)
            {
                string prompt = hubResident
                    ? "Press E to open The Archivist's memory archive"
                    : questOfferPending
                        ? "Press E again to accept The Archivist's quest"
                        : "Press E to speak with The Archivist";
                GameUI.Instance.SetPickupPrompt(prompt);
            }

            if (Time.timeScale > 0f && Input.GetKeyDown(KeyCode.E)) Interact();
        }
        else if (inRange)
        {
            inRange = false;
            if (GameUI.Instance != null) GameUI.Instance.SetPickupPrompt("");
        }
    }

    void Interact()
    {
        if (GameUI.Instance == null) return;

        if (hubResident)
        {
            GameProgression.RegisterNpcMet("The Archivist");
            GameUI.Instance.OpenArchivistMenu();
            return;
        }

        if (!GameProgression.ArchivistQuestAccepted)
        {
            if (!questOfferPending)
            {
                questOfferPending = true;
                string offer = GameProgression.FirstBossDefeated
                    ? "The Guardian has fallen. Clear the final wave and return to the portal. Press E again to accept my quest."
                    : "The Guardian is waiting beyond the portal. Defeat the boss and clear the final wave. Press E again to accept my quest.";
                GameUI.Instance.ShowThoughtBubble(transform, offer);
                GameUI.Instance.SetPickupPrompt("Press E again to accept The Archivist's quest");
                patrolEnabled = false;
                PlayIdle();
                return;
            }

            GameProgression.AcceptArchivistQuest();
            questOfferPending = false;
            GameUI.Instance.ShowThoughtBubble(transform,
                "Quest accepted. Defeat the Guardian, clear the final wave, and meet me at the return portal.");
            patrolEnabled = false;
            PlayIdle();
            return;
        }

        string response = GameProgression.FirstBossDefeated
            ? "You defeated the Guardian. Finish the expedition and meet me at the return portal."
            : "Defeat the first boss. I will be waiting at the return portal.";
        GameUI.Instance.ShowThoughtBubble(transform, response);
    }

    void PlayIdle()
    {
        PlayClip(idleClip, true);
    }

    void PlayClip(AnimationClip clip, bool loop)
    {
        if (animator == null || clip == null || !animationGraph.IsValid()) return;

        AnimationClipPlayable next = AnimationClipPlayable.Create(animationGraph, clip);
        next.SetApplyFootIK(true);
        animationOutput.SetSourcePlayable(next);
        if (currentClipPlayable.IsValid()) currentClipPlayable.Destroy();
        currentClipPlayable = next;
        currentClip = clip;
        currentClipLoops = loop;
        if (!animationGraph.IsPlaying()) animationGraph.Play();
    }

    void CreateNameTag()
    {
        if (nameTag != null)
        {
            nameTag.text = "The Archivist";
            nameTag.transform.localPosition = new Vector3(0f, 2.35f, 0f);
            return;
        }

        GameObject label = new GameObject("Archivist Name Tag");
        label.transform.SetParent(transform, false);
        label.transform.localPosition = new Vector3(0f, 2.35f, 0f);
        nameTag = label.AddComponent<TextMesh>();
        nameTag.text = "The Archivist";
        nameTag.fontSize = 40;
        nameTag.characterSize = 0.06f;
        nameTag.anchor = TextAnchor.LowerCenter;
        nameTag.alignment = TextAlignment.Center;
        nameTag.color = new Color(0.9f, 0.83f, 0.67f);
    }

    void LateUpdate()
    {
        if (nameTag != null && Camera.main != null)
            nameTag.transform.forward = Camera.main.transform.forward;
    }

    void OnDestroy()
    {
        if (activeArchivist == this) activeArchivist = null;
        if (inRange && GameUI.Instance != null) GameUI.Instance.SetPickupPrompt("");
        if (animationGraph.IsValid()) animationGraph.Destroy();
    }
}
