using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Utils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SqrDistance(Vector2 a, Vector2 b)
	{
		return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SqrDistance(Vector2Int a, Vector2 b)
	{
		return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SqrDistance(Vector2 a, Vector2Int b)
	{
		return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SqrDistance(Vector2Int a, Vector2Int b)
	{
		return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
	}
}
