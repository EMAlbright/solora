using Godot;
using System;

public interface IEquippable {
	void OnEquip(Player player, InventoryEntry item);
	void OnUnequip();
}
