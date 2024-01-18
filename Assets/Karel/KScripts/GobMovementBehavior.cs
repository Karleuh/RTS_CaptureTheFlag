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
    private bool canSpawn = true;

    [SerializeField] private float minRange;
    [SerializeField] private float nextTargetRange;
    [SerializeField] private GameObject virtualTargetGO;

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

            if (virtualTarget ==null)
            {
                virtualTarget = FOVScript.virtualTarget;
                if (virtualTarget ==null)
                {
                    StartCoroutine("SpawnVirtualTarget");
                }
            }
            else
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
        float distance = direction.magnitude;

        if (distance > minRange)
            {
        //AJOUTER ICI ANIMATION DE OMG JLAI REPERER
                StartCoroutine("Wait1SecAndLookTowards", currentTarget);
            }
    }

    IEnumerator SpawnVirtualTarget()
    {
        if (canSpawn)
        {
            canSpawn = false;
            Vector3 SomeWhereInTheFront = transform.forward*Random.Range(0, nextTargetRange) + transform.right*Random.Range(-nextTargetRange/2, nextTargetRange/2);
            Vector3 positionNextTarget = transform.position + SomeWhereInTheFront;
            GameObject newVirtualTarget = Instantiate(virtualTargetGO, positionNextTarget, Quaternion.identity);
            yield return new WaitForSeconds(3.0f);
            canSpawn = true;
        }
        
    }

    IEnumerator Wait1SecAndLookTowards(Transform currentTarget)
    {
        yield return new WaitForSeconds(1.0f);
        transform.LookAt(currentTarget);
    }
}
