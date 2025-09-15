using Godot;
using System;

public partial class WorldObject : Node2D {
	protected Area2D _area;
	public int health = 100;

	public override void _Ready() {
		_area = GetNodeOrNull<Area2D>("Area2D");
		if (_area != null) {
			_area.BodyEntered += OnBodyEntered;
			_area.BodyExited += OnBodyExited;
		}
	}

	// Optional: for highlighting, interaction hints, some proximity detection.
	protected virtual void OnBodyEntered(Node body) {
	}

	protected virtual void OnBodyExited(Node body) {
	}

	// The main interface method for interactions (e.g. used by tools)
	public virtual void Interact(Player player) {
		GD.Print("Default WorldObject interacted with");
	}
	
	// override this for dorpping loot, base resource
	public virtual void OnDestroyed() {
		QueueFree();
	}
	
	public void TakeDamage(int amount) {
		if (health <= 0) {
			OnDestroyed();
		}
		health -= amount;
	}
}
