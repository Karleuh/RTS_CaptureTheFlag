using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolManager : MonoBehaviour
{
    
    [SerializeField] private GameObject virtualTarget;

    [SerializeField] private float arenaSize;
    [SerializeField] private int nbVirtualTargets;

    void SpawnVirtualTarget()
    {
        Vector3 position = new Vector3(Random.Range(-arenaSize/2, arenaSize/2), 0, Random.Range(-arenaSize/2, arenaSize/2));
        virtualTarget = Instantiate(virtualTarget, position, Quaternion.identity);
        virtualTarget.transform.parent = gameObject.transform;
        
    }

    void Start()
    {
        for (int i=0; i<nbVirtualTargets; i++)
        {
            SpawnVirtualTarget();
        }
    }
}
