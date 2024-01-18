using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualTargetSelfDestruct : MonoBehaviour
{

    [SerializeField] private float selfDestructDelay;
    void Start()
    {
        StartCoroutine("SelfDestructIn", selfDestructDelay);
    }

    
    IEnumerator SelfDestructIn(float selfDestructDelaydelay)
    {
        yield return new WaitForSeconds(selfDestructDelay);
        Destroy(gameObject);  
    }


}
