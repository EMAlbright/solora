using Godot;
using System;

public partial class Inventory_UI : Control
{
	// array of slots
	private InventoryUiSlot[] _slots;
	// player inv class
	private Inventory _inventory;
	
	public override void _Ready() {

		Visible = false;
		
		GridContainer grid = GetNode<GridContainer>("NinePatchRect/SlotsGrid");

		_slots = new InventoryUiSlot[grid.GetChildCount()];

		for (int i = 0; i < _slots.Length; i++)
		{
			_slots[i] = grid.GetChild<InventoryUiSlot>(i);

			_slots[i].InventorySlotIndex = i;

		}
		
		// get inventory node
		_inventory = GetParent().GetNode<Inventory>("Inventory");
		
		_inventory.InventoryChange += UpdateItems;
		
		UpdateItems();
	}
	
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("inventory")) {
			Visible = !Visible;
		}
	}

	public void UpdateItems()
	{
		for(int i = 0; i <_slots.Length; i++)
		{
			InventoryEntry entry = _inventory.GetItemAtIndex(i);

			if(entry != null)
			{
				_slots[i].SetItem(entry);
			} else {
				_slots[i].Clear();
			}
		}
	}
}
