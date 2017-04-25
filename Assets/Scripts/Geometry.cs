using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
	public static Direction Invert(Direction direction)
	{
		var d = (int)direction;
		d = (d + 2) % 4;

		return (Direction)d;
	}
}

public class Position
{
	public int x;
	public int z;

	public Position(int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	public Position(Position p)
	{
		this.x = p.x;
		this.z = p.z;
	}

	public Position(Vector2 v)
	{
		this.x = (int)v.x;
		this.z = (int)v.y;
	}

	public static bool operator ==(Position a, Position b)
	{
		return a.x == b.x && a.z == b.z;
	}

	public static bool operator !=(Position a, Position b)
	{
		return a.x != b.x || a.z != b.z;
	}

	public override bool Equals(object o)
	{
		if (!(o is Position))
		{
			return false;
		}

		return this == (Position)o;
	}
}

public enum Direction
{
	South, // +z	0
	East,  // +x	1
	North, // -z	2
	West   // -x	3
}
