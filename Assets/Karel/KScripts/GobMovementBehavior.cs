using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GobMovementBehavior : MonoBehaviour
{

    public Vector3 direction;
    [SerializeField] private float speed;
    private FieldOfView FOVScript;
    private Transform mainTarget;
    private Transform virtualTarget;
    private bool isMovingTowardMainTarget;

    [SerializeField] private float minRange;
    


    void Start()
    {
        FOVScript = transform.GetChild(1).GetComponent<FieldOfView>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((FOVScript.visibleTargets.Count > 0) && !isMovingTowardMainTarget)
        {
            isMovingTowardMainTarget = true;
            mainTarget = FOVScript.mainTarget;
            if (!(mainTarget==null))
            {
                LookTowards(mainTarget);
                MoveTowards(mainTarget);
            }
        }
        else 
        {
            isMovingTowardMainTarget = false;
            virtualTarget = FOVScript.virtualTarget;
            if (!(virtualTarget==null))
            {
                LookTowards(virtualTarget);
                MoveTowards(virtualTarget);
            }
        }
    }



    void MoveTowards(Transform currentTarget)
    {
        direction = (currentTarget.position - transform.position);

        float distance = direction.magnitude;

        if (distance > minRange)
        {
        Vector3 normalizedDirection = direction.normalized;
        transform.position = transform.position + normalizedDirection*Time.deltaTime*speed;
        }
    }

    void LookTowards(Transform currentTarget)
    {
        //AJOUTER ICI ANIMATION DE OMG JLAI REPERER
        transform.LookAt(currentTarget);
    }

}
