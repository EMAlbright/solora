using Godot;
using System;
using System.Collections.Generic;

public partial class TileDatabase : Node {
	public static Dictionary<string, TileSetData> TileDB = new Dictionary<string, TileSetData>();
	
	public override void _Ready(){
		LoadTiles("res://scripts/DB/resources/GroundTiles.tres", "ground");
		LoadTiles("res://scripts/DB/resources/ObjectTiles.tres", "object");
		GD.Print("Tile database loaded");
	}
	
	public void LoadTiles(string TileResourcePath, string layername){
		TileSet tileSet = GD.Load<TileSet>(TileResourcePath);
		TileSetAtlasSource source = tileSet.GetSource(0) as TileSetAtlasSource;
		for(int i = 0; i < source.GetTilesCount(); i++){
			Vector2I coords = source.GetTileId(i);
			TileData data = source.GetTileData(coords, 0);
			
			string name = (string)data.GetCustomData("Name").AsString();
			float hardness = data.GetCustomData("hardness").AsSingle();
			bool mineable = data.GetCustomData("mineable").AsBool();
			string dropitem = data.GetCustomData("drop_item").AsString();
			string layer = layername;
			
			TileDB[name] = new TileSetData(name, coords, hardness, mineable, dropitem, layer);
		}
	}
	
	public static TileSetData Get(string name){
		return TileDB.TryGetValue(name, out TileSetData tileData) ? tileData : null;
	}
}
