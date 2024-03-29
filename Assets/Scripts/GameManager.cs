﻿using System;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
	[System.Serializable]
	public struct SpawnType
	{
		public BasicUnit unit;
		public int minNumber;
		public int maxNumber;

		public UnitActionType unitAction;
		public Vector2 location;

	}

	public static GameManager Instance { get; private set; }

	[SerializeField]
	private Camera cam;
	[SerializeField]
	private float terrainHeight;
	[SerializeField]
	private Menu menu;
	[SerializeField]
	private LineRenderer lineRenderer;
	[SerializeField]
	private List<SpawnType> defenderSpawnTypes;
	[SerializeField]
	private List<SpawnType> attackerSpawnTypes;
	[SerializeField]
	GameObject flag;
	[SerializeField]
	GameObject flagAttacker;

	public Team PlayerTeam { get; set; } = Team.ATTACKER;

	private float startTime;
	private bool isChoosingStartingArea;
	private Vector2Int startingArea;

	private float forbiddenZoneRadius;

	private int remainingDefenserUnits = 0;
	private int remainingAttackerUnits = 0;

	private bool flagTaken = false;
	private BasicUnit king;

	List<BasicUnit> allUnits = new List<BasicUnit>();

	public float GameTime => Time.time - this.startTime;
	public float RemainingPlayerUnit => this.PlayerTeam == Team.ATTACKER ? this.remainingAttackerUnits : this.remainingDefenserUnits;
	public bool IsGameStarted { get; private set; }


	private void Awake()
	{
		GameManager.Instance = this;
	}

	public void StartGame()
	{
		this.startTime = Time.time;
		this.IsGameStarted = true;
	}

	public void GameOver(bool win)
	{
		this.IsGameStarted = false;
		this.menu.OnWin(win);
		if (this.flag.transform.parent != null)
		{
			this.flag.transform.SetParent(null);
			this.flag.transform.position = Vector3.zero;
		}

		foreach (BasicUnit u in this.allUnits)
			if(u != null)
				u.Hit(DamageType.DIRECT, int.MaxValue);
	}

	public void ChooseStartingArea()
	{
		this.flagAttacker.SetActive(false);
		this.remainingDefenserUnits = 0;
		this.remainingAttackerUnits = 0;
		this.allUnits.Clear();

		if (this.PlayerTeam == Team.ATTACKER)
		{
			this.isChoosingStartingArea = true;

			this.lineRenderer.gameObject.SetActive(true);

			int count = 30;
			float radius = Terrain.instance.CenterZoneRadius;
			this.forbiddenZoneRadius = radius;
			this.lineRenderer.positionCount = 30;
			float theta = 2 * Mathf.PI / count;

			for (int i = 0; i < count; i++)
			{
				this.lineRenderer.SetPosition(i, new Vector3(radius * Mathf.Cos(i * theta), 1, radius * Mathf.Sin(i * theta)));
			}
		}
		else
		{
			int count = 100;
			float radius = Terrain.instance.CenterZoneRadius + 10;
			this.forbiddenZoneRadius = Terrain.instance.CenterZoneRadius;
			this.lineRenderer.positionCount = 30;
			float theta = 2 * Mathf.PI / count;

			for (int i = 0; i < count; i++)
			{
				Vector2Int pos = new Vector2Int(Mathf.FloorToInt(radius * Mathf.Cos(i * theta)), Mathf.FloorToInt(radius * Mathf.Sin(i * theta)));

				if (Terrain.instance.IsInTerrain(pos) && Terrain.instance.GetClosestAccessiblePos(pos) == pos)
				{

					this.startingArea = pos;

					this.flagAttacker.transform.position = new Vector3(this.startingArea.x, 0, this.startingArea.y);
					this.flagAttacker.SetActive(true);

					this.SpawnDefendersUnits(Vector2Int.zero, Team.DEFENDER);
					this.SpawnDefendersUnits(this.startingArea, Team.ATTACKER);
					break;
				}
			}
		}
	}

	private void Update()
	{
		if(this.IsGameStarted && Input.GetKeyUp(KeyCode.O))
		{
			foreach(Unit unit in this.allUnits)
			{
				BasicUnit basicUnit = unit as BasicUnit;
				if(basicUnit != null && !basicUnit.IsDead && basicUnit.Team != PlayerTeam)
				{
					basicUnit.Hit(DamageType.DIRECT, int.MaxValue);
				}
			}
		}
		if(this.isChoosingStartingArea && Input.GetKeyUp(KeyCode.Mouse0))
		{
			Vector2Int startingPoint = Vector2Int.FloorToInt(this.GetPosFromScreenPoint(Input.mousePosition));

			if (Terrain.instance.IsInTerrain(startingPoint) && Terrain.instance.GetClosestAccessiblePos(startingPoint) == startingPoint && Utils.SqrDistance(Vector2.zero, startingPoint) > this.forbiddenZoneRadius * this.forbiddenZoneRadius)
			{
				this.isChoosingStartingArea = false;
				this.startingArea = startingPoint;

				this.flagAttacker.transform.position = new Vector3(this.startingArea.x, 0, this.startingArea.y);
				this.flagAttacker.SetActive(true);

				this.lineRenderer.gameObject.SetActive(false);

				this.SpawnDefendersUnits(Vector2Int.zero, Team.DEFENDER);
				this.SpawnDefendersUnits(startingArea, Team.ATTACKER);
				this.StartGame();

			}
		}

		if(this.flagTaken && this.IsGameStarted)
		{
			if (Utils.SqrDistance(this.king.Position, this.startingArea) < 1.0f)
			{
				this.flagTaken = false;
				this.king = null;
				this.GameOver(true);
			}
		}
	}



	public Vector2 GetPosFromScreenPoint(Vector3 mousePos)
	{
		//line equation to have O(1)
		Ray r = this.cam.ScreenPointToRay(mousePos);
		float t = (this.terrainHeight - r.origin.y) / (r.direction.y);

		Vector3 collidePoint = r.origin + t * r.direction;

		return new Vector2(collidePoint.x, collidePoint.z);
	}


	public void SpawnDefendersUnits(Vector2Int position, Team team)
	{
		List<SpawnType> spawnTypes = team == Team.DEFENDER ? this.defenderSpawnTypes : this.attackerSpawnTypes;

		if (spawnTypes.Count == 0)
			return;

		Formation formationf = null;
		List<Unit> currentUnits = new List<Unit>();

		HashSet<Vector2Int> usedPlaces = new HashSet<Vector2Int>();
		Queue<Vector2Int> toAdd = new Queue<Vector2Int>();

		int currentSpawnType = 0;
		int currentSpawnTypeMaxAmount = UnityEngine.Random.Range(spawnTypes[currentSpawnType].minNumber, spawnTypes[currentSpawnType].maxNumber);
		int unitIndex = 0;

		toAdd.Enqueue(position);

		while(toAdd.Count > 0)
		{
			//chooseUnit
			if(unitIndex >= currentSpawnTypeMaxAmount)
			{
				if(this.PlayerTeam != team && currentUnits.Count > 0)
				{
					formationf = new GameObject().AddComponent<Formation>();
					formationf.OnCreation(currentUnits);
					switch(spawnTypes[currentSpawnType].unitAction)
					{
						case UnitActionType.PATROL:
							PatrolAction patrolAction = new PatrolAction(formationf);
							patrolAction.EnqueueMove(new Vector2(spawnTypes[currentSpawnType].location.x, spawnTypes[currentSpawnType].location.y));
							patrolAction.EnqueueMove(new Vector2(spawnTypes[currentSpawnType].location.x, -spawnTypes[currentSpawnType].location.y));
							patrolAction.EnqueueMove(new Vector2(-spawnTypes[currentSpawnType].location.x, -spawnTypes[currentSpawnType].location.y));
							patrolAction.EnqueueMove(new Vector2(-spawnTypes[currentSpawnType].location.x, spawnTypes[currentSpawnType].location.y));
							formationf.EnqueueAction(patrolAction, true);
							break;
						case UnitActionType.ATTACK_MOVE:
							AttackMoveAction attackMoveAction = new AttackMoveAction(formationf);
							attackMoveAction.EnqueueMove(spawnTypes[currentSpawnType].location);
							formationf.EnqueueAction(attackMoveAction, true);
							break;
					}

					currentUnits.Clear();
				}

				currentSpawnType += 1;
				if (currentSpawnType >= spawnTypes.Count)
					break;

				currentSpawnTypeMaxAmount = UnityEngine.Random.Range(spawnTypes[currentSpawnType].minNumber, spawnTypes[currentSpawnType].maxNumber);
				unitIndex = 0;
				if (currentSpawnTypeMaxAmount == 0)
					continue;

			}

			//spawn
			Vector2Int pos = toAdd.Dequeue();
			BasicUnit u = GameObject.Instantiate(spawnTypes[currentSpawnType].unit, new Vector3(pos.x, 0, pos.y), Quaternion.identity);
			currentUnits.Add(u);
			allUnits.Add(u);
			unitIndex++;
			switch(team)
			{
				case Team.DEFENDER:
					this.remainingDefenserUnits++;
					break;
				case Team.ATTACKER:
					this.remainingAttackerUnits++;
					break;
			}


			//floodfill
			for (int i = -2; i <= 4; i+= 4)
			{
				for (int j = 0; j <= 1; j++)
				{
					Vector2Int newPos = new Vector2Int(pos.x + j * i, pos.y + (1-j) * i);
					if (Terrain.instance.IsInTerrain(newPos) && !Terrain.instance.IsObstacle(newPos) && !usedPlaces.Contains(newPos) && (team == Team.DEFENDER || Utils.SqrDistance(Vector2Int.zero, newPos) > this.forbiddenZoneRadius * this.forbiddenZoneRadius))
					{
						toAdd.Enqueue(newPos);
						usedPlaces.Add(newPos);
					}
				}
			}

		}
	}


	//public void SpawnAttackerUnits(Vector2Int position)
	//{
	//	if (this.spawnTypes.Count == 0)
	//		return;

	//	Formation formationf = null;
	//	List<Unit> currentUnits = new List<Unit>();

	//	HashSet<Vector2Int> usedPlaces = new HashSet<Vector2Int>();
	//	Queue<Vector2Int> toAdd = new Queue<Vector2Int>();

	//	int currentSpawnType = 0;
	//	int currentSpawnTypeMaxAmount = UnityEngine.Random.Range(this.spawnTypes[currentSpawnType].minNumber, this.spawnTypes[currentSpawnType].maxNumber);
	//	int unitIndex = 0;

	//	toAdd.Enqueue(position);

	//	while (toAdd.Count > 0)
	//	{
	//		//chooseUnit
	//		if (unitIndex >= currentSpawnTypeMaxAmount)
	//		{
	//			if (this.PlayerTeam != Team.DEFENDER && currentUnits.Count > 0)
	//			{
	//				formationf = new GameObject().AddComponent<Formation>();
	//				formationf.OnCreation(currentUnits);
	//				switch (this.spawnTypes[currentSpawnType].unitAction)
	//				{
	//					case UnitActionType.PATROL:
	//						PatrolAction patrolAction = new PatrolAction(formationf);
	//						patrolAction.EnqueueMove(new Vector2(this.spawnTypes[currentSpawnType].location.x, this.spawnTypes[currentSpawnType].location.y));
	//						patrolAction.EnqueueMove(new Vector2(this.spawnTypes[currentSpawnType].location.x, -this.spawnTypes[currentSpawnType].location.y));
	//						patrolAction.EnqueueMove(new Vector2(-this.spawnTypes[currentSpawnType].location.x, -this.spawnTypes[currentSpawnType].location.y));
	//						patrolAction.EnqueueMove(new Vector2(-this.spawnTypes[currentSpawnType].location.x, this.spawnTypes[currentSpawnType].location.y));
	//						formationf.EnqueueAction(patrolAction, true);
	//						break;
	//					case UnitActionType.ATTACK_MOVE:
	//						AttackMoveAction attackMoveAction = new AttackMoveAction(formationf);
	//						attackMoveAction.EnqueueMove(this.spawnTypes[currentSpawnType].location);
	//						formationf.EnqueueAction(attackMoveAction, true);
	//						break;
	//				}

	//				currentUnits.Clear();
	//			}

	//			currentSpawnType += 1;
	//			if (currentSpawnType >= this.spawnTypes.Count)
	//				break;

	//			currentSpawnTypeMaxAmount = UnityEngine.Random.Range(this.spawnTypes[currentSpawnType].minNumber, this.spawnTypes[currentSpawnType].maxNumber);
	//			unitIndex = 0;
	//			if (currentSpawnTypeMaxAmount == 0)
	//				continue;

	//		}

	//		//spawn
	//		Vector2Int pos = toAdd.Dequeue();
	//		Unit u = GameObject.Instantiate(this.spawnTypes[currentSpawnType].unit, new Vector3(pos.x, 0, pos.y), Quaternion.identity);
	//		currentUnits.Add(u);
	//		unitIndex++;


	//		//floodfill
	//		for (int i = -2; i <= 4; i += 4)
	//		{
	//			for (int j = 0; j <= 1; j++)
	//			{
	//				Vector2Int newPos = new Vector2Int(pos.x + j * i, pos.y + (1 - j) * i);
	//				if (Terrain.instance.IsInTerrain(newPos) && !Terrain.instance.IsObstacle(newPos) && !usedPlaces.Contains(newPos) && Utils.SqrDistance(Vector2Int.zero, newPos) > this.startAreaRadius * this.startAreaRadius)
	//				{
	//					toAdd.Enqueue(newPos);
	//					usedPlaces.Add(newPos);
	//				}
	//			}
	//		}

	//	}
	//}


	public void OnUnitDead(BasicUnit basicUnit)
	{
		if (!this.IsGameStarted)
			return;
		if (this.flagTaken && this.king.gameObject == basicUnit.gameObject)
		{
			this.GameOver(this.PlayerTeam == Team.DEFENDER);

		}
		switch (basicUnit.Team)
		{
			case Team.ATTACKER:
				this.remainingAttackerUnits--;
				if (this.remainingAttackerUnits == 0 && this.PlayerTeam == Team.ATTACKER)
					this.GameOver(false);
				break;
			case Team.DEFENDER:
				this.remainingDefenserUnits--;
				if (this.remainingDefenserUnits == 0 && this.PlayerTeam == Team.DEFENDER)
					this.GameOver(false);
				break;
			default:
				break;
		}
	}

	public void OnWalkOnFlag(BasicUnit unit)
	{
		if (this.flagTaken)
			return;

		this.flagTaken = true;
		this.flag.transform.SetParent(unit.transform.GetChild(0));
		this.flag.transform.localPosition = new Vector3(0.67f, 0.67f, 0);
		this.flag.transform.localRotation = Quaternion.Euler(-180, 90, -90);
		this.king = unit;

		if (this.PlayerTeam == Team.ATTACKER)
		{
			foreach (Unit u in this.allUnits)
			{
				BasicUnit basicUnit = u as BasicUnit;
				if (basicUnit != null && !basicUnit.IsDead && basicUnit.Team == Team.DEFENDER)
				{
					AttackMoveAction action = new AttackMoveAction(basicUnit);
					action.EnqueueAttack(this.king);
					basicUnit.EnqueueAction(action, true);
				}
			}
		}

	}
}
