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
		for (int i = 0; i < grid.GetChildCount(); i++)
		{
			GD.Print("Looping (inventory)");
			_slots[i] = grid.GetChild(i) as InventoryUiSlot;
			if (_slots[i] != null)
			{
				_slots[i].InventorySlotIndex = i;
			}
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

	public void Update()
	{
		int slotIndex = 0;
		var invGrid = _playerInventory.GetInventoryGrid();

		for(int i = 0; i < invGrid.GetLength(1); i++){
			for(int j = 0; j < invGrid.GetLength(0); j++){
				// if slot reachable
				if(slotIndex < _slots.Length){

					var entry = invGrid[j,i];
					// set new item to null (empty) slot
					if(entry != null){
						_slots[slotIndex].SetItem(entry);
					}
					//TODO: CURRENTLY IF YOU PLACE ITEM IN SLOT OCCUPIED
					//THEN ITEM IS JUST CLEARED (not from inv, but visually)
					//This should probably just have nothing happen
					else {
						_slots[slotIndex].Clear();
					}
				}
				slotIndex++;
			}
		}
	}
}
