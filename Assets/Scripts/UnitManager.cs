using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class UnitManager
{
	public static readonly int chunckSize = 10;

	private static Dictionary<ChunkPos, HashSet<Unit>> unitsInChunk = new Dictionary<ChunkPos, HashSet<Unit>>();


	public static IReadOnlyCollection<Unit> GetUnitsInChunk(ChunkPos chunkPos)
	{
		return UnitManager.unitsInChunk.TryGetValue(chunkPos, out HashSet<Unit> e) ? e : null;
	}

	public static void OnUnitMove(Unit u, ChunkPos oldPos)
	{
		UnitManager.unitsInChunk[oldPos]?.Remove(u);
		if (!unitsInChunk.TryGetValue(u.ChunkPosition, out HashSet<Unit> hs))
		{
			hs = new HashSet<Unit>();
			unitsInChunk.Add(u.ChunkPosition, hs);
		}
		hs.Add(u);
	}

	public static void RegisterUnit(Unit u)
	{
		HashSet<Unit> hs = null;
		if (!unitsInChunk.TryGetValue(u.ChunkPosition, out hs))
		{
			hs = new HashSet<Unit>();
			unitsInChunk.Add(u.ChunkPosition, hs);
		}
		hs.Add(u);
	}

	
}
