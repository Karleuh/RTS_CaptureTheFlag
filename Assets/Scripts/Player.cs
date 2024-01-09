using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
	[Header("Camera")]
	[SerializeField]
	float terrainHeight;
	[SerializeField]
	Camera cam;
	[SerializeField]
	float camSpeed;
	[SerializeField]
	float camScrollSpeed;

	[Header("Selection")]
	[SerializeField]
	RectTransform selectionSquare;
	[SerializeField]
	float minMassSelectingDistance;
	[SerializeField]
	LayerMask selectableLayer;
	[SerializeField]
	GameObject selectionCirclePrefab;

	[Header("Gameplay")]
	[SerializeField] 
	Team team;

	bool isMassSelecting;
	Vector2 startMousePos;

	HashSet<Unit> selectedUnits = new HashSet<Unit>();

	List<GameObject> selectionCircles = new List<GameObject>();


	private Vector3 camForward;








	// Start is called before the first frame update
	void Start()
    {
		this.camForward = cam.transform.forward;
		this.camForward.y = 0;
		this.camForward.Normalize();

		smesh = testMesh;
	}


	[SerializeField]
	public Mesh testMesh;

	public static Mesh smesh;
    // Update is called once per frame
    void Update()
    {
		HandleSelection();

		if(Input.GetKeyDown(KeyCode.Mouse1) && this.selectedUnits != null)
		{
			Vector2Int p = Vector2Int.FloorToInt(this.GetPosFromScreenPoint(Input.mousePosition));

			Unit targetUnit = null;
			//find if we attack a unit
			Ray r = this.cam.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(r, out RaycastHit info, 100.0f, this.selectableLayer))
			{
				Unit unit = info.collider.GetComponentInParent<Unit>();
				if(unit != null && unit.Team != this.team)
					targetUnit = unit;
			}

			if (this.selectedUnits.Count > 1)
			{
				Formation form = new GameObject().AddComponent<Formation>();
				form.gameObject.AddComponent<MeshFilter>().sharedMesh = testMesh;
				form.gameObject.AddComponent<MeshRenderer>();
				form.OnCreation(this.selectedUnits);
				if (targetUnit != null)
					form.Attack(targetUnit);
				else
					form.MoveTo(p);
			}
			else if (this.selectedUnits.Count == 1)
			{
				HashSet<Unit>.Enumerator enumerator = this.selectedUnits.GetEnumerator();
				enumerator.MoveNext();
				enumerator.Current.MoveTo(this.GetPosFromScreenPoint(Input.mousePosition));
				if (targetUnit != null)
					enumerator.Current.Attack(targetUnit);
				else
					enumerator.Current.MoveTo(p);
			}
		}

		if(Input.GetKeyDown(KeyCode.M))
		{
			foreach(var u in this.selectedUnits)
			{
				((BasicUnit)u).DebugPath();
			}
		}



		Vector3 cammove = Vector3.zero;
		if(Input.GetKey(KeyCode.Z))
		{
			cammove += this.camForward * this.camSpeed * this.cam.transform.position.y * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.S))
		{
			cammove -= this.camForward * this.camSpeed * this.cam.transform.position.y * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.D))
		{
			cammove += this.cam.transform.right * this.camSpeed * this.cam.transform.position.y * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.Q))
		{
			cammove -= this.cam.transform.right * this.camSpeed * this.cam.transform.position.y * Time.deltaTime;
		}

		float scroll = Input.GetAxis("Mouse ScrollWheel");
		if (scroll != 0 && (scroll < 0 || this.cam.transform.position.y > 5))
		{
			cammove += this.cam.transform.forward * scroll * this.camScrollSpeed;
		}

		this.cam.transform.Translate(cammove, Space.World);

	}



	private void HandleSelection()
	{
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			if (!Input.GetKey(KeyCode.LeftShift))
			{
				foreach(var circle in this.selectionCircles)
				{
					circle.SetActive(false);
				}
				this.selectedUnits.Clear();
			}

			this.startMousePos = Input.mousePosition;
			this.isMassSelecting = false;
		}
		if(Input.GetKey(KeyCode.Mouse0))
		{
			if (!this.isMassSelecting && (startMousePos - (Vector2)Input.mousePosition).sqrMagnitude > minMassSelectingDistance * minMassSelectingDistance)
			{

				this.isMassSelecting = true;
				this.selectionSquare.gameObject.SetActive(true);
			}

			if(this.isMassSelecting)
			{
				float w = Input.mousePosition.x - this.startMousePos.x;
				float h = Input.mousePosition.y - this.startMousePos.y;
				this.selectionSquare.anchoredPosition = this.startMousePos + new Vector2(w / 2, h / 2);
				this.selectionSquare.sizeDelta = new Vector2
				(
					Mathf.Clamp(Mathf.Abs(w), this.minMassSelectingDistance, Mathf.Infinity),
					Mathf.Clamp(Mathf.Abs(Mathf.Abs(h)), this.minMassSelectingDistance, Mathf.Infinity)
				);
			}

		}

		if(Input.GetKeyUp(KeyCode.Mouse0))
		{
			if (this.isMassSelecting)
			{
				Vector2 tl = this.GetPosFromScreenPoint(this.startMousePos);
				Vector2 br = this.GetPosFromScreenPoint(Input.mousePosition);
				Vector2 tr = this.GetPosFromScreenPoint(new Vector3(Input.mousePosition.x, this.startMousePos.y));
				Vector2 bl = this.GetPosFromScreenPoint(new Vector3(this.startMousePos.x, Input.mousePosition.y));

				int minX = (int)Mathf.Min(tl.x, br.x		, tr.x, bl.x);
				int maxX = (int)Mathf.Max(tl.x, br.x		, tr.x, bl.x);
				int minY = (int)Mathf.Min(tl.y, br.y		, tr.y, bl.y);
				int maxY = (int)Mathf.Max(tl.y, br.y		, tr.y, bl.y);


				for (int x = minX / UnitManager.chunckSize; x <= maxX/ UnitManager.chunckSize; x++)
				{
					for (int y = minY / UnitManager.chunckSize; y <= maxY / UnitManager.chunckSize; y++)
					{
						IReadOnlyCollection<Unit> units = UnitManager.GetUnitsInChunk(new ChunkPos(x, y));

						if (units != null)
						{
							foreach (var unit in units)
							{
								if (unit.Team == this.team && unit.Position.x > minX && unit.Position.x < maxX && unit.Position.y > minY && unit.Position.y < maxY)
								{
									this.selectedUnits.Add(unit);
									GameObject circle = null;
									if (this.selectedUnits.Count > 0 && this.selectedUnits.Count <= this.selectionCircles.Count)
									{
										circle = this.selectionCircles[this.selectedUnits.Count - 1];
										circle.transform.SetParent(unit.transform);
										circle.transform.localPosition = new Vector3(0, 0.2f, 0);
										circle.SetActive(true);
									}
									else
									{
										circle = GameObject.Instantiate(this.selectionCirclePrefab, unit.transform);
										this.selectionCircles.Add(circle);
									}
								}
							}
						}
					}

				}

				this.selectionSquare.gameObject.SetActive(false);
				this.isMassSelecting = false;


			}
			else
			{
				Ray r = this.cam.ScreenPointToRay(this.startMousePos);
				if (Physics.Raycast(r, out RaycastHit info, 100.0f, this.selectableLayer))
				{
					Unit unit = info.collider.GetComponentInParent<Unit>();
					if (unit != null && unit.Team == this.team)
					{
						this.selectedUnits.Add(unit);

						GameObject circle = null;
						if (this.selectedUnits.Count > 0 && this.selectedUnits.Count <= this.selectionCircles.Count)
						{
							circle = this.selectionCircles[this.selectedUnits.Count - 1];
							circle.transform.SetParent(unit.transform);
							circle.transform.localPosition = new Vector3(0, 0.2f, 0);
							circle.SetActive(true);
						}
						else
						{
							circle = GameObject.Instantiate(this.selectionCirclePrefab, unit.transform);
							this.selectionCircles.Add(circle);
						}
					}
				}

			}
		}
	}

	private Vector2 GetPosFromScreenPoint(Vector3 mousePos)
	{
		//line equation to have O(1)
		Ray r = this.cam.ScreenPointToRay(mousePos);
		float t = (this.terrainHeight - r.origin.y) / (r.direction.y);

		Vector3 collidePoint = r.origin + t * r.direction;

		return  new Vector2(collidePoint.x, collidePoint.z);
	}



	private Unit GetUnitFromMouse()
	{
		Vector2 p = this.GetPosFromScreenPoint(Input.mousePosition);

		IReadOnlyCollection<Unit> units = UnitManager.GetUnitsInChunk(new ChunkPos(p, UnitManager.chunckSize));

		if (units == null)
			return null;

		foreach (Unit u in units)
		{
			if ((u.Position - p).sqrMagnitude < this.minMassSelectingDistance * this.minMassSelectingDistance)
				return u;
		}
		return null;
	}
}
