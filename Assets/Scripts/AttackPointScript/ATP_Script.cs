using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATP_Script : MonoBehaviour
{
    public string atpTypeName;

    Transform target;

    [SerializeField]
    bool isActive;

    public float activeDelay;

    // Declarpation of DetectionRanges Dictionary
    Dictionary<string, float> detectionRanges = new Dictionary<string, float>();

    private void Awake()
    {
        // Initalization of DetectionRanges Dictionary
        detectionRanges.Add("F_ATP", 140.0f);
        detectionRanges.Add("L_ATP", 140.0f);
        detectionRanges.Add("R_ATP", 50.0f);
        detectionRanges.Add("B_ATP", 140.0f);
    }

    private void Start()
    {
        isActive = false;
        StartCoroutine(WaitToActive());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerHand" && isActive == true)
        {
            target = other.gameObject.transform;

            switch (atpTypeName)
            {
                case ("F_ATP"):
                    FrontDetection(target);
                    break;
                case ("L_ATP"):
                    LeftDetection(target);
                    break;
                case ("R_ATP"):
                    RightDetection(target);
                    break;
                case ("B_ATP"):
                    BottomDetection(target);
                    break;
                default:
                    Debug.Log("Type of ATP not found!!!");
                    break;
            }
        }
    }

    void FrontDetection(Transform target)
    {
        if (Vector3.Angle(transform.forward, target.position - transform.position) > GetDetectionRange())
        {
            DetectionHit();
        }
        else
        {
            DetectionMiss();
        }
    }

    void LeftDetection(Transform target)
    {
        if (Vector3.Angle(transform.right, target.position - transform.position) > GetDetectionRange())
        {
            DetectionHit();
        }
        else
        {
            DetectionMiss();
        }
    }

    void RightDetection(Transform target)
    {
        if (Vector3.Angle(transform.right, target.position - transform.position) < GetDetectionRange())
        {
            DetectionHit();
        }
        else
        {
            DetectionMiss();
        }
    }

    void BottomDetection(Transform target)
    {
        if (Vector3.Angle(transform.up, target.position - transform.position) > GetDetectionRange())
        {
            DetectionHit();
        }
        else
        {
            DetectionMiss();
        }
    }

    void DetectionHit()
    {
        GameManager.instance.audioManager.Play(1, "hitSfx", false);
        AttackPointManager.instance.DestroyCurrentATP();
        Debug.Log("Hit!!!");
    }

    void DetectionMiss()
    {
        GameManager.instance.audioManager.Play(1, "missSfx", false);
        Debug.Log("Miss!!!");
    }

    float GetDetectionRange()
    {
        float detectionRange = detectionRanges[atpTypeName];

        return detectionRange;
    }

    IEnumerator WaitToActive()
    {
        yield return new WaitForSeconds(activeDelay);
        isActive = true;
    }
}
