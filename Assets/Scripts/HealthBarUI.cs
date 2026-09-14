using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public Transform target;
    public Vector3 offset = new Vector3(0, 2.2f, 0);
    public float maxHP = 100f;
    public Canvas canvas;

    private Camera mainCam;
    private float currentHP;

    void Start()
    {
        mainCam = Camera.main;
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localScale = Vector3.one * 0.01f;
        }
    }

    void LateUpdate()
    {
        if (target != null && mainCam != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(mainCam.transform);
            transform.Rotate(0, 180, 0);
        }
    }

    public void SetHealth(float hp)
    {
        currentHP = hp;
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(hp / maxHP);
        }
    }

    public void Initialize(float maxHealth, Transform followTarget)
    {
        maxHP = maxHealth;
        target = followTarget;
        SetHealth(maxHealth);
    }
}