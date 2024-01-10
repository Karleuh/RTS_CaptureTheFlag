using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{

    // Sources :
    // Field of view visualisation - Sebastian Lague
    // How To make a mesh in unity (Dynamic, Vertices, UVs, Triangles) - Code Monkey


    [SerializeField] private float FOVRefreshDelay;
    [SerializeField] private float viewRadius;
    [SerializeField] private float viewAngle;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    //Liste des Targets visibles !
    public List<Transform> visibleTargets = new List<Transform>();

    //Visuel
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;

    void Start()
    {
        StartCoroutine("FindTargetsWithDelay", FOVRefreshDelay);


        //VISUEL 
        mesh = new Mesh();

        vertices = new Vector3[3];
      
        Vector2[] uv = new Vector2[3];
        int[] triangles = new int[3];

        triangles[0] = 2;
        triangles[1] = 1;
        triangles[2] = 0;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        vertices[0] = new Vector3(0,0,0);
        vertices[1] = DirFromAngle(viewAngle / 2) * viewRadius;
        vertices[2] = DirFromAngle(-viewAngle / 2) * viewRadius;
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices;

    }

    void Update()
    {
        //pas besoin d'update visuel (ça a l'air louche pcq le mesh est crée localement depuis le gameobject auquel ce script est attaché et bouge tout seul)
        //vertices[0] = new Vector3(0,0,0);
        //vertices[1] = DirFromAngle(viewAngle / 2) * viewRadius;
        //vertices[2] = DirFromAngle(-viewAngle / 2) * viewRadius;

        //mesh.vertices = vertices;

    }


    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        //Liste des targets dans le cercle de vision maximal
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            //Targets dans le cone de vision
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                //RayCast pour s'assurer que la target n'est pas derrière un obstacle
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }

            }

        }
    }
    private Vector3 DirFromAngle(float angle)
    {
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }


}
