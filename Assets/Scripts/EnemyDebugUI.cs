using UnityEngine;
using TMPro; // Use "using UnityEngine.UI;" if using standard UI text instead of TextMeshPro

public class EnemyDebugUI : MonoBehaviour
{
    public TextMeshProUGUI debugTextField; // Drag your TextMeshPro component here

    // Static references so other scripts (like HitReceiver or EnemyController) can easily pass messages
    public static string enemyStateText = "Idle";
    public static string lastHitResultText = "None";

    void Update()
    {
        if (debugTextField != null)
        {
            debugTextField.text = $"<b>Enemy State:</b> {enemyStateText}\n" +
                                  $"<b>Last Hit Result:</b> {lastHitResultText}";
        }
    }

    // Call these methods from your HitReceiver or EnemyController scripts when events happen
    public static void LogHit(string hitType)
    {
        lastHitResultText = hitType;
        Debug.Log("[Hit Feedback] " + hitType);
    }
}