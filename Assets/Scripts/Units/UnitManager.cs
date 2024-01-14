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



	public static List<IDamageable> OverlapCircleUnitDamageable(Vector2 center, float radius, Team team = Team.ANY)
	{
		List<IDamageable> units = new List<IDamageable>();

		for (int x = (int)((center.x - radius) / UnitManager.chunckSize); x <= (int)((center.x + radius) / UnitManager.chunckSize); x++)
		{
			for (int y = (int)((center.y - radius) / UnitManager.chunckSize); y <= (int)((center.y + radius) / UnitManager.chunckSize); y++)
			{
				if (UnitManager.unitsInChunk.TryGetValue(new ChunkPos(x, y), out HashSet<Unit> unitsInChunck))
				{
					foreach (Unit u in unitsInChunck)
					{
						IDamageable damageable = u as IDamageable;
						if (Utils.SqrDistance(center, u.Position) <= radius * radius && (team == Team.ANY || u.Team == team) && damageable != null && !damageable.IsDead)
							units.Add(damageable);
					}
				}
			}
		}

		return units;
	}
	
}
