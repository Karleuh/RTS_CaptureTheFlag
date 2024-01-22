using System;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
	[System.Serializable]
	public struct SpawnType
	{
		public Unit unit;
		public int minNumber;
		public int maxNumber;

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
	private List<SpawnType> spawnTypes;


	public Team PlayerTeam { get; set; }

	private float startTime;
	private bool isChoosingStartingArea;
	private Vector2Int startingArea;
	private float startAreaRadius;

	private int remainingDefenserUnits;
	private int remainingAttackerUnits;

	public float GameTime => Time.time - this.startTime;
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
		this.menu.OnWin(win);
		this.IsGameStarted = false;
	}

	public void ChooseStartingArea()
	{
		this.isChoosingStartingArea = true;

		this.lineRenderer.gameObject.SetActive(true);

		int count = 30;
		float radius = 20;
		this.startAreaRadius = radius;
		this.lineRenderer.positionCount = 30;
		float theta = 2 * Mathf.PI / count;

		for (int i =0;i<count;i++)
		{
			this.lineRenderer.SetPosition(i, new Vector3(radius * Mathf.Cos(i * theta), 1, radius * Mathf.Sin(i * theta)));
		}
	}

	private void Update()
	{
		if(this.isChoosingStartingArea && Input.GetKeyUp(KeyCode.Mouse0))
		{
			Vector2Int startingPoint = Vector2Int.FloorToInt(this.GetPosFromScreenPoint(Input.mousePosition));

			if (Terrain.instance.IsInTerrain(startingPoint) && Terrain.instance.GetClosestAccessiblePos(startingPoint) == startingPoint && Utils.SqrDistance(Vector2.zero, startingPoint) > this.startAreaRadius * this.startAreaRadius)
			{
				this.isChoosingStartingArea = false;
				this.startingArea = startingPoint;
				this.lineRenderer.gameObject.SetActive(false);

				this.SpawnAlliesUnits(startingArea);
				this.StartGame();

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


	public void SpawnAlliesUnits(Vector2Int position)
	{
		if (this.spawnTypes.Count == 0)
			return;

		HashSet<Vector2Int> usedPlaces = new HashSet<Vector2Int>();
		Queue<Vector2Int> toAdd = new Queue<Vector2Int>();

		int currentSpawnType = 0;
		int currentSpawnTypeMaxAmount = UnityEngine.Random.Range(this.spawnTypes[currentSpawnType].minNumber, this.spawnTypes[currentSpawnType].maxNumber);
		int unitIndex = 0;

		toAdd.Enqueue(position);

		while(toAdd.Count > 0)
		{
			//chooseUnit
			if(unitIndex >= currentSpawnTypeMaxAmount)
			{
				currentSpawnType += 1;
				if (currentSpawnType >= this.spawnTypes.Count)
					break;

				currentSpawnTypeMaxAmount = UnityEngine.Random.Range(this.spawnTypes[currentSpawnType].minNumber, this.spawnTypes[currentSpawnType].maxNumber);
				unitIndex = 0;
				if (currentSpawnTypeMaxAmount == 0)
					continue;
			}

			//spawn
			Vector2Int pos = toAdd.Dequeue();
			GameObject.Instantiate(this.spawnTypes[currentSpawnType].unit, new Vector3(pos.x, 0, pos.y), Quaternion.identity);
			unitIndex++;

			//floodfill
			for (int i = -2; i <= 4; i+= 4)
			{
				for (int j = 0; j <= 1; j++)
				{
					Vector2Int newPos = new Vector2Int(pos.x + j * i, pos.y + (1-j) * i);
					if (Terrain.instance.IsInTerrain(newPos) && !Terrain.instance.IsObstacle(newPos) && !usedPlaces.Contains(newPos) && Utils.SqrDistance(Vector2Int.zero, newPos) > this.startAreaRadius * this.startAreaRadius)
					{
						toAdd.Enqueue(newPos);
						usedPlaces.Add(newPos);
					}
				}
			}

		}
	}


	public void OnUnitDead(Team team)
	{
		switch (team)
		{
			case Team.ANY:
				break;
			case Team.ATTACKER:
				this.remainingAttackerUnits--;
				if()
				break;
			case Team.DEFENDER:
				this.remainingDefenserUnits--;
				break;
			default:
				break;
		}
	}
}
