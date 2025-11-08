using Godot;
using System.Collections.Generic;

public class MiningManager
{
	private MineableLayer _currLayer;
	private MineableLayer _secondLayer;
	private TileMapLayer _crackLayer;
	
	// tile data is static, need to store per instance changes somewhere
	private List<MineableLayer> Layers = new();
	
	// MIGHT HAVE TO COMPLETELY SEPERATE UNDERGROUND AND SURFACE MANAGERS
	// SURFACE ONLY PASSES IN GROUND AND CRACKS, UNDER PASSES IN ORE ADDITIONALLY
	public MiningManager(TileMapLayer cracks, params MineableLayer[] minelayers)
	{
		// list of MineableLayers from top -> bottom
		// mountains -> ground -> underground -> ore
		Layers.AddRange(minelayers);
		_crackLayer = cracks;
	}
	
	public bool MineTile(Vector2I tilePos, ToolItem tool)
	{
		for (int i = 0;  i < Layers.Count;  i++)
		{
			if (Layers[i].Tilemap == null)
			{
				continue;
			}
			GD.Print(Layers[i].name);
			_currLayer = Layers[i];

			TileData tile = Layers[i].Tilemap.GetCellTileData(tilePos);
			TileData nextTile = null;

			if(i < Layers.Count-1 && Layers[i+1].Tilemap != null)
			{
				_secondLayer = Layers[i+1];
				nextTile = Layers[i+1].Tilemap.GetCellTileData(tilePos);
			}
			if (tile == null)
			{
				continue;
			}
			GD.Print(GetCustomDataString(tile, "name"));
			if (WorldManager.IsUnderground && nextTile != null && GetCustomDataBool(tile, "mineable") && GetCustomDataBool(nextTile, "mineable"))
			{
				return MineTwoTile(tilePos, tool, nextTile, tile);
			}
			if (GetCustomDataBool(tile, "mineable"))
			{
				return MineSingleTile(tilePos, true, tool, tile, Layers[i].Tilemap, Layers[i].Health);
			}
		}
		GD.Print("No mineable tile at ", tilePos);
		return false;
	}
	
	private bool MineSingleTile(Vector2I tilePos, bool isOre, ToolItem tool, TileData tile, TileMapLayer layer, Dictionary<Vector2I, float> healthDict) {
		float maxHealth = GetCustomDataFloat(tile, "hardness");
		
		if (!healthDict.ContainsKey(tilePos)) {
			healthDict[tilePos] = maxHealth;
		}
		healthDict[tilePos] -= tool.Damage;
		float remainingHealth = healthDict[tilePos];
		
		if(remainingHealth <= 0) {
			layer.SetCell(tilePos, -1);
			_crackLayer?.SetCell(tilePos, -1);
			healthDict.Remove(tilePos);
			// entrace no work
			WorldManager.AddEntrance(tilePos);
			SpawnDroppedItem(tilePos, tile);
		
			GD.Print($"Tile at {tilePos} removed.");
		}
		else if (isOre != true){
			int crackIndex = remainingHealth / maxHealth <= 0.33f ? 2 : 
			remainingHealth / maxHealth <= 0.66f ? 1 : 0;
			_crackLayer?.SetCell(tilePos, crackIndex, new Vector2I(0, 0));
		}
		return true;
	}
	
