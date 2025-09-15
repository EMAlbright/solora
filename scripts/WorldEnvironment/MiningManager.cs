using Godot;
using System.Collections.Generic;

public class MiningManager
{
	private TileMapLayer _groundLayer;
	private TileMapLayer _oreLayer;
	private TileMapLayer _crackLayer;
	private bool hasGround;
	private bool hasOre;
	private TileData groundTile;
	private TileData oreTile;
	
	// tile data is static, need to store per instance changes somewhere
	private Dictionary<Vector2I, float> _groundHealth = new();
	private Dictionary<Vector2I, float> _oreHealth = new();
	
	// MIGHT HAVE TO COMPLETELY SEPERATE UNDERGROUND AND SURFACE MANAGERS
	// SURFACE ONLY PASSES IN GROUND AND CRACKS, UNDER PASSES IN ORE ADDITIONALLY
	public MiningManager(TileMapLayer ground, TileMapLayer cracks, TileMapLayer ore = null) {
		_groundLayer = ground;
		_oreLayer = ore;
		_crackLayer = cracks;
	}
	
	public bool MineTile(Vector2I tilePos, ToolItem tool)
	{
		if (_oreLayer != null) {
			oreTile = _oreLayer.GetCellTileData(tilePos);
			hasOre = oreTile != null && GetCustomDataBool(oreTile, "mineable");
		}
		
		if(_groundLayer != null) {
			groundTile = _groundLayer.GetCellTileData(tilePos);
			hasGround = groundTile != null && GetCustomDataBool(groundTile, "mineable");
		}
		
		if (hasOre && hasGround) {
			return MineTwoTile(tilePos, tool, oreTile, groundTile);
		}
		else if (hasOre) {
			return MineSingleTile(tilePos, true, tool, oreTile, _oreLayer, _oreHealth);
		}
		
		else if (hasGround) {
			return MineSingleTile(tilePos, false, tool, groundTile, _groundLayer, _groundHealth);
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
	if (!_oreHealth.ContainsKey(tilePos)) {
		_oreHealth[tilePos] = oreMaxHealth;
	}
	if (!_groundHealth.ContainsKey(tilePos)) {
		_groundHealth[tilePos] = groundMaxHealth;
	}
	
	// Damage both tiles
	_oreHealth[tilePos] -= tool.Damage;
	_groundHealth[tilePos] -= tool.Damage;
	
	float oreRemainingHealth = _oreHealth[tilePos];
	float groundRemainingHealth = _groundHealth[tilePos];
	
	// Check if both are destroyed
	if (oreRemainingHealth <= 0 && groundRemainingHealth <= 0) {
		// Remove both tiles
		_oreLayer.SetCell(tilePos, -1);
		_groundLayer.SetCell(tilePos, -1);
		_crackLayer?.SetCell(tilePos, -1);
		
		// Clean up health tracking
		_oreHealth.Remove(tilePos);
		_groundHealth.Remove(tilePos);
		
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
		Vector2 worldPos = _groundLayer.MapToLocal(tilePos);		
		instance.Position = worldPos;
		instance.GravityScale = 1;
		instance.zHeight = 0;
		instance.zVelocity = 0;
	
	
		_groundLayer.GetParent().AddChild(instance);
		
	}
	
}
