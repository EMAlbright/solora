using Godot;
using System;

public partial class Pickaxe : ToolBase
{
	public override void Use(Player player) {
		base.Use(player);
		player.PlayerAnimation("Crush", ToolData.AttackDuration);
	}
}
