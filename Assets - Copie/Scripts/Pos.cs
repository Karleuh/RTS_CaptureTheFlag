using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkPos
{
	int x;
	int y;

	public ChunkPos(int x, int y)
	{
		this.x = x;
		this.y = y;
	}




	public ChunkPos DivideAndFloor(float b)
	{
		return new ChunkPos((int)(this.x / b), (int)(this.y / b));
	}




	public override bool Equals(object obj)
	{
		if (!(obj is ChunkPos))
			return false;

		ChunkPos p = (ChunkPos)obj;
		return this.x == p.x && this.y == p.y;
	}

	public override int GetHashCode()
	{
		return this.x.GetHashCode() ^ (this.y.GetHashCode() << 2);
	}

	public static bool operator ==(ChunkPos a, ChunkPos b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public static bool operator !=(ChunkPos a, ChunkPos b)
	{
		return a.x != b.x || a.y != b.y;
	}


	public static ChunkPos operator +(ChunkPos a, ChunkPos b)
	{
		return new ChunkPos(a.x + b.x, a.y + b.y);
	}

	public static ChunkPos operator -(ChunkPos a, ChunkPos b)
	{
		return new ChunkPos(a.x - b.x, a.y - b.y);
	}

	public static ChunkPos operator +(ChunkPos a)
	{
		return a;
	}

	public static ChunkPos operator -(ChunkPos a)
	{
		return new ChunkPos(-a.x, -a.y);
	}

	public static ChunkPos operator *(ChunkPos a, int b)
	{
		return new ChunkPos(a.x * b, a.y * b);
	}


	public static Vector3 operator *(ChunkPos a, float b)
	{
		return new Vector3(a.x * b, 0, a.y * b);
	}


	public static Vector3 operator /(ChunkPos a, float b)
	{
		return new Vector3(a.x / b, 0, a.y / b);
	}

	public static implicit operator Vector3(ChunkPos a)
	{
		return new Vector3(a.x, 0, a.y);
	}

	public static explicit operator ChunkPos(Vector3 a)
	{
		return new ChunkPos((int)a.x, (int)a.z);
	}


	public ChunkPos(Vector2 a, float b)
	{
		this.x = (int)(a.x / b);
		this.y = (int)(a.y / b);
	}
}
