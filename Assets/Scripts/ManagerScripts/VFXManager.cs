using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;

    public GameObject hitVFX_F;
    public GameObject hitVFX_L;
    public GameObject hitVFX_R;
    public GameObject hitVFX_B;

    private void Awake()
    {
        if (instance != null) Destroy(this);
        else if(instance == null) instance = this;
    }

    public void SpawnHitVFXInFront(Transform spawnPos)
    {
        Instantiate(hitVFX_F, spawnPos.position, Quaternion.identity);
    }

    public void SpawnHitVFXInLeft(Transform spawnPos)
    {
        Instantiate(hitVFX_L, spawnPos.position, Quaternion.identity);
    }

    public void SpawnHitVFXInRight(Transform spawnPos)
    {
        Instantiate(hitVFX_R, spawnPos.position, Quaternion.identity);
    }

    public void SpawnHitVFXInBottom(Transform spawnPos)
    {
        Instantiate(hitVFX_B, spawnPos.position, Quaternion.identity);
    }
}
