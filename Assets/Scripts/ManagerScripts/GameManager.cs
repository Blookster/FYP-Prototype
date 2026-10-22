using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Manager Prefab")]
    public GameObject audioManager_P;
    public GameObject attackPointManager_P;

    [Header("Manager Component")]
    public AudioManager audioManager;
    public AttackPointManager attackPointManager;

    private void Awake()
    {
        if (instance != null) Destroy(this);
        else if (instance == null) instance = this;
    }

    private void Start()
    {
        audioManager = Instantiate(audioManager_P).GetComponent<AudioManager>();
        attackPointManager = Instantiate(attackPointManager_P, transform.position, Quaternion.identity).GetComponent<AttackPointManager>();
    }


}
