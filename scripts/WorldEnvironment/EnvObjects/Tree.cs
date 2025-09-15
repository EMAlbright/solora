using Godot;
using System;

public partial class Tree : WorldObject, IMineable
{
	public override void OnDestroyed() {
		DropItems();
		base.OnDestroyed();
	}
	
	private void DropItems() {
		GD.Print("Tree dropped wood");
	}
	
	public void Mine(int amount) {
		TakeDamage(amount);
	}
}
