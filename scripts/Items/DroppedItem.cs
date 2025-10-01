using Godot;
using System;

public partial class DroppedItem : RigidBody2D
{
	public BaseItem ItemData { get; private set; }
	private Sprite2D _icon;
	
	public float zHeight;
	public float zVelocity;

	public override void _Ready() {
		_icon = GetNode<Sprite2D>("Sprite2D");
		if(_icon == null) {
			GD.Print("Null icon");
		}
		UpdateIcon();
		if (WorldManager._isUnderground) {
			CollisionMask = 2;
			GravityScale = 1;      
			zHeight = 0;           
			zVelocity = 0;
			_icon.Position = Vector2.Zero;
		}
		else {
			CollisionMask = 1;
			GravityScale = 0;
			if (zHeight <= 0) {
				DropItemFromHeight(40f, 0f); 
		}	
		
	}
	}
	
	public override void _PhysicsProcess(double delta) {
		if (zHeight > 0) {
		zVelocity -= 40f * (float)delta; 
		zHeight += zVelocity * (float)delta;
			if (zHeight <= 0) {
				zHeight = 0;
				zVelocity = 0;
			}
		}
		if (_icon != null) {
			_icon.Position = new Vector2(0, -zHeight);
		}
	}
	
	public void DropItemFromHeight(float dropHeight, float v) {
		zHeight = dropHeight;
		zVelocity = v;
	}

	public void Init(BaseItem item) {
		ItemData = item;
		UpdateIcon();
	}

	private void UpdateIcon() {
		if (_icon == null)
			return;  

		if (ItemData != null && ItemData.Icon != null) {
			_icon.Texture = ItemData.Icon;
			_icon.Scale = new Vector2(ItemData.WorldScale, ItemData.WorldScale);	
		}
		else {
			_icon.Texture = null;
		}
	}

	private void _on_pickup_area_body_entered(Node2D body) {
		// make it so always detecting physics when area is in it
		Sleeping = false;
		if (body is Player player) {
			
			// try to add to hotbar
			var hotbar = player.GetNode<Hotbar>("Hotbar");
			if (hotbar.AddItem(ItemData, 1)) {
				GD.Print($" Added {ItemData.DisplayName} to hotbar");
			}
			else {
				player.Inventory.AddItem(ItemData);
				// add to inventory if htobar full
				GD.Print($"Added {ItemData.DisplayName} to inventory");
			}
			QueueFree();
		}
	}
}
