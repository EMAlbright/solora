using Godot;
using System.Collections.Generic;

public class MineableLayer
{
	public bool HasCracks;
	public Dictionary<Vector2I, float> Health = new();
	public TileMapLayer Tilemap;
	public string name;
	
	public MineableLayer(TileMapLayer tml, bool hasCracks = false, string tm = ""){
		Tilemap = tml;
		HasCracks = hasCracks;
		name = tm;
	}
	
}
