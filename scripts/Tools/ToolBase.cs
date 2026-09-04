using Godot;
using System;

public partial class ToolBase : Node2D, IEquippable, IUsable
{
	public enum UseAnimationType {
    None,
    Shovel,
    Mine,
    Chop
	}
	public virtual UseAnimationType UseAnimation => UseAnimationType.None;
	public ToolItem ToolData;
	private Sprite2D _sprite;
	private bool _isUsingTool = false;
	private Player _player;
	
	public override void _Ready() {
		Area2D _toolArea = GetNode<Area2D>("tool_area2d");
		_toolArea.AreaEntered += OnAreaEntered;
	}
	
	public virtual void Use(UseContext context) {
		if(ToolData == null || _isUsingTool)
		{
			return;
		}

		_isUsingTool = true;
		
		UseOnTile(context);
		
		FinishUseAfter(ToolData.AttackDuration);
	}

	protected void UseOnTile(UseContext context)
	{
		Vector2 targetWorldPosition = context.Origin + context.Direction * 16f;
		Vector2I tile = WorldManager.WorldToTilePos(targetWorldPosition);
		WorldManager.Mining.MineTile(tile, ToolData);

	}

	private async void FinishUseAfter(float duration)
	{
		await ToSignal(
			GetTree().CreateTimer(duration),
			SceneTreeTimer.SignalName.Timeout
		);
		_isUsingTool = false;

	}
	
	
	public virtual void OnEquip(EquipmentContext context) {
		Owner = context.Owner;

		ToolData = context.Entry.Item as ToolItem;

		if (ToolData == null)
		{
			GD.PushError("Non ToolItem equipped");
		}
		Show();
	}
	
	public virtual void OnUnequip() {
		_isUsingTool = false;
		Hide();

		Owner = null;
		ToolData = null;
	}
	
	// check if area the Tools hitbox (area2d) has entered is a mineable area
	private void OnAreaEntered(Area2D area) {
		// dont take damage from just walking around
		if (!_isUsingTool || ToolData == null) {
			return;
		}
		
		if (area.GetParent() is IMineable mineable) {
			mineable.Mine(ToolData.Damage);
			GD.Print($"hit for {ToolData.Damage} by tool");
		}
	}

}
