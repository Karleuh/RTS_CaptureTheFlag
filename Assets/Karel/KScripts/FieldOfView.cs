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

    [SerializeField] private LayerMask realTargetMask;
    [SerializeField] private LayerMask virtualTargetMask;
    [SerializeField] private LayerMask obstacleMask;

    //Liste des Targets visibles !
    public List<Transform> visibleTargets = new List<Transform>();

    //Liste des Targets virtuelles (pour la patrouille)
    public List<Transform> virtualTargets = new List<Transform>();
    
    public Transform mainTarget;
    public Transform virtualTarget;
    //Visuel
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;

    void Start()
    {

        //Find Target commence ici et ne s'arrete jamais 
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
        //pas besoin d'update visuel (�a a l'air louche pcq le mesh est cr�e localement depuis le gameobject auquel ce script est attach� et bouge tout seul)
        //vertices[0] = new Vector3(0,0,0);
        //vertices[1] = DirFromAngle(viewAngle / 2) * viewRadius;
        //vertices[2] = DirFromAngle(-viewAngle / 2) * viewRadius;

        //mesh.vertices = vertices;

        //Find virtual target commence quand il n'y a pas de main target, et s'arrete dès qu'une main target est trouvée
        if (visibleTargets.Count > 0)
        {
            mainTarget = FindCurrentTarget(visibleTargets);
            StopCoroutine("FindVirtualTargetWithDelay");
        }
        else
        {
            mainTarget = null;
            virtualTarget = FindCurrentTarget(virtualTargets);
            StartCoroutine("FindVirtualTargetWithDelay", FOVRefreshDelay);
        }

    }


    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets(visibleTargets, realTargetMask);
        }
    }

    IEnumerator FindVirtualTargetWithDelay(float delay)
    {
        while(true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets(virtualTargets, virtualTargetMask);
        }
    }




    void FindVisibleTargets(List<Transform> _visibleTargets, LayerMask targetMask)
    {
        _visibleTargets.Clear();
        //Liste des targets dans le cercle de vision maximal
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            //Targets dans le cone de vision
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                //Utilisation d'astar.IsLineWalkable pcq ff les raycasts

                Vector2Int myPos = new Vector2Int();
                myPos.x = Mathf.FloorToInt(transform.position.x);
                myPos.y = Mathf.FloorToInt(transform.position.z);
                Vector2Int targetPos = new Vector2Int();
                targetPos.x = Mathf.FloorToInt(target.position.x);
                targetPos.y = Mathf.FloorToInt(target.position.z);

                if (AStar.IsLineWalkable(myPos, targetPos))
                {
                    _visibleTargets.Add(target);
                }

            }

        }
    }

    Transform FindCurrentTarget(List<Transform> _visibleTargets)
    {
        if (_visibleTargets.Count > 0)
        {
            return _visibleTargets[0];
        }
        else
        {
            return null;
        }
    }
    private Vector3 DirFromAngle(float angle)
    {
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
