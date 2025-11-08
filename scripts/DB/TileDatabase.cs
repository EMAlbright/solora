using Godot;
using System;

public partial class TileDatabase : Node {
	public static Dictionary<string, TileSetData> TileDB = new Dictionary<string, TileSetData>();
	
	public override void _Ready(){
		LoadTiles("res://scripts/DB/resources/GroundTiles.tres");
	}
	
	public void LoadTiles(string TileResourcePath){
		TileSet tileSet = GD.Load<TileSet>(TileResourcePath);
		
		for(var id in tileSet.GetTileIDs()){
			var data = tileSet.GetTileData(id);
			
			string name = (string)data.GetCustomData("name").AsString();
			float hardness = data.GetCustomData("hardness").AsSingle();
			bool mineable = data.GetCustomData("mineable").AsBool();
			string dropitem = data.GetCustomData("drop_item").AsString();
			
			
			TileDB[name] = new TileSetData(name, id, hardness, mineable, dropitem);
		}
	}
	
	public static TileSetData Get(string name){
		return TileDB.TryGetValue(name, out var tile) ? tile : null;
	}
}
