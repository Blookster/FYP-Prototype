using System.Collections;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
//  GameManager  -  owns the GAME LOOP:  Ready -> Playing -> Victory / Defeat -> restart
// ============================================================================
// WHAT CHANGED AND WHY
//  [NEW]  A GameState. Every other script can now ask "is the fight actually running?"
//         (GameManager.instance.State) instead of guessing.
//  [NEW]  Recentring the player (the VR distance fix, enemy -> player half).
//  [NEW]  Win / lose / restart. Both fighters can die now, so the game needs an ending.
//  Your existing code (the singleton, spawning the Audio and AttackPoint managers) is unchanged.
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { Ready, Playing, Victory, Defeat }

    public GameState State { get; private set; } = GameState.Ready;

    [Header("Manager Prefab")]
    public GameObject audioManager_P;
    public GameObject attackPointManager_P;
    //public GameObject uiManager_P;

    [Header("Manager Component")]
    public AudioManager audioManager;
    public AttackPointManager attackPointManager;
    //public UIManager uiManager;

    [Header("Fighters (auto-found if left empty)")]
    public FSM enemyFsm;
    public P_FSM playerFsm;

    [Header("Fight Setup (VR distance fix)")]

    public bool recenterPlayer = true;

    public float engagementDistance = 1.2f;
    [Tooltip("Seconds to wait after pressing Play before the fight starts. Gives VR head tracking time to settle.")]
    public float readyDelay = 2.0f;

    [Header("Game Over")]
    public GameObject WinPanel;
    public GameObject LosePanel;

    public float restartDelay = 5.0f;

    public E_HealthSystem enemyHealthscript;
    public P_HealthSystem playerHealthscript;

    private void Awake()
    {
        if (instance != null) Destroy(this);
        else if (instance == null) instance = this;
    }

    private void Start()
    {
        audioManager = Instantiate(audioManager_P).GetComponent<AudioManager>();
        attackPointManager = Instantiate(attackPointManager_P, transform.position, Quaternion.identity).GetComponent<AttackPointManager>();

        if (WinPanel != null)
            WinPanel.SetActive(false);

        if (LosePanel != null)
            LosePanel.SetActive(false);

        //uiManager = Instantiate(uiManager_P, transform.position, Quaternion.identity).GetComponent<UIManager>();

        // ---- Find the two fighters if they weren't dragged in ---------------------------
        // EnemyParameter / PlayerParameter already have static "instance" fields, so we can
        // reach their GameObjects without searching the whole scene.

        //if (enemyFsm == null && EnemyParameter.instance != null) enemyFsm = GameObject.FindWithTag("Enemy").GetComponent<FSM>();
        //if (playerFsm == null && PlayerParameter.instance != null) playerFsm = GameObject.FindWithTag("player").GetComponent<P_FSM>();

        //if (enemyFsm == null || playerFsm == null)
        //{
        //    Debug.LogError("[GameManager] Could not find the enemy FSM and/or the player P_FSM. Drag them into the GameManager.");
        //    return;
        //}

        // ---- Listen for deaths ------------------------------------------------------------
        // "+=" subscribes a function to the event we added to HealthSystem.

        //enemyHealth = enemyFsm.GetComponent<HealthSystem>();
        //playerHealth = playerFsm.GetComponent<HealthSystem>();

        //enemyHealth.OnDied += OnEnemyDied;
        //playerHealth.OnDied += OnPlayerDied;

        StartCoroutine(StartFight());
    }

    // Always unsubscribe from events when we are destroyed, otherwise the event keeps a dead
    // reference to us (a classic source of "MissingReferenceException" after reloading a scene).
    private void OnDestroy()
    {
        if (enemyHealthscript != null) enemyHealthscript.OnDied -= OnEnemyDied;
        if (playerHealthscript != null) playerHealthscript.OnDied -= OnPlayerDied;
    }

    IEnumerator StartFight()
    {
        State = GameState.Ready;

        yield return new WaitForSeconds(readyDelay);

        if (recenterPlayer) RecenterPlayerOnEnemy();

        State = GameState.Playing;

        //attackPointManager.BeginRound(); // the first bubble appears in front of the player

        Debug.Log("[GameManager] FIGHT!");
    }

    // ------------------------------------------------------------------------------------
    //  THE VR DISTANCE FIX (enemy -> player half)
    // ------------------------------------------------------------------------------------
    // In VR the headset position is wherever the player happens to be standing in their room
    // (the XR origin only places the ROOM, not the person). So we can't assume the player is
    // 1.2 m from the enemy. Instead of moving the enemy (he lives in a ring that shouldn't
    // move) we slide the player's rig so that their HEAD ends up at the right distance.
    void RecenterPlayerOnEnemy()
    {
        PlayerParameter pp = PlayerParameter.instance;
        if (pp == null || pp.headCamera == null || pp.xrOrigin == null) return;

        Vector3 enemyPos = enemyFsm.transform.position;

        Vector3 awayFromEnemy = pp.xrOrigin.position - enemyPos;
        awayFromEnemy.y = 0.0f;
        if (awayFromEnemy.sqrMagnitude < 0.0001f) awayFromEnemy = -enemyFsm.transform.forward;
        awayFromEnemy.Normalize();

        Vector3 targetHead = enemyPos + awayFromEnemy * engagementDistance;

        Vector3 shift = targetHead - pp.headCamera.position;
        shift.y = 0.0f;

        pp.xrOrigin.position += shift;

        Debug.Log("[GameManager] Recentred the player by " + shift.magnitude.ToString("F2") + " m.");
    }

    // ------------------------------------------------------------------------------------
    //  ENDING THE GAME
    // ------------------------------------------------------------------------------------
    void OnEnemyDied()  { EndGame(GameState.Victory, WinPanel); }
    void OnPlayerDied() { EndGame(GameState.Defeat, LosePanel); }

    void EndGame(GameState result, GameObject panel)
    {
        // If the fight is already decided, ignore the second death (both KO'd in the same frame).
        if (State != GameState.Playing) return;

        State = result;
        Debug.Log("[GameManager] Game over: " + result);

        //attackPointManager.EndRound();

        if (panel != null) panel.SetActive(true);
        if (restartDelay > 0.0f) StartCoroutine(RestartAfterDelay());
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //public void SetEnemyHealthSystem(E_HealthSystem enemyHealthSystem)
    //{
    //    if (enemyHealth == null) enemyHealth = enemyHealthSystem;

    //    else Debug.Log("Had been got enemyHealth");
        
    //}

    //public void SetPlayerHealthSystem(P_HealthSystem playerHealthSystem)
    //{
    //    if (playerHealth == null) playerHealth = playerHealthSystem;

    //    else Debug.Log("Had been got playerHealth");

    //}
}
