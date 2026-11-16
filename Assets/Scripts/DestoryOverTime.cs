using System.Collections;
using UnityEngine;

public class DestoryOverTime : MonoBehaviour
{
    public float destroyDelay;

    private void Start()
    {
        StartCoroutine(DestroyVFX());
    }

    IEnumerator DestroyVFX()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(this.gameObject);
    }
}
