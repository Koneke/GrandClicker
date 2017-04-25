using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class TileMap
{
	public Vector2 Size;

	private TileScript[,] Map;

	public LevelScript Level;

	public List<Direction> Directions = new List<Direction>()
	{
		Direction.South,
		Direction.East,
		Direction.North,
		Direction.West
	};

	public List<TileScript> Tiles;

	// ==============================

	public void SetWall(Edge e, WallType wall)
	{
		// graphical stuff
		var o = e.Wall;
		var n = wall;

		if (o != WallType.None && n == WallType.None)
		{
			this.Level.DestroyWall(e);
		}
		else if ((o == WallType.None || o == WallType.Null) && n != WallType.None)
		{
			var t = this.GetTile(e.F); // or e.S, doesn't matter
			this.Level.SpawnWall(t, t.FindEdge(e));
		}

		// actual model change
		e.Wall = wall;
	}

	// yo notice how this *OVERWRITES* the other edge if there is one
	// --
	// maybe throw something on not-null edge?
	// and just be not dumb when using it?
	public Edge CreateEdge(Position p, Direction d)
	{
		var t = this.GetTile(p);
		var e = new Edge(
			this.Level.Map, p,
			this.GetPositionInDirection(p, d));

		t.SetEdge(d, e);

		var o = this.GetTileInDirection(t, d);
		if (o != null)
		{
			o.SetEdge(Utils.Invert(d), e);
		}

		return e;
	}

	// when we spawn a tile
	public void PullEdges(TileScript ts)
	{
		foreach (var d in this.Directions)
		{
			var n = this.GetTileInDirection(ts, d);
			if (n == null) continue;

			var ne = n.GetEdge(Utils.Invert(d));

			ts.SetEdge(d, ne);
		}
	}

	public void KillEdge(Edge e)
	{
		e.Unlink();
		this.Level.DestroyWall(e);
	}

	// null -> outdoor -> none -> indoor
	// (only after pull edges btw)

	public void UpgradeEdges(TileScript t)
	{
		foreach (var d in this.Directions)
		{
			var e = t.GetEdge(d);
			if (e == null)
			{
				e = this.CreateEdge(t.Position, d);
				t.SetEdge(d, e);
			}

			var wt = (WallType)((int)e.Wall + 1);
			this.SetWall(e, (WallType)((int)e.Wall + 1));
		}
	}

	// indoor -2-> none -> outdoor -> null
	// (down indoor twice)

	public void DowngradeEdges(TileScript t)
	{
		foreach (var d in this.Directions)
		{
			var e = t.GetEdge(d);
			if (e == null) continue;

			switch(e.Wall)
			{
				case WallType.Indoor:
				case WallType.None:
					this.SetWall(e, WallType.Outdoor);
					break;

				case WallType.Outdoor:
					this.KillEdge(e);
					break;
			}
		}
	}

	// Edges on tile t, shared with a null tile
	// (e.g. prime for pruning when we remove tiles)
	public List<Edge> GetLooseEdges(TileScript t)
	{
		var loose = new List<Edge>();
		var ns = this.GetFreeNeighbours(t);

		foreach (var n in ns)
		{
			var e = this.GetEdge(t, n);

			if (e != null)
			{
				loose.Add(e);
			}
		}

		return loose;
	}

	public List<Direction> GetFreeEdgeDirections(TileScript t)
	{
		var free = new List<Direction>();

		foreach (var d in this.Directions)
		{
			if (t.GetEdge(d) == null)
			{
				free.Add(d);
			}
		}

		return free;
	}

	public Edge GetEdge(TileScript a, TileScript b)
	{
		// should always be -1, 0 or 1
		var dz = b.Position.z - a.Position.z;
		var dx = b.Position.x - a.Position.x;

		return a.Edges[(int)this.DirectionFromDeltas(dx, dz)];
	}

	// ==============================

	public Direction DirectionFromDeltas(int dx, int dz)
	{
		return dz == 0
			? dx == -1
				? Direction.West
				: Direction.East
			: dz == -1
				? Direction.North
				: Direction.South;
	}

	public Position DeltasFromDirection(Direction direction)
	{
		var d = (int)direction;
		var x = Math.Sign(d % 2) * (2 - d);
		var z = Math.Sign(++d % 2) * (2 - d);

		return new Position(x, z);
	}

	// ==============================

	public Position GetFirst(Position a, Position b)
	{
		return (a.x == b.x
			? (a.z == b.z 
				? a 
				: (a.z < b.z 
					? a 
					: b))
			: (a.x < b.x 
				? a 
				: b));
	}

	public Position GetOther(Position a, Position b)
	{
		return a == this.GetFirst(a, b)
			? b
			: a;
	}

	// ==============================

	public List<TileScript> GetFreeNeighbours(TileScript t)
	{
		return this.GetNeighbours(t)
			.Where(n => n == null)
			.ToList();
	}

	public List<TileScript> GetRealNeighbours(TileScript t)
	{
		return this.GetNeighbours(t)
			.Where(n => n != null)
			.ToList();
	}

	// ==============================

	public Position GetPositionInDirection(Position p, Direction direction)
	{
		var deltas = this.DeltasFromDirection(direction);
		var x = p.x + deltas.x;
		var z = p.z + deltas.z;

		return new Position(x, z);
	}

	public Position GetPositionInDirection(TileScript source, Direction direction)
	{
		return this.GetPositionInDirection(source.Position, direction);
	}

	public TileScript GetTileInDirection(TileScript source, Direction direction)
	{
		return this.GetTile(this.GetPositionInDirection(source, direction));
	}

	public T GetInDirection<T>(TileScript source, Direction direction) where T : class
	{
		return null;
	}

	// ==============================

	public List<Position> GetNeighbourPositions(TileScript t)
	{
		return this.GetNeighboursBy<Position>(this.GetPositionInDirection, t);
	}

	public List<TileScript> GetNeighbours(TileScript t)
	{
		return this.GetNeighboursBy<TileScript>(this.GetTileInDirection, t);
	}

	private List<T> GetNeighboursBy<T>(Func<TileScript, Direction, T> f, TileScript t)
	{
		var neighbours = new List<T>();

		//for (var d = 0; d < 4; d++)
		foreach (var d in this.Directions)
		{
			neighbours.Add(f(t, d));
		}

		return neighbours;
	}

	// ==============================

	public bool InBounds(int x, int z)
	{
		return
			x >= 0 && x < this.Size.x &&
			z >= 0 && z < this.Size.y;
	}

	public bool InBounds(Position p)
	{
		return this.InBounds(p.x, p.z);
	}

	// ==============================

	public void SetTile(int x, int z, TileScript t)
	{
		if (this.InBounds(x, z))
		{
			this.Map[x, z] = t;
		}
	}

	public void SetTile(Position p, TileScript t)
	{
		this.SetTile(p.x, p.z, t);
	}

	public TileScript GetTile(int x, int z)
	{
		return this.InBounds(x, z)
			? this.Map[x, z]
			: null;
	}

	public TileScript GetTile(Position p)
	{
		return this.GetTile(p.x, p.z);
	}

	public bool HasTile(int x, int z)
	{
		return this.InBounds(x, z)
			? this.Map[x, z] != null
			: false;
	}

	public bool HasTile(Position p)
	{
		return this.HasTile(p.x, p.z);
	}
	
	public TileMap(LevelScript level, int x, int z)
	{
		this.Level = level;
		this.Map = new TileScript[x, z];
		this.Size = new Vector2(x, z);
		this.Tiles = new List<TileScript>();
	}
}
