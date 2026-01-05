using Godot;
using System;

public partial class TileSetData : Resource
{
	public Vector2I atlasCoords;
	public string Name;
	public bool Mineable;
	public float Hardness;
	public string DroppedItem;

	public string Layer;
	
	public TileSetData(string name, Vector2I coords, float hardness, bool mineable, string di, string layer){
		Name = name;
		atlasCoords = coords;
		Hardness = hardness;
		Mineable = mineable;
		DroppedItem = di;
		Layer = layer;
	}
	
}
