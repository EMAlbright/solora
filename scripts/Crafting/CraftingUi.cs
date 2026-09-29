using Godot;
using System;

public partial class CraftingUi : Control
{
	// Crafting
	private Crafting _crafting;
	
	// Crafting Slots
	private CraftingUiSlot[] _slots;

	public override void _Ready() {

		Visible = false;

		var grid = GetNode<GridContainer>("NinePatchRect/CraftGrid");

		_slots = new CraftingUiSlot[grid.GetChildCount()];

		for (int i = 0; i < grid.GetChildCount(); i++)
		{
			_slots[i] = grid.GetChild<CraftingUiSlot>(i);

			// convert to 2 x 2 grid coordinates
			_slots[i].SlotX = i % 2;
			_slots[i].SlotY = i / 2;
		}
		
		// get crafting node
		_crafting = GetParent().GetNode<Crafting>("Crafting");
		
		// set signal to update UI from Crafting logic node
		_crafting.CraftingChange += UpdateItems;

		UpdateItems();
	}
	
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("inventory")) {
			Visible = !Visible;
		}
	}

	public void UpdateItems()
	{
		for (int i = 0; i < _slots.Length; i++)
		{
			int x = i % 2;
			int y = i / 2;
			CraftingEntry entry = _crafting.GetItemAt(x, y);
			
			if(entry != null)
			{
				_slots[i].SetItem(entry);
			} else {
				_slots[i].Clear();
			}

		}
	}
}
