using UnityEngine;
using TMPro;


public class FpsViewer : MonoBehaviour
{
    public TMP_Text fpsText;

    private void Start()
    {
        fpsText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        float fps = 1.0f / Time.deltaTime;
        fpsText.text = "FPS: " + (int)fps;
    }
}