	private bool MineTwoTile(Vector2I tilePos, ToolItem tool, TileData oreTile, TileData groundTile) {
	float oreMaxHealth = GetCustomDataFloat(oreTile, "hardness");
	float groundMaxHealth = GetCustomDataFloat(groundTile, "hardness");
	
	// Initialize health if not already tracked
	if (!_secondLayer.Health.ContainsKey(tilePos)) {
		_secondLayer.Health[tilePos] = oreMaxHealth;
	}
	if (!_currLayer.Health.ContainsKey(tilePos)) {
		_currLayer.Health[tilePos] = groundMaxHealth;
	}
	
	// Damage both tiles
	_secondLayer.Health[tilePos] -= tool.Damage;
	_currLayer.Health[tilePos] -= tool.Damage;
	
	float oreRemainingHealth = _secondLayer.Health[tilePos];
	float groundRemainingHealth = _currLayer.Health[tilePos];
	
	// Check if both are destroyed
	if (oreRemainingHealth <= 0 && groundRemainingHealth <= 0) {
		// Remove both tiles
		_secondLayer.Tilemap.SetCell(tilePos, -1);
		_currLayer.Tilemap.SetCell(tilePos, -1);
		_crackLayer?.SetCell(tilePos, -1);
		
		// Clean up health tracking
		_secondLayer.Health.Remove(tilePos);
		_currLayer.Health.Remove(tilePos);
		
		// Create entrance and spawn ore drop
		WorldManager.AddEntrance(tilePos);
		SpawnDroppedItem(tilePos, oreTile);
		SpawnDroppedItem(tilePos, groundTile);
		
		GD.Print($"Both ore and ground tiles at {tilePos} removed.");
		return true;
	}
	
	// Show cracks based on the weakest tile (whichever is closest to breaking)
	float oreHealthPercent = oreRemainingHealth / oreMaxHealth;
	float groundHealthPercent = groundRemainingHealth / groundMaxHealth;
	float weakestPercent = groundHealthPercent;
	
	if (weakestPercent > 0) {
		int crackIndex = weakestPercent <= 0.33f ? 2 : 
						weakestPercent <= 0.66f ? 1 : 0;
		_crackLayer?.SetCell(tilePos, crackIndex, new Vector2I(0, 0));
	}
	
	return true;
	}
	
	private bool GetCustomDataBool(TileData tiledata, string name) {
		Variant data = tiledata.GetCustomData(name);
		GD.Print("hit tree");
		return data.VariantType != Variant.Type.Nil ? data.AsBool() : false;
	}

	private string GetCustomDataString(TileData tiledata, string name)
	{
		Variant data = tiledata.GetCustomData(name);
		return data.VariantType != Variant.Type.Nil ? data.AsString() : "";
	}

	private float GetCustomDataFloat(TileData tiledata, string name)
	{
		Variant data = tiledata.GetCustomData(name);
		return data.VariantType != Variant.Type.Nil ? data.AsSingle() : 0.0f;
	}
	
	
	private void SpawnDroppedItem(Vector2I tilePos, TileData tiledata) {
		var droppedItemScene = GD.Load<PackedScene>("res://scenes/dropped_item.tscn");
		string itemId = GetCustomDataString(tiledata, "drop_item");
		GD.Print($"Got {itemId} and trying to spawn");
		var instance = droppedItemScene.Instantiate<DroppedItem>();
		var itemData = ItemDatabase.GetItem(itemId);
		if (itemData == null) {
			GD.Print("Item data null in mining manager");
		}
		GD.Print(itemData.DisplayName);
		instance.Init(itemData);
		Vector2 worldPos = _currLayer.Tilemap.MapToLocal(tilePos);		
		instance.Position = worldPos;
		instance.GravityScale = 1;
		instance.zHeight = 0;
		instance.zVelocity = 0;
	
		_currLayer.Tilemap.GetParent().AddChild(instance);
		
	}
	
	public void SpawnBaseDroppedItem(Vector2I tilePos, BaseItem item){
		var droppedItemScene = GD.Load<PackedScene>("res://scenes/dropped_item.tscn");
		var instance = droppedItemScene.Instantiate<DroppedItem>();
		if (item == null) {
			GD.Print("Item data null in mining manager");
		}
		instance.Init(item);
		Vector2 worldPos = _currLayer.Tilemap.MapToLocal(tilePos);		
		instance.Position = worldPos;
		instance.GravityScale = 1;
		instance.zHeight = 0;
		instance.zVelocity = 0;
	
		_currLayer.Tilemap.GetParent().AddChild(instance);
	}
	
}
