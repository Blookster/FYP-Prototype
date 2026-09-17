using UnityEngine;
using System.Collections;

public class FighterVisualManager : MonoBehaviour
{
    private Renderer targetRenderer;
    private Material matInstance;
    private Color originalEmission;

    void Awake()
    {
        targetRenderer = GetComponentInChildren<Renderer>();
        if (targetRenderer != null)
        {
            // Use .material to safely instantiate a unique instance preventing asset modification leaks
            matInstance = targetRenderer.material;
            originalEmission = matInstance.GetColor("_EmissionColor");
        }
    }

    public void TriggerFlash(Color flashColor, float duration)
    {
        StartCoroutine(FlashRoutine(flashColor, duration));
    }

    private IEnumerator FlashRoutine(Color color, float duration)
    {
        if (matInstance != null)
        {
            matInstance.EnableKeyword("_EMISSION");
            matInstance.SetColor("_EmissionColor", color * 2f);
            yield return new WaitForSeconds(duration);
            matInstance.SetColor("_EmissionColor", originalEmission);
        }
    }
}