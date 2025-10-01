using Godot;
using System;

public partial class ToolBase : Node2D, IEquippable, IUsable
{
	public ToolItem ToolData;
	private Sprite2D _sprite;
	private bool _isUsingTool = false;
	private Player _player;
	
	public override void _Ready() {
		_sprite = GetNode<Sprite2D>("ToolSprite");
		var area = GetNode<Area2D>("tool_area2d");
		area.AreaEntered += OnAreaEntered;
	}
	
	public virtual void Use(Player player) {
		GD.Print("Override tool base animation");
		_isUsingTool = true;
		
		// get tile pos from game manager
		Vector2I tilePos = WorldManager.WorldToTilePos(player.GlobalPosition);
		WorldManager.Mining.MineTile(tilePos + player.FacingDirection, ToolData);
		
		GetTree().CreateTimer(ToolData.AttackDuration).Timeout += () => {
			_isUsingTool = false;
		};
	}
	
	
	public void OnEquip(Player player, BaseItem item) {
		_player = player;
		ToolData = item as ToolItem;
		Show();
	}
	
	public void OnUnequip() {
		GD.Print("Unequip");
	}
	
	// check if area the Tools hitbox (area2d) has entered is a mineable area
	private void OnAreaEntered(Area2D area) {
		// dont take damage from just walking around
		if (!_isUsingTool) {
			return;
		}
		
		if (area.GetParent() is IMineable mineable) {
			mineable.Mine(ToolData.Damage);
			GD.Print($"hit for {ToolData.Damage} by tool");
		}
	}

}
