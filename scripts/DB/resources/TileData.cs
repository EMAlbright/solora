using Godot;
using System;

public partial class TileSetData : Resource
{
	public int Id;
	public string Name;
	public bool Mineable;
	public float Hardness;
	public string DroppedItem;
	
	public TileSetData(string name, int id, float hardness, bool mineable, string di){
		Name = name;
		Id = id;
		Hardness = hardness;
		Mineable = mineable;
		DroppedItem = di;
	}
	
}
