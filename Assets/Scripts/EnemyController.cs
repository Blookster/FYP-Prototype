using UnityEngine;

public class EnemyController : MonoBehaviour
{
    
    public enum FighterState
    {
        Idle,
        Attack,
        Blocking,
        Dodge,
        BeenHit
    }

    [Header("Enemy Stats")]
    public float enemyHP = 100f;
    public float enemyJumpHeight = 5f;
    public FighterState currentEnemyState;

    [Header("AI Decision Settings")]
    public float decisionInterval = 3f;
    private float decisionTimer;

    void Start()
    {
        currentEnemyState = FighterState.Idle;
        decisionTimer = decisionInterval;
    }

    void Update()
    {
        
        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0)
        {
            MakeDecision();
            decisionTimer = decisionInterval; // Reset the timer
        }
    }

    void MakeDecision()
    {
        // Randomly pick one of the 5 states for arcade-style predictability
        int randomChoice = Random.Range(0, 5);
        currentEnemyState = (FighterState)randomChoice;

        Debug.Log("Enemy AI State Changed To: " + currentEnemyState);

        // Handle specific behaviors based on the chosen state
        switch (currentEnemyState)
        {
            case FighterState.Idle:
                break;
            case FighterState.Attack:
                break;
            case FighterState.Blocking:
                break;
            case FighterState.Dodge:
                break;
            case FighterState.BeenHit:
                break;
        }
    }
}