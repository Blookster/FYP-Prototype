using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AttackPointManager : MonoBehaviour
{
    public List<GameObject> attackPoints = new List<GameObject>();
    public Transform spawnPoint;
    GameObject currentAttackPoint;

    private void Start()
    {
        SpawnAttackPoint();
    }

    void SpawnAttackPoint()
    {
        currentAttackPoint = Instantiate(attackPoints[RandomIndex()], spawnPoint.position, Quaternion.identity);
    }

    int RandomIndex()
    {
        int index = UnityEngine.Random.Range(0, attackPoints.Count);
        return index;
    }

    public void StartNextRound()
    {
        Destroy(currentAttackPoint);
        SpawnAttackPoint();
    }
}
