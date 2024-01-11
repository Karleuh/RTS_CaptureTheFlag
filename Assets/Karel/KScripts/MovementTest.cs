using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTest : MonoBehaviour
{
    private float speed = 2.0f;
    private float rotationSpeed = 100f;

    private BasicUnit unitScript;
    private FieldOfView FOVScript;
    public List<Transform> visibleTargets;


    //Implémentation A_star
    [SerializeField] private A_Star_Karel A_starScript;
    //checkpoints est modifié à chaque appel de A_star de sorte à opti tout le bordel et trouver le cheminxxe
    SimpleConcatLinkedList<Vector2Int> checkpoints = new SimpleConcatLinkedList<Vector2Int>();


    void Start()
    {

        //Utiliser A_StarScript.A_Star(vector2 startpos, vector2 goalpos, checkpoints)

        FOVScript = transform.GetChild(1).GetComponent<FieldOfView>();
        visibleTargets = FOVScript.visibleTargets;

    }   

    void Update()
    {
        if (visibleTargets.Count > 0)
        {

            Transform target = visibleTargets[0];
            Vector2 target_position = target.position;
            Debug.Log(target_position);

            unitScript.MoveTo(target_position);
        }



        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(0,rotationSpeed * Time.deltaTime,0);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(0,-rotationSpeed * Time.deltaTime,0);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position -= transform.forward * speed * Time.deltaTime;
        }
    }
}
