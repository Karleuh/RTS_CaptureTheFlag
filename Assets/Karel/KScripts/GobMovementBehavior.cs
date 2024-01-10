using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GobMovementBehavior : MonoBehaviour
{


    private FieldOfView FOVScript;

    void Start()
    {
        FOVScript = GetComponent<FieldOfView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (FOVScript.visibleTargets.Count > 0)
        {
            Debug.Log(FOVScript.visibleTargets);
        }
    }
}
