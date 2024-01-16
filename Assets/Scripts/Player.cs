using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

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
	[SerializeField] UnitActionType unitAction;
	[SerializeField] FormationType formationType;


	[Header("UI")]
	[SerializeField]
	RectTransform orderPanel;
	[SerializeField]
	Canvas canvas;
	[SerializeField]
	RectTransform formationPanel;

	bool isMassSelecting;
	Vector2 startMousePos;

	HashSet<Unit> selectedUnits = new HashSet<Unit>();

	List<GameObject> selectionCircles = new List<GameObject>();

	private Vector3 camForward;
	private Formation currentFormation;







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
		bool isPointerOverUI = this.IsPointerOverUI();

		if(!isPointerOverUI)
			HandleSelection();



		if(!isPointerOverUI && Input.GetKeyDown(KeyCode.Mouse1) && this.selectedUnits.Count > 0)
		{
			this.HandleAction();
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


	private void HandleAction()
	{
		Vector2 targetPos = this.GetPosFromScreenPoint(Input.mousePosition);
		IDamageable targetUnit = null;

		//find if we attack a unit
		Ray r = this.cam.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(r, out RaycastHit info, 100.0f, this.selectableLayer))
		{
			Unit unit = info.collider.GetComponentInParent<Unit>();
			IDamageable damageable = unit as IDamageable;
			if (unit != null && !damageable.IsDead) //TODO pas ouf, faudrait vérifier la team
				targetUnit = damageable;
		}

		Unit currentUnit = null;

		if(this.currentFormation != null)
		{
			currentUnit = currentFormation;
		}
		else if (this.selectedUnits.Count > 1)
		{
			this.currentFormation = new GameObject().AddComponent<Formation>();
			this.currentFormation.gameObject.AddComponent<MeshFilter>().sharedMesh = testMesh;
			this.currentFormation.gameObject.AddComponent<MeshRenderer>();
			this.currentFormation.OnCreation(this.selectedUnits);
			this.currentFormation.FormationType = this.formationType;

			currentUnit = this.currentFormation;
		}
		else if (this.selectedUnits.Count == 1)
		{
			HashSet<Unit>.Enumerator enumerator = this.selectedUnits.GetEnumerator();
			enumerator.MoveNext();

			currentUnit = enumerator.Current;
		}

		bool isShifting = Input.GetKey(KeyCode.LeftShift);
		if (targetUnit != null)
		{

			if (unitAction != currentUnit.UnitActionType || !isShifting || !currentUnit.EnqueueAttack(targetUnit))
			{
				UnitAction action = UnitAction.FromType(this.unitAction, currentUnit);
				if (action.EnqueueAttack(targetUnit))
					currentUnit.EnqueueAction(action, !isShifting);
			}
		}
		else
		{
			if (unitAction != currentUnit.UnitActionType || !isShifting || !currentUnit.EnqueueMove(targetPos))
			{
				UnitAction action = UnitAction.FromType(this.unitAction, currentUnit);
				if (action.EnqueueMove(targetPos))
					currentUnit.EnqueueAction(action, !isShifting);
			}
		}
	}



	private void HandleSelection()
	{
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			this.currentFormation = null;
			this.formationPanel.gameObject.SetActive(false);

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
				this.selectionSquare.anchoredPosition = (this.startMousePos + new Vector2(w / 2, h / 2)) / canvas.scaleFactor;
				this.selectionSquare.sizeDelta = new Vector2
				(
					Mathf.Clamp(Mathf.Abs(w), this.minMassSelectingDistance, Mathf.Infinity),
					Mathf.Clamp(Mathf.Abs(Mathf.Abs(h)), this.minMassSelectingDistance, Mathf.Infinity)
				) / canvas.scaleFactor;
			}

		}

		if(Input.GetKeyUp(KeyCode.Mouse0))
		{
			if (this.isMassSelecting)
			{
				//TODO fix selection
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

				if (this.selectedUnits.Count > 1)							// show formation panel
					this.formationPanel.gameObject.SetActive(true);
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




	//UI


	private bool IsPointerOverUI()
	{
		if (!EventSystem.current.IsPointerOverGameObject() || this.isMassSelecting)
			return false;

		return true;
	}

	public void OnUnitActionSelectionButtonClick(int unitActionType)
	{
		this.unitAction = (UnitActionType)unitActionType;
		for(int i=0; i<this.orderPanel.childCount; i++)
		{
			RectTransform child = this.orderPanel.GetChild(i) as RectTransform;

			child.GetChild(0).GetChild(0).gameObject.SetActive(i == unitActionType - 1);
		}
	}


	public void OnFormationSelectionButtonClick(int formationType)
	{
		this.formationType = (FormationType)formationType;
		for (int i = 0; i < this.formationPanel.childCount; i++)
		{
			RectTransform child = this.formationPanel.GetChild(i) as RectTransform;

			child.GetChild(0).GetChild(0).gameObject.SetActive(i == formationType);
		}

		if (this.currentFormation != null)
			this.currentFormation.FormationType = this.formationType;
	}

}




