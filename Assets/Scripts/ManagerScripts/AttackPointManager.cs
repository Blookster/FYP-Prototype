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

    public float nextGameDelay = 1.5f;

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
        InitiateNewGame();
        //StartCoroutine(I_InitiateNewGame(0));
    }

    void InitiateNewGame()
    {
        currentSpawnLimit = 0;
        SpawnAttackPoint();
        ActivateTimer();
    }

    public void DestroyCurrentATP()
    {
        Destroy(currentAttackPoint);
        CheckCurrentSpawnLimit();
    }


    void CheckCurrentSpawnLimit()
    {
        // Deal damage on every bubble hit (drains Guard first, then HP once broken)
        float damage = (PlayerParameter.instance != null) ? PlayerParameter.instance.GetCurrentDamage() : 10f;
        GameManager.instance.enemyHealthscript.TakeDamage(damage);

        AudioManager.instance.Play(1, "hpChangeSfx", false);

        if (currentSpawnLimit >= spawnLimit)
        {
            DeactivateTimer();
            InitiateNewGame();
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

    //IEnumerator I_InitiateNewGame(float initiationDelay)
    //{
    //    yield return new WaitForSeconds(initiationDelay);
    //    currentSpawnLimit = 0;
    //    SpawnAttackPoint();
    //    ActivateTimer();
    //    Debug.Log("Done");
    //}

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
            AudioManager.instance.Play(1, "hpChangeSfx", false);

            UIManager.instance.UpdatePlayerHpSlider(PlayerParameter.instance.currentHp);
            DeactivateTimer();

            Destroy(currentAttackPoint);

            InitiateNewGame();
        }
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
