using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class AStar
{
	public static void A_Star(Vector2 start, Vector2 goal, ICollection<Vector2Int> result)
	{

		// The set of discovered nodes that may need to be (re-)expanded.
		// Initially, only the start node is known.
		// This is usually implemented as a min-heap or priority queue rather than a hash-set.
		HashSet<Vector2Int> openSet = new HashSet<Vector2Int> { Vector2Int.FloorToInt(start) };

		// For node n, cameFrom[n] is the node immediately preceding it on the cheapest path from the start
		// to n currently known.

		Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

		// For node n, gScore[n] is the cost of the cheapest path from start to n currently known.
		Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>(); //default values of Infinity
		gScore[Vector2Int.FloorToInt(start)] = 0;

		// For node n, fScore[n] := gScore[n] + h(n). fScore[n] represents our current best guess as to
		// how cheap a path could be from start to finish if it goes through n.
		Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>(); //default values of Infinity  
		fScore[Vector2Int.FloorToInt(start)] = Mathf.Abs(goal.x - start.x) + Mathf.Abs(goal.y - start.y); //TODO use priority queue

		bool foundSmallerFScore = false;
		Vector2Int smallerFScore = default;

		int chibre = 0;

		while (openSet.Count > 0)
		{
			chibre += 1;
			// This operation can occur in O(Log(N)) time if openSet is a min-heap or a priority queue
			Vector2Int current = Vector2Int.zero;
			float minFScore = float.MaxValue;
			if (!foundSmallerFScore)
			{
				foreach (Vector2Int i in openSet)
				{
					float score = fScore[i];
					if (score < minFScore)
					{
						minFScore = score;
						current = i;
					}
				}
			}
			else
			{
				current = smallerFScore;
				foundSmallerFScore = false;
				minFScore = fScore[current];
			}

			if (chibre > 10000)
			{
				Debug.Log("BUG : " + current + "  " + goal);
				Debug.Log("BUG : " + current + "  " + goal);
				return;
			}
			//return
			if (current == Vector2Int.FloorToInt(goal))
			{
				//List<Vector2Int> result = new List<Vector2Int>();
				SmoothAndAssemblePath(cameFrom, current, result);
				return;

			}
			//return reconstruct_path(cameFrom, current);


			openSet.Remove(current);

			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if (i == 0 && j == 0)
						continue;

					Vector2Int neighbor = current + new Vector2Int(i, j);

					// d(current,neighbor) is the weight of the edge from current to neighbor
					// tentative_gScore is the distance from start to the neighbor through current
					float gscoreCurrent = gScore.TryGetValue(current, out float gs) ? gs : float.MaxValue;
					float gscoreNeighbor = gScore.TryGetValue(neighbor, out float gs2) ? gs2 : float.MaxValue;
					float tentative_gScore = gscoreCurrent + Terrain.instance.getWalkWeight(neighbor.x, neighbor.y);

					if (tentative_gScore < gscoreNeighbor)
					{
						float newFScore = tentative_gScore + Vector2.Distance(goal, neighbor);
						// This path to neighbor is better than any previous one. Record it!
						cameFrom[neighbor] = current;
						gScore[neighbor] = tentative_gScore;
						fScore[neighbor] = newFScore;
						openSet.Add(neighbor);

						if (newFScore <= minFScore)                 //optimisation : if the new fscore is smaller than the parent one, we skip the search
						{
							foundSmallerFScore = true;
							smallerFScore = neighbor;
							minFScore = newFScore;
						}

					}

				}
			}
		}

		return;
	}


	/**
	 *	Smooth the path by removing unnecessary checkpoints 
	 */
	public static void SmoothAndAssemblePath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int goal, ICollection<Vector2Int> result)
	{
		Vector2Int checkPoint = goal;
		Vector2Int current = goal;
		Vector2Int next;
		while (cameFrom.TryGetValue(current, out next))
		{
			if (!IsLineWalkable(checkPoint, next))
			{
				result.Add(current);
				checkPoint = current;
			}
			current = next;
		}
		result.Add(current);

	}

	public static void SmoothPath(SimpleConcatLinkedList<Vector2Int> path)
	{
		if (path.Count <= 2)
			return;

		SimpleConcatLinkedListNode<Vector2Int> next = path.Last;
		SimpleConcatLinkedListNode<Vector2Int> current = next.Prev;

		while (current != path.First)
		{
			if (IsLineWalkable(next.Value, current.Prev.Value))
			{
				path.Remove(current, next);
				current = current.Prev;
			}
			else
			{
				next = current;
				current = next.Prev;
			}
		}

	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLineWalkable(Vector2Int from, Vector2Int to)
	{
		Vector2 currentPoint = from;
		Vector2 dir = ((Vector2)(to - from)).normalized;
		Vector2Int floorToInt = Vector2Int.FloorToInt(currentPoint);

		while (Vector2.Dot(to - currentPoint, dir) > 0)
		{
			currentPoint += dir * 0.1f;
			floorToInt = Vector2Int.FloorToInt(currentPoint);
			if (Terrain.instance.getWalkWeight(floorToInt) > Terrain.instance.getWalkWeight(from))
				return false;
		}


		return true;
	}




	private class AStarNode : IComparable<AStarNode>
	{
		public Vector2Int Node { get; set; }
		public Vector2Int CameFrom { get; set; }
		public float GScore { get; set; }
		public float FScore { get; set; }

		public int CompareTo(AStarNode other)
		{
			return this.FScore.CompareTo(other);
		}
	}


	public static void A_Star_New(Vector2 start, Vector2 goal, ICollection<Vector2Int> result)
	{

		// The set of discovered nodes that may need to be (re-)expanded.
		// Initially, only the start node is known.

		HashSet<Vector2Int> openSet = new HashSet<Vector2Int> { Vector2Int.FloorToInt(start) };

		// For node n, cameFrom[n] is the node immediately preceding it on the cheapest path from the start
		// to n currently known.

		Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

		// For node n, gScore[n] is the cost of the cheapest path from start to n currently known.
		Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>(); //default values of Infinity
		gScore[Vector2Int.FloorToInt(start)] = 0;

		// For node n, fScore[n] := gScore[n] + h(n). fScore[n] represents our current best guess as to
		// how cheap a path could be from start to finish if it goes through n.
		Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>(); //default values of Infinity  
		fScore[Vector2Int.FloorToInt(start)] = Mathf.Abs(goal.x - start.x) + Mathf.Abs(goal.y - start.y); //TODO use priority queue

		bool foundSmallerFScore = false;
		Vector2Int smallerFScore = default;

		int chibre = 0;

		while (openSet.Count > 0)
		{
			chibre += 1;
			// This operation can occur in O(Log(N)) time if openSet is a min-heap or a priority queue
			Vector2Int current = Vector2Int.zero;
			float minFScore = float.MaxValue;
			if (!foundSmallerFScore)
			{
				foreach (Vector2Int i in openSet)
				{
					float score = fScore[i];
					if (score < minFScore)
					{
						minFScore = score;
						current = i;
					}
				}
			}
			else
			{
				current = smallerFScore;
				foundSmallerFScore = false;
				minFScore = fScore[current];
			}

			if (chibre > 10000)
			{
				Debug.Log("BUG : " + current + "  " + goal);
				Debug.Log("BUG : " + current + "  " + goal);
				return;
			}
			//return
			if (current == Vector2Int.FloorToInt(goal))
			{
				//List<Vector2Int> result = new List<Vector2Int>();
				SmoothAndAssemblePath(cameFrom, current, result);
				return;

			}
			//return reconstruct_path(cameFrom, current);


			openSet.Remove(current);

			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if (i == 0 && j == 0)
						continue;

					Vector2Int neighbor = current + new Vector2Int(i, j);

					// d(current,neighbor) is the weight of the edge from current to neighbor
					// tentative_gScore is the distance from start to the neighbor through current
					float gscoreCurrent = gScore.TryGetValue(current, out float gs) ? gs : float.MaxValue;
					float gscoreNeighbor = gScore.TryGetValue(neighbor, out float gs2) ? gs2 : float.MaxValue;
					float tentative_gScore = gscoreCurrent + Terrain.instance.getWalkWeight(neighbor.x, neighbor.y);

					if (tentative_gScore < gscoreNeighbor)
					{
						float newFScore = tentative_gScore + Vector2.Distance(goal, neighbor);
						// This path to neighbor is better than any previous one. Record it!
						cameFrom[neighbor] = current;
						gScore[neighbor] = tentative_gScore;
						fScore[neighbor] = newFScore;
						openSet.Add(neighbor);

						if (newFScore <= minFScore)                 //optimisation : if the new fscore is smaller than the parent one, we skip the search
						{
							foundSmallerFScore = true;
							smallerFScore = neighbor;
							minFScore = newFScore;
						}

					}

				}
			}
		}

		return;
	}


}
