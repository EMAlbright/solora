using Godot;

public static class EquipmentManager {
	public static Node2D EquipItem(InventoryEntry item, Node2D pivot, Player player, Node2D currentEquipped = null) {
		UnequipItem(currentEquipped);
		
		if(item == null) {
			GD.Print("item null");
		}

		// if its a generic (only holdable) material, load holdable scene
		if (item.Item.Type == ItemType.Mat) {
			
			// this can be for non placeable mats
			//var matscene = GD.Load<PackedScene>("res://scenes/HoldableGeneric.tscn");
			//var matinstance = matscene.Instantiate<Node2D>();
			//var matsprite = matinstance.GetNode<Sprite2D>("Sprite2D");
			//matsprite.Texture = item.Icon;
			//for placeable mats
			var matscene = GD.Load<PackedScene>("res://scenes/placeable_block.tscn");
			var matinstance = matscene.Instantiate<StaticBody2D>();
			pivot.AddChild(matinstance);
			if (matinstance is IEquippable matequippable) {
				matequippable.OnEquip(player, item);
			}
			return matinstance;
		}
		
		var scene = GD.Load<PackedScene>(item.Item.ScenePath);
		if (scene == null) {
			GD.PrintErr($"Scene not found at path: {item.Item.ScenePath}");
			return null;
		}
		
		var instance = scene.Instantiate<Node2D>();
		pivot.AddChild(instance);
		
		if (instance is IEquippable equippable) {
			equippable.OnEquip(player, item);
		}
		
		return instance;
	}
	
	public static void UnequipItem(Node2D currentEquipped) {
		if (currentEquipped != null) {
			if(currentEquipped is IEquippable equippable) {
				equippable.OnUnequip();
			}
			currentEquipped.QueueFree();
			GD.Print("Unequipped item");
		}
	}
}
