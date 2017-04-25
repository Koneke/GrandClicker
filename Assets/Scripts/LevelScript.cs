using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
	public Vector2 Size;

	public GameObject TilePlane;

	public GameObject Tile;

	public GameObject Walls;

	public GameObject Wall;

	public TileMap Map;

	void Awake()
	{
		this.Map = new TileMap(this, (int)this.Size.x, (int)this.Size.y);
	}

	void Start()
	{
	}

	void Update()
	{
	}

	// ==============================*/

	// Should move to TileMap really
	public TileScript SpawnTile(Position p)
	{
		if (!this.Map.InBounds(p))
		{
			return null;
		}

		// position
		var go = Instantiate(this.Tile);
		go.name = this.Tile.name;
		var ts = go.GetComponent<TileScript>().Init(this, p);
		ts.MoveTo(p);

		// put it in the hierarchy
		go.transform.parent = this.TilePlane.transform;

		// this really, *really* should move to TileMap...
		this.Map.SetTile(p, ts);
		this.Map.PullEdges(ts);
		this.Map.UpgradeEdges(ts);
		this.Map.Tiles.Add(ts);

		return ts;
	}

	public void RazeEdges(TileScript t)
	{
		// remove all loose edges
		foreach (var d in this.Map.Directions.Where(x => t.GetEdge(x).Opposite(t) == null))
		{
			this.Map.KillEdge(t.GetEdge(d));
		}

		// downgrade rest (but Indoor -> Outdoor, skipping an extra step)
		this.Map.DowngradeEdges(t);
	}

	// As should this
	public void DestroyTile(Position p)
	{
		if (!this.Map.InBounds(p))
		{
			return;
		}

		var t = this.Map.GetTile(p);

		this.RazeEdges(t);
		Destroy(t.gameObject);

		this.Map.SetTile(p, null);
	}

	// ==============================

	public void DestroyWall(Edge e)
	{
		UnityEngine.Object.Destroy(e.EdgeObject);
	}

	// game object wise, not model wise
	public GameObject SpawnWall(TileScript t, Direction d)
	{
		var w = Instantiate(this.Wall);
		var wt = w.transform;
		var center = t.transform.Find("Center");

		wt.position = center.position;
		wt.eulerAngles = new Vector3(wt.eulerAngles.x, ((int)d) * 90, wt.eulerAngles.z);

		// place into the hierarchy
		wt.parent = this.Walls.transform;

		t.GetEdge(d).EdgeObject = w;

		return w;
	}

	public void DrawWalls()
	{
		var edges = new List<Edge>();
		var visited = new List<TileScript>();
		var next = new List<TileScript>();

		var current = this.Map.Tiles[0];
		next.Add(current);

		return;

		while (next.Count > 0)
		{
			current = next[0];

			foreach (var e in current.Edges)
			{
				if (e != null && !edges.Contains(e))
				{
					edges.Add(e);
				}
			}

			visited.Add(current);

			next.AddRange(
				this.Map
					.GetRealNeighbours(current)
					.Where(n => !visited.Contains(n)));

			next.Remove(current);
		}
	}

	// ==============================

	private Position PositionFromHit(RaycastHit hit)
	{
		return new Position(
			(int)Math.Floor(hit.point.x),
			(int)Math.Floor(hit.point.z));
	}

	public void Hit(RaycastHit hit)
	{
		var p = this.PositionFromHit(hit);

		if (this.Map.HasTile(p))
		{
			this.DestroyTile(p);
		}
		else
		{
			var t = this.SpawnTile(p);

			//this.DrawWalls();
		}
	}

	// Right click hit
	public void Tih(RaycastHit hit)
	{
		var p = this.PositionFromHit(hit);
		var t = this.Map.GetTile(p);

		if (t == null)
		{
			return;
		}

		var feds = this.Map.GetFreeEdgeDirections(t);

		foreach (var fed in feds)
		{
			this.Map.CreateEdge(p, fed);
			Debug.Log(":)");
		}

		feds = this.Map.GetFreeEdgeDirections(t);
		Debug.Log("Now: " + feds.Count.ToString());
	}
}
