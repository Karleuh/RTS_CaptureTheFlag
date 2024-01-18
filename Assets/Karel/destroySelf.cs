using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroySelf : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Suicide");
    }

    IEnumerator Suicide()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

}
