using Noise;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Terrain : MonoBehaviour
{

	public static Terrain instance;

	[SerializeField]
	MeshFilter forestMeshFilter;
	[SerializeField]
	MeshFilter plainMeshFilter;

	[SerializeField]
	float strength;

	[SerializeField]
	float scale;

	[SerializeField]
	[Range(0,1)]
	float forestThreshold;
	[SerializeField]
	[Range(0, 1)]
	float nearForestThreshold;

	[SerializeField]
	float niceZoneRadius;

	[SerializeField]
	float aSigmoid;

	[Header("Trees")]
	[SerializeField]
	Mesh trunkMesh;
	[SerializeField]
	Mesh leavesMesh;
	[SerializeField]
	[Range(0, 1)]
	float treeDensity;
	[SerializeField]
	MeshFilter trunksMeshFilter;
	[SerializeField]
	MeshFilter leavesMeshFilter;


	[SerializeField]
	long seed = 0;

	int width = 100;
	int height = 100;

	char[] terrainType; //0 for forest, 1 for near forest, 2 for plain
	Vector2Int[] closestAccessible; 

	List<Vector3> trunksVertices = new List<Vector3>();
	List<int> trunksIndices = new List<int>();
	List<Vector2> trunksUvs = new List<Vector2>();

	List<Vector3> leavesVertices = new List<Vector3>();
	List<int> leavesIndices = new List<int>();
	List<Vector2> leavesUvs = new List<Vector2>();



	private void Start()
	{
		if (Terrain.instance == null)
		{
			Terrain.instance = this;

			this.terrainType = new char[width * height];
			this.Generate();
		}
		else
			GameObject.Destroy(this.gameObject);

	}

	public void Update()
	{
		if (Input.GetKey(KeyCode.K))
			this.Generate();
	}

#region generation

	public void Generate()
	{
		Debug.Log("=================== Generation =======================");
		if(this.seed == 0)
			this.seed = (long)(Random.value * long.MaxValue);
		OpenSimplexNoise noise = new OpenSimplexNoise(this.seed);
		Debug.Log("Seed : " + this.seed);

		List<Vector3> forestVertices = new List<Vector3>();
		List<int> forestIndices = new List<int>();
		List<Vector2> forestUvs = new List<Vector2>();

		List<Vector3> plainVertices = new List<Vector3>();
		List<int> plainIndices = new List<int>();
		List<Vector2> plainUvs = new List<Vector2>();

		this.trunksVertices.Clear();
		this.trunksIndices.Clear();
		this.trunksUvs.Clear();

		this.leavesVertices.Clear();
		this.leavesIndices.Clear();
		this.leavesUvs.Clear();

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{

				//float t = ((float)noise.Evaluate(i * scale, j * scale) + 1 )/ 2;

				float dist = Mathf.Sqrt((i - width / 2) * (i - width / 2) + (j - height / 2) * (j - height / 2));

				float sig = this.Sigmoid(dist, this.aSigmoid, this.niceZoneRadius);

				float t = Mathf.Abs((float)noise.Evaluate(i * scale, j * scale)) * sig;

				this.terrainType[i + j * width] = t < this.forestThreshold ? t < this.nearForestThreshold ? (char) 2 : (char) 1 : (char)0;

				if (t < this.forestThreshold)
				{

					plainVertices.AddRange(new Vector3[]
					{
						new Vector3(i - width/2, 0 , j - height/2),
						new Vector3((i+1) - width/2, 0 , j - height/2),
						new Vector3((i + 1) - width / 2, 0, (j + 1) - height / 2),
						new Vector3(i - width / 2, 0, (j + 1) - height / 2),
					});

					plainIndices.AddRange(new int[]
					{
						plainVertices.Count - 4, plainVertices.Count - 2, plainVertices.Count - 3,
						plainVertices.Count - 4, plainVertices.Count - 1, plainVertices.Count - 2
					});


					plainUvs.AddRange(new Vector2[]
					{
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1,1),
						new Vector2(0, 1),
					});
				}
				else
				{
					forestVertices.AddRange(new Vector3[]
					{
						new Vector3(i - width/2, 0 , j - height/2),
						new Vector3((i+1) - width/2, 0 , j - height/2),
						new Vector3((i + 1) - width / 2, 0, (j + 1) - height / 2),
						new Vector3(i - width / 2, 0, (j + 1) - height / 2),
					});

					forestIndices.AddRange(new int[]
					{
						forestVertices.Count - 4, forestVertices.Count - 2, forestVertices.Count - 3,
						forestVertices.Count - 4, forestVertices.Count - 1, forestVertices.Count - 2
					});


					forestUvs.AddRange(new Vector2[]
					{
						new Vector2(0, 0),
						new Vector2(1, 0),
						new Vector2(1,1),
						new Vector2(0, 1),
					});

					if (Random.value < this.treeDensity)
					{
						Vector2 rand = Random.insideUnitCircle / 2;
						Vector3 pos = new Vector3(i - width / 2, 0, j - height / 2) + new Vector3(0.5f + rand.x, 0, 0.5f + rand.y);

						this.AddTree(pos);
					}
				}


			}
		}
		Mesh forestMesh = new Mesh();

		forestMesh.vertices = forestVertices.ToArray();
		forestMesh.triangles = forestIndices.ToArray();
		forestMesh.uv = forestUvs.ToArray();
		forestMesh.RecalculateNormals();
		this.forestMeshFilter.mesh = forestMesh;

		Mesh plainMesh = new Mesh();

		plainMesh.vertices = plainVertices.ToArray();
		plainMesh.triangles = plainIndices.ToArray();
		plainMesh.uv = plainUvs.ToArray();
		plainMesh.RecalculateNormals();
		this.plainMeshFilter.mesh = plainMesh;

		Mesh trunkMesh = new Mesh();

		trunkMesh.vertices = this.trunksVertices.ToArray();
		trunkMesh.triangles = this.trunksIndices.ToArray();
		trunkMesh.uv = this.trunksUvs.ToArray();
		trunkMesh.RecalculateNormals();
		this.trunksMeshFilter.mesh = trunkMesh;

		Mesh leavesMesh = new Mesh();

		leavesMesh.vertices = this.leavesVertices.ToArray();
		leavesMesh.triangles = this.leavesIndices.ToArray();
		leavesMesh.uv = this.leavesUvs.ToArray();
		leavesMesh.RecalculateNormals();
		this.leavesMeshFilter.mesh = leavesMesh;



		//TODO multiple meshes if too mush vertices
		//TODO fosse mesh

		this.PreCalculateClosestWalkableTiles();
	}

	public void AddTree(Vector3 position)
	{
		int prevTrunkCount = this.trunksVertices.Count;

		foreach (Vector3 vertex in this.trunkMesh.vertices)
		{
			this.trunksVertices.Add(position + vertex);
		}

		foreach (int index in this.trunkMesh.triangles)
		{
			this.trunksIndices.Add(prevTrunkCount + index);
		}

		this.trunksUvs.AddRange(this.trunkMesh.uv);


		int prevLeavesCount = this.leavesVertices.Count;

		foreach (Vector3 vertex in this.leavesMesh.vertices)
		{
			this.leavesVertices.Add(position + vertex);
		}

		foreach (int index in this.leavesMesh.triangles)
		{
			this.leavesIndices.Add(prevLeavesCount + index);
		}

		this.leavesUvs.AddRange(this.leavesMesh.uv);

	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float Sigmoid(float x, float a, float b)
	{
		return 1 / (1 + Mathf.Exp(-a * (x - b)));
	}




	private void PreCalculateClosestWalkableTiles()
	{
		this.closestAccessible = new Vector2Int[this.width * this.height];
		this.closestAccessible[this.width / 2 + (this.height / 2) * this.width] = new Vector2Int(int.MaxValue, int.MaxValue); //set center
		// (x + width/2) + (y + height/2) * this.width
		Queue<Vector2Int> toCalculate = new Queue<Vector2Int>(); 
		toCalculate.Enqueue(new Vector2Int(1, 0));
		toCalculate.Enqueue(new Vector2Int(0, 1));
		toCalculate.Enqueue(new Vector2Int(-1, 0));
		toCalculate.Enqueue(new Vector2Int(0, -1));


		while (toCalculate.Count > 0)
		{
			Vector2Int pos = toCalculate.Dequeue();
			//Debug.Log(pos);
			if (this.closestAccessible[(pos.x + width / 2) + (pos.y + height / 2) * this.width] != Vector2Int.zero)
				continue;

			this.closestAccessible[(pos.x + width / 2) + (pos.y + height / 2) * this.width] = pos;

			//spread
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if (i == 0 && j == 0)
						continue;
					if (pos.x + i >= -width / 2 && pos.x + i < width / 2 && pos.y + j >= -height / 2 && pos.y + j < height / 2 && this.closestAccessible[(pos.x + i + width / 2) + (pos.y + j + height / 2) * this.width] == Vector2Int.zero && !this.IsObstacle(pos.x + i , pos.y + j))
						toCalculate.Enqueue(new Vector2Int(pos.x + i, pos.y + j));
				}
			}

		}

		this.closestAccessible[this.width / 2 + (this.height / 2) * this.width] = Vector2Int.zero; //set center


		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (this.closestAccessible[x + y * this.width] == Vector2Int.zero)
				{
					Vector2Int pos = new Vector2Int(x - width / 2, y - height / 2);
					Vector2Int toFind = pos;

					bool found = false;

					for(int h=1; h < Mathf.Max(width/2, height/2); h++)
					{
						for (int p = 0; p < 8 * h; p++)									//8*h is the perimeter of a square of h
						{
							if (p < 2 * h + 1)											// top of the perimeter
								toFind = new Vector2Int(pos.x - h + p, pos.y + h);
							else if (p < 4 * h + 1)                                     // right of the perimeter
								toFind = new Vector2Int(pos.x + h, pos.y + 3*h  - p);
							else if (p < 6 * h + 1)                                     // bottom of the perimeter
								toFind = new Vector2Int(pos.x + 5*h - p, pos.y - h);
							else														// left of the perimeter
								toFind = new Vector2Int(pos.x - h, pos.y - 7*h + p);

							if (toFind.x >= -width / 2 && toFind.x < width / 2 && toFind.y >= -height / 2 && toFind.y < height / 2 && !this.IsObstacle(toFind))
							{
								Vector2Int target = this.closestAccessible[(toFind.x + width / 2) + (toFind.y + height / 2) * this.width];
								if (target.x == toFind.x && target.y == toFind.y) //this is a tile accessible
								{
									found = true;
									break;
								}
							}
						}
						if (found)
							break;
					}

					this.closestAccessible[x + y * this.width] = toFind;
				}
			}
		}
	}



	#endregion

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getWalkWeight(int x, int y)
	{
		if (x < -this.width / 2 || x > this.width / 2 - 1 || y < -this.height / 2 || y > this.height / 2 - 1) return int.MaxValue;

		switch (this.terrainType[(x + width/2) + (y + height/2) * this.width])
		{
			case (char)0:
				return 500;
			case (char)1:
				return 5;
			case (char)2:
				return 1;
			default:
				return 5;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getWalkWeight(Vector2Int pos)
	{
		if (pos.x < -this.width / 2 || pos.x > this.width / 2 - 1 || pos.y < -this.height / 2 || pos.y > this.height / 2 - 1) return int.MaxValue;

		switch (this.terrainType[(pos.x + width / 2) + (pos.y + height / 2) * this.width])
		{
			case (char)0:
				return 500;
			case (char)1:
				return 5;
			case (char)2:
				return 1;
			default:
				return 5;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsObstacle(int x, int y)
	{
		return this.terrainType[(x + width / 2) + (y + height / 2) * this.width] < 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsInTerrain(Vector2Int v)
	{
		return v.x >= -this.width/2 && v.x < this.width/2 && v.y >= -this.height/2 && v.y < this.height/2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsObstacle(Vector2Int pos)
	{
		return this.terrainType[(pos.x + width / 2) + (pos.y + height / 2) * this.width] < 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2Int GetClosestAccessiblePos(Vector2Int pos)
	{
		return this.closestAccessible[(pos.x + width / 2) + (pos.y + height / 2) * this.width];
	}


}
