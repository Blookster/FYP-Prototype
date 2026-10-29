using Meta.WitAi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================================
//  AttackPointManager  -  spawns the bubble in front of the player and turns a
//                         successful punch into damage on the enemy.
// ============================================================================
// WHAT CHANGED AND WHY
//  [FIX]  VR DISTANCE PROBLEM, player -> enemy half. The old version spawned the bubble at a
//         FIXED world point (spawnPoint). In VR the player can stand anywhere in their room,
//         so a fixed point is sometimes out of reach. Now the bubble is placed RELATIVE TO THE
//         PLAYER'S HEAD at arm's length, every time. The player never has to reach the enemy.
//  [NEW]  Damage. Before, a correct punch only played a sound and respawned the bubble.
//         Now it damages the enemy through FSM.HandleHit (so health bar, hit animation and
//         the KO state all work exactly like a normal hit).
//  [NEW]  It no longer spawns a bubble by itself in Start(). The GameManager calls
//         BeginRound() when the fight actually starts (VR head tracking needs a moment
//         to deliver its first real position, so spawning at frame 0 puts it in the wrong place).
public class AttackPointManager : MonoBehaviour
{
    public static AttackPointManager instance;

    [Header("Bubble Prefabs (one is picked at random each time)")]
    public List<GameObject> attackPoints = new List<GameObject>();

    [Header("Placement (all measured from the player's HEAD)")]
    [Tooltip("Old fixed spawn point. Only used as a fallback if the player's head can't be found.")]
    public Transform spawnPoint;

    [Tooltip("How far in front of the head the bubble appears (metres). Roughly arm's length minus a bit, so the player doesn't have to fully stretch.")]
    public float bubbleDistance = 0.55f;

    [Tooltip("Height relative to the head (metres). Negative = lower than the eyes, around chest height is comfortable.")]
    public float bubbleHeightOffset = -0.25f;

    [Header("Round Rules")]
    [Tooltip("Seconds between a bubble being popped and the next one appearing.")]
    public float respawnDelay = 0.8f;

    [Tooltip("Bubble damage = the player's current punch damage x this. (The player's damage already doubles during the fist-bump special.)")]
    public float bubbleDamageScale = 1.5f;

    [Tooltip("Optional: a particle effect prefab to play where the bubble pops.")]
    public GameObject popEffectPrefab;

    private GameObject currentAttackPoint;
    private Coroutine respawnRoutine;

    public int spawnLimit;
    [SerializeField]
    int currentSpawnLimit = 0;

    public float roundDuration;

    [SerializeField]
    float roundTimer;

    [SerializeField]
    bool isCountingTime;

    private void Awake()
    {
        if (instance != null) Destroy(this);
        else if (instance == null) instance = this;
    }

    private void Start()
    {
        BeginRound();
    }

    public void BeginRound()
    {
        SpawnAttackPoint();
        ActivateTimer();
    }

    public void SpawnAttackPoint()
    {
        currentAttackPoint = Instantiate(attackPoints[Random.Range(0, attackPoints.Count)], spawnPoint.transform.position, Quaternion.identity);
        currentSpawnLimit += 1;
    }

    public void DestroyCurrentATP()
    {
        Destroy(currentAttackPoint);
        GoToNextRound();
    }

    void GoToNextRound()
    {
        if (currentSpawnLimit >= spawnLimit)
        {
            GameManager.instance.enemyHealth.TakeDamage(10);
            UIManager.instance.UpdateEnemyHpSlider(GameManager.instance.enemyHealth.CurrentHp);
            DeactateTimer();
        }
        else
        {
            StartCoroutine(I_SpawnAfterDelay());
        }
    }

    IEnumerator I_SpawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnAttackPoint();
    }

    private void Update()
    {
        if (isCountingTime && roundTimer > 0)
        {
            roundTimer -= Time.deltaTime;
            UIManager.instance.UpdateTimerUI((int)roundTimer);
        }
        else if (roundTimer <= 0)
        {
            GameManager.instance.playerHealth.TakeDamage(10);
            UIManager.instance.UpdatePlayerHpSlider(GameManager.instance.playerHealth.CurrentHp);
            DeactateTimer();

            Destroy(currentAttackPoint);

        }
    }

    void ActivateTimer()
    {
        isCountingTime = true;
        roundTimer = roundDuration;
    }

    void DeactateTimer()
    {
        UIManager.instance.UpdateTimerUI(0);
        isCountingTime = false;
        roundTimer = roundDuration;
    }


    #region Old Version
    // Called by the GameManager when the fight begins.
    //public void BeginRound()
    //{
    //    SpawnAttackPoint();
    //}

    //// Called by the GameManager when the fight ends (win or lose): clean everything up.
    //public void EndRound()
    //{
    //    // Stop a respawn that is still counting down, or a new bubble would appear after the game ended.
    //    if (respawnRoutine != null)
    //    {
    //        StopCoroutine(respawnRoutine);
    //        respawnRoutine = null;
    //    }

    //    // Unity overloads "== null" so a destroyed object counts as null too.
    //    if (currentAttackPoint != null)
    //    {
    //        Destroy(currentAttackPoint);
    //        currentAttackPoint = null;
    //    }
    //}

    //void SpawnAttackPoint()
    //{
    //    if (attackPoints.Count == 0)
    //    {
    //        Debug.LogWarning("[AttackPointManager] No bubble prefabs assigned in 'Attack Points'.");
    //        return;
    //    }

    //    // Never spawn outside the fight (e.g. a respawn timer that finishes just as the game ends).
    //    if (GameManager.instance.State != GameManager.GameState.Playing) return;

    //    Vector3 position;
    //    Quaternion rotation;
    //    GetSpawnPose(out position, out rotation);

    //    // Pick a random prefab from the list. UnityEngine.Random.Range(0, n) returns 0..n-1 for ints.
    //    int index = Random.Range(0, attackPoints.Count);
    //    currentAttackPoint = Instantiate(attackPoints[index], position, rotation);
    //}

    //// Works out WHERE the bubble goes and WHICH WAY it faces. "out" parameters let one
    //// function hand back two results.
    //void GetSpawnPose(out Vector3 position, out Quaternion rotation)
    //{
    //    Transform head = (PlayerParameter.instance != null) ? PlayerParameter.instance.headCamera : null;

    //    if (head == null)
    //    {
    //        // Fallback so the game still runs without a head reference.
    //        position = (spawnPoint != null) ? spawnPoint.position : transform.position;
    //        rotation = Quaternion.identity;
    //        return;
    //    }

    //    // The head's "forward" tilts when the player looks down or up. Flatten it onto the
    //    // floor plane so the bubble stays at the same height however they tilt their head.
    //    Vector3 flatForward = Vector3.ProjectOnPlane(head.forward, Vector3.up);

    //    // Looking straight up/down leaves (almost) nothing after flattening - avoid dividing by ~0.
    //    if (flatForward.sqrMagnitude < 0.001f) flatForward = Vector3.forward;
    //    flatForward.Normalize();

    //    position = head.position + flatForward * bubbleDistance;
    //    position.y = head.position.y + bubbleHeightOffset;

    //    // Rotate the bubble so ITS local +Z points the way the player is facing. That is what
    //    // lets AttackPointBase describe punch directions in local space ("forward" = away from the player).
    //    rotation = Quaternion.LookRotation(flatForward, Vector3.up);
    //}

    //// Called by AttackPointBase when a bubble was punched from the right direction.
    //public void OnAttackPointHit(AttackPointBase bubble)
    //{
    //    GameManager gm = GameManager.instance;

    //    // Ignore hits that land after the fight is over.
    //    if (gm.State != GameManager.GameState.Playing) return;

    //    gm.audioManager.Play(0, "hitSfx", false);

    //    // 1) Remove the bubble FIRST. Destroy() is applied at the end of the frame, so we can still
    //    //    read the bubble's position and settings below.
    //    if (popEffectPrefab != null)
    //    {
    //        Instantiate(popEffectPrefab, bubble.transform.position, bubble.transform.rotation);
    //    }
    //    Destroy(bubble.gameObject);
    //    currentAttackPoint = null;

    //    // 2) Damage the enemy. Damage comes from ONE place (PlayerParameter.GetCurrentDamage), so the
    //    //    fist-bump special automatically makes bubbles hit harder too.
    //    float damage = PlayerParameter.instance.GetCurrentDamage() * bubbleDamageScale * bubble.damageMultiplier;
    //    gm.enemyFsm.HandleHit(damage, bubble.HitsHead);

    //    // 3) Queue the next bubble - but only if that hit didn't just win the fight.
    //    //    (If it did, GameManager already called EndRound() and there is nothing left to spawn.)
    //    if (gm.State == GameManager.GameState.Playing)
    //    {
    //        respawnRoutine = StartCoroutine(SpawnAfterDelay());
    //    }
    //}

    //// A coroutine = a function that can pause. "yield return new WaitForSeconds(x)" pauses
    //// it for x seconds and then carries on from the next line, without freezing the game.
    //IEnumerator SpawnAfterDelay()
    //{
    //    yield return new WaitForSeconds(respawnDelay);
    //    respawnRoutine = null;
    //    SpawnAttackPoint();
    //}
    #endregion
}
