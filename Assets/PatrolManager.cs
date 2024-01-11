using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolManager : MonoBehaviour
{
    
    [SerializeField] private GameObject virtualTarget;

    [SerializeField] private float arenaSize;

    void SpawnVirtualTarget()
    {
        Vector3 position = new Vector3(Random.Range(-arenaSize, arenaSize), 0, Random.Range(-arenaSize, arenaSize));
        Instantiate(virtualTarget, position, Quaternion.identity);
    }
}
