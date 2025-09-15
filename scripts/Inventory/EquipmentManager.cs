using Godot;

public static class EquipmentManager {
	public static Node2D EquipItem(BaseItem item, Node2D pivot, Player player, Node2D currentEquipped = null) {
		UnequipItem(currentEquipped);
		
		if(item == null || string.IsNullOrEmpty(item.ScenePath)) {
			GD.PrintErr("Invalid item/scene path");
		}
		GD.Print($"Hotbar go this item type: {item.DisplayName} -- {item.Type}");

		// if its a generic (only holdable) material, load holdable scene
		if (item.Type == ItemType.Mat) {
			var matscene = GD.Load<PackedScene>("res://scenes/HoldableGeneric.tscn");
			var matinstance = matscene.Instantiate<Node2D>();
			var matsprite = matinstance.GetNode<Sprite2D>("Sprite2D");
			matsprite.Texture = item.Icon;
			pivot.AddChild(matinstance);
			return matinstance;
		}
		
		var scene = GD.Load<PackedScene>(item.ScenePath);
		if (scene == null) {
			GD.PrintErr($"Scene not found at path: {item.ScenePath}");
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
