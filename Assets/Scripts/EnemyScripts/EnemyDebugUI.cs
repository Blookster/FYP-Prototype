using UnityEngine;
using TMPro;

public class EnemyDebugUI : MonoBehaviour
{
    public TextMeshProUGUI debugTextField;

    public static string enemyStateText = "Idle";
    public static string lastHitResultText = "None";
    public static float currentHP = 100f; // Declared here to fix the compilation error

    void Update()
    {
        Animator enemyAnim = FindObjectOfType<Animator>();
        if (enemyAnim != null)
        {
            AnimatorStateInfo info = enemyAnim.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("Idle")) enemyStateText = "Idle";
            else if (info.IsName("Jab")) enemyStateText = "Jab Attack";
            else if (info.IsName("Right Cross")) enemyStateText = "Right Cross";
            else if (info.IsName("Left Hook")) enemyStateText = "Left Hook";
            else if (info.IsName("Right Hook")) enemyStateText = "Right Hook";
            else if (info.IsName("Left Uppercut")) enemyStateText = "Left Uppercut";
            else if (info.IsName("HeadHitRight")) enemyStateText = "Head Been Hit";
            else if (info.IsName("HeadHitLeft")) enemyStateText = "Head Been Hit";
            else if (info.IsName("Stomach Hit")) enemyStateText = "Body Been Hit";
            else if (info.IsName("Dodge")) enemyStateText = "Dodging";
            else if (info.IsName("Blocking")) enemyStateText = "Blocking";
            else if (info.IsName("KO")) enemyStateText = "KO";
        }

        if (debugTextField != null)
        {
            debugTextField.text = $"<b>Enemy State:</b> {enemyStateText}\n" +
                                  $"<b>Last Hit Result:</b> {lastHitResultText}\n" +
                                  $"<b>HP:</b> {currentHP}";
        }
    }

    public static void LogHit(string hitType)
    {
        lastHitResultText = hitType;
        Debug.Log("[Hit Feedback] " + hitType);
    }
}