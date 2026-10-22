using System.Net.NetworkInformation;
using UnityEngine;

public class LockPosScript : MonoBehaviour
{
    public GameObject headSet;
    Transform headSetTran;

    private void Start()
    {
        headSetTran = headSet.transform;
    }

    private void Update()
    {
        headSet.transform.position = headSetTran.position;
    }

}
