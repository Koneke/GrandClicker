using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallType
{
	Null, // just created
	Outdoor, // automagic
	None,
	Indoor
}

public class Edge
{
	private TileMap map;

	// Determined by most -x, then by most -z
	public TileScript First { get { return this.map.GetTile(F); } }
	public TileScript Second { get { return this.map.GetTile(S); } }

	public GameObject EdgeObject; // EdgeScript

	public Position F;
	public Position S;

	public WallType Wall;

	public bool IsPortal; // for doors and similar, later

	public Edge(TileMap map, Position f, Position s)
	{
		this.map = map;
		this.F = f;
		this.S = s;

		// weird way due to graphical/structure reasons
		//this.map.SetWall(this, WallType.Null);
		this.Wall = WallType.Null;
	}

	private void Reorder()
	{
		if (this.F != this.map.GetFirst(this.F, this.S))
		{
			var temp = this.F;
			this.F = this.S;
			this.S = temp;
		}
	}

	public TileScript Opposite(TileScript t)
	{
		return t == First
			? Second
			: First;
	}

	public void Unlink()
	{
		foreach (var t in new [] {
			this.map.GetTile(this.F),
			this.map.GetTile(this.S)})
		{
			if (t == null) continue;

			t.SetEdge(t.FindEdge(this), null);
		}
	}
}

public class EdgeScript : MonoBehaviour
{
	void Start()
	{
	}
	
	void Update()
	{
	}
}
