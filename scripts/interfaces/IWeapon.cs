using Godot;
using System;

public interface IWeapon {
	void LightAttack(Player player);
	void HeavyAttack(Player player);
}
