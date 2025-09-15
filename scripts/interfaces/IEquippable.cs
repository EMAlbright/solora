using Godot;
using System;

public interface IEquippable {
	void OnEquip(Player player, BaseItem item);
	void OnUnequip();
}
