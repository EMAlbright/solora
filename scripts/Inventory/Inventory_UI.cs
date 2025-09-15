using Godot;
using System;

public partial class Inventory_UI : Control
{
	// array of slots
	private InventoryUiSlot[] _slots;
	// player inv class
	private Inventory _playerInventory;
	
	public override void _Ready() {
		Visible = false;
		
		var grid = GetNode<GridContainer>("NinePatchRect/SlotsGrid");
		_slots = new InventoryUiSlot[grid.GetChildCount()];
		for(int i = 0; i < grid.GetChildCount(); i++) {
			GD.Print("Looping (inventory)");
			_slots[i] = grid.GetChild(i) as InventoryUiSlot;
		}
		
		// get inventory node
		_playerInventory = GetParent().GetNode<Inventory>("Inventory");
		if (_playerInventory == null) {
			GD.PrintErr("Inventory node not found!");
		} else {
			GD.Print("Inventory node successfully referenced.");
		}
		
		// set signal to update UI from Inventory logic node
		_playerInventory.InventoryChange += Update;
	}
	
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("inventory")) {
			Visible = !Visible;
		}
	}
	
	public void Update() {
		int slotIndex = 0;
		foreach(var entry in _playerInventory.GetAllItems()) {
			if (slotIndex >= _slots.Length) break;
			
			_slots[slotIndex].SetItem(entry);
			slotIndex++;
		}
		
		// clear any empty lsots
		for(; slotIndex < _slots.Length; slotIndex++) {
			_slots[slotIndex].Clear();
		}
	}
}
