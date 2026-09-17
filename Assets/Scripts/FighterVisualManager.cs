using UnityEngine;
using System.Collections;

public class FighterVisualManager : MonoBehaviour
{
    [Header("Visual Renderers")]
    [Tooltip("The main mesh renderer of the fighter to apply color/emission flashes.")]
    public Renderer fighterRenderer;

    [Header("Flash Settings")]
    private Material targetMaterial;
    private Color originalEmissionColor;
    private Coroutine flashCoroutine;

    void Start()
    {
        if (fighterRenderer == null)
        {
            fighterRenderer = GetComponentInChildren<Renderer>();
        }

        if (fighterRenderer != null)
        {
            // Instantiate material instance to avoid modifying the asset globally across the game
            targetMaterial = fighterRenderer.material;
            if (targetMaterial.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = targetMaterial.GetColor("_EmissionColor");
            }
        }
    }

    // Triggers a temporary color flash on the fighter's body
    public void TriggerFlash(Color flashColor, float duration)
    {
        if (targetMaterial == null) return;

        // If a flash is already running, stop it to restart cleanly
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine(flashColor, duration));
    }

    IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        // Apply flash color
        if (targetMaterial.HasProperty("_EmissionColor"))
        {
            targetMaterial.EnableKeyword("_EMISSION");
            targetMaterial.SetColor("_EmissionColor", flashColor * 2f); // Boost intensity for visual pop
        }
        else if (targetMaterial.HasProperty("_Color"))
        {
            targetMaterial.SetColor("_Color", flashColor);
        }

        yield return new WaitForSeconds(duration);

        // Revert back to original
        if (targetMaterial.HasProperty("_EmissionColor"))
        {
            targetMaterial.SetColor("_EmissionColor", originalEmissionColor);
        }
        else if (targetMaterial.HasProperty("_Color"))
        {
            targetMaterial.SetColor("_Color", Color.white);
        }

        flashCoroutine = null;
    }
}