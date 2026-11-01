using Meta.WitAi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class AttackPointManager : MonoBehaviour
{
    public static AttackPointManager instance;

    [Header("Bubble Prefabs (one is picked at random each time)")]
    public List<GameObject> attackPoints = new List<GameObject>();

    [Header("Placement (all measured from the player's HEAD)")]
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

    //[Tooltip("Optional: a particle effect prefab to play where the bubble pops.")]
    //public GameObject popEffectPrefab;

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

    public void DestroyCurrentATP()
    {
        Destroy(currentAttackPoint);
        SpawnNextBall();
    }

    void SpawnNextBall()
    {
        if (currentSpawnLimit >= spawnLimit)
        {
            GameManager.instance.enemyHealthscript.TakeDamage(10);
            UIManager.instance.UpdateEnemyHpSlider(GameManager.instance.enemyHealthscript.currentHp);
            DeactivateTimer();

            // Start next round after player correctly hit all ATP
            StartNextRound();
        }
        else
        {
            StartCoroutine(I_SpawnAfterDelay());
        }
    }
    public void SpawnAttackPoint()
    {
        currentAttackPoint = Instantiate(attackPoints[Random.Range(0, attackPoints.Count)], spawnPoint.transform.position, Quaternion.identity);
        currentSpawnLimit += 1;
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
            GameManager.instance.playerHealthscript.TakeDamage(10);
            UIManager.instance.UpdatePlayerHpSlider(GameManager.instance.playerHealthscript.currentHp);
            DeactivateTimer();

            Destroy(currentAttackPoint);

            // Start next round after timer countdown over
            StartNextRound();
        }
    }

    public void StartNextRound()
    {
        currentSpawnLimit = 0;
        BeginRound();
    }

    void ActivateTimer()
    {
        isCountingTime = true;
        roundTimer = roundDuration;
    }

    void DeactivateTimer()
    {
        UIManager.instance.UpdateTimerUI(0);
        isCountingTime = false;
        roundTimer = roundDuration;
    }

}
