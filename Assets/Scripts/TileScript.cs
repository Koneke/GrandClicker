using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
	public const float MagicYConstant = 0.05f;

	public LevelScript level;

	public Edge[] Edges;

	public Position Position;

	public TileScript Init(LevelScript level, Position p)
	{
		this.Edges = new Edge[4];
		this.level = level;
		this.Position = new Position(p);
		return this;
	}

	void Start()
	{
	}

	void Update()
	{
	}

	public Direction GetDirection(Edge edge)
	{
		var i = this.Edges.ToList().IndexOf(edge);

		if (i < 0)
		{
			throw new ArgumentException();
		}

		return (Direction)i;
	}

	public Direction FindEdge(Edge e)
	{
		for (var i = 0; i < 4; i++)
		{
			if (this.Edges[i] == e)
			{
				return (Direction)i;
			}
		}

		return (Direction)(-1);
	}

	public Edge GetEdge(Direction direction)
	{
		return this.Edges[(int)direction];
	}

	public void SetEdge(Direction direction, Edge e)
	{
		this.Edges[(int)direction] = e;
	}

	public TileScript GetTileAcrossEdge(Direction direction)
	{
		return this.level.Map.GetTileInDirection(this, direction);
	}

	public void MoveTo(Position p)
	{
		var lp = this.transform.localPosition;

		lp.x = p.x;
		lp.z = p.z;
		lp.y = MagicYConstant;

		this.Position = p;

		this.transform.localPosition = lp;
	}
}
