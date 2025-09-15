using Godot;
using System;
using System.Collections.Generic;
public partial class Sword : WeaponBase
{
	public override void Use(Player player) {
		base.Use(player);
		player.PlayerAnimation("Pierce", WeaponData.AttackDuration);
		base.OnAttackStart();
	}
}
