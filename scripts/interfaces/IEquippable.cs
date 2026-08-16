using Godot;
using System;

public interface IEquippable {
	void OnEquip(EquipmentContext context);
	void OnUnequip();
}
