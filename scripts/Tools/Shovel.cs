using Godot;
using System;

public partial class Shovel : ToolBase
{
	public override void Use(Player player) {
		base.Use(player);
		player.PlayerAnimation("Shovel", ToolData.AttackDuration);
	}
}
