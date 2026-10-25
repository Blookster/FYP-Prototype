using System.Collections;
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

    // An enum is a fixed list of names for a value. Much safer than using ints or strings.
    public enum GameState { Ready, Playing, Victory, Defeat }

    // Public read, private write: other scripts can check State, but only the GameManager changes it.
    public GameState State { get; private set; } = GameState.Ready;

    [Header("Manager Prefab")]
    public GameObject audioManager_P;
    public GameObject attackPointManager_P;

    [Header("Manager Component")]
    public AudioManager audioManager;
    public AttackPointManager attackPointManager;

    [Header("Fighters (auto-found if left empty)")]
    public FSM enemyFsm;
    public P_FSM playerFsm;

    [Header("Fight Setup (VR distance fix)")]
    [Tooltip("Slide the player's whole rig so their HEAD starts this far from the enemy, wherever they stand in their room. Turn off if you position the player by hand.")]
    public bool recenterPlayer = true;

    [Tooltip("Head-to-enemy distance in metres at the start of the fight. Keep it SMALLER than the enemy's 'Punch Reach' or the enemy can never hit you.")]
    public float engagementDistance = 1.2f;

    [Tooltip("Seconds to wait after pressing Play before the fight starts. Gives VR head tracking time to settle.")]
    public float readyDelay = 2.0f;

    [Header("Game Over")]
    [Tooltip("Optional: shown on victory (e.g. a world-space canvas that says KO!).")]
    public GameObject victoryPanel;
    [Tooltip("Optional: shown on defeat.")]
    public GameObject defeatPanel;
    [Tooltip("Seconds after the fight ends before the scene reloads. The scene must be in File > Build Settings. 0 or less = never restart.")]
    public float restartDelay = 5.0f;

    private HealthSystem enemyHealth;
    private HealthSystem playerHealth;

    private void Awake()
    {
        if (instance != null) Destroy(this);
        else if (instance == null) instance = this;
    }

    private void Start()
    {
        audioManager = Instantiate(audioManager_P).GetComponent<AudioManager>();
        attackPointManager = Instantiate(attackPointManager_P, transform.position, Quaternion.identity).GetComponent<AttackPointManager>();

        // ---- Find the two fighters if they weren't dragged in ---------------------------
        // EnemyParameter / PlayerParameter already have static "instance" fields, so we can
        // reach their GameObjects without searching the whole scene.
        if (enemyFsm == null && EnemyParameter.instance != null) enemyFsm = EnemyParameter.instance.GetComponent<FSM>();
        if (playerFsm == null && PlayerParameter.instance != null) playerFsm = PlayerParameter.instance.GetComponent<P_FSM>();

        if (enemyFsm == null || playerFsm == null)
        {
            Debug.LogError("[GameManager] Could not find the enemy FSM and/or the player P_FSM. Drag them into the GameManager.");
            return;
        }

        // ---- Listen for deaths ------------------------------------------------------------
        // "+=" subscribes a function to the event we added to HealthSystem.
        enemyHealth = enemyFsm.GetComponent<HealthSystem>();
        playerHealth = playerFsm.GetComponent<HealthSystem>();
        enemyHealth.OnDied += OnEnemyDied;
        playerHealth.OnDied += OnPlayerDied;

        StartCoroutine(StartFight());
    }

    // Always unsubscribe from events when we are destroyed, otherwise the event keeps a dead
    // reference to us (a classic source of "MissingReferenceException" after reloading a scene).
    private void OnDestroy()
    {
        if (enemyHealth != null) enemyHealth.OnDied -= OnEnemyDied;
        if (playerHealth != null) playerHealth.OnDied -= OnPlayerDied;
    }

    IEnumerator StartFight()
    {
        State = GameState.Ready;

        yield return new WaitForSeconds(readyDelay);

        if (recenterPlayer) RecenterPlayerOnEnemy();

        State = GameState.Playing;
        attackPointManager.BeginRound(); // the first bubble appears in front of the player
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

        // Which side of the enemy is the player supposed to be on? Use the direction from the
        // enemy to where you placed the rig in the scene. Flatten (y = 0): we only slide
        // along the floor, never up or down.
        Vector3 awayFromEnemy = pp.xrOrigin.position - enemyPos;
        awayFromEnemy.y = 0.0f;
        if (awayFromEnemy.sqrMagnitude < 0.0001f) awayFromEnemy = -enemyFsm.transform.forward; // rig placed exactly on the enemy: fall back to "in front of him"
        awayFromEnemy.Normalize();

        // Where the head SHOULD be: engagementDistance metres from the enemy, on the player's side.
        Vector3 targetHead = enemyPos + awayFromEnemy * engagementDistance;

        // How far off is the head right now? (floor plane only)
        Vector3 shift = targetHead - pp.headCamera.position;
        shift.y = 0.0f;

        // Moving the rig parent moves the head, hands and body together. Because we move the
        // PARENT, the real-world offset between headset and room origin stays intact.
        pp.xrOrigin.position += shift;

        Debug.Log("[GameManager] Recentred the player by " + shift.magnitude.ToString("F2") + " m.");
    }

    // ------------------------------------------------------------------------------------
    //  ENDING THE GAME
    // ------------------------------------------------------------------------------------
    void OnEnemyDied()  { EndGame(GameState.Victory, victoryPanel); }
    void OnPlayerDied() { EndGame(GameState.Defeat, defeatPanel); }

    void EndGame(GameState result, GameObject panel)
    {
        // If the fight is already decided, ignore the second death (both KO'd in the same frame).
        if (State != GameState.Playing) return;

        State = result;
        Debug.Log("[GameManager] Game over: " + result);

        attackPointManager.EndRound();

        if (panel != null) panel.SetActive(true);
        if (restartDelay > 0.0f) StartCoroutine(RestartAfterDelay());
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
