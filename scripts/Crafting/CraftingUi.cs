using Godot;
using System;

public partial class CraftingUi : Control
{
	// array of slots
	private CraftingUiSlot[] _slots;
	// player inv class
	private Crafting _playerCrafting;
	
	public override void _Ready() {
		Visible = false;
		var grid = GetNode<GridContainer>("NinePatchRect/CraftGrid");
		_slots = new CraftingUiSlot[grid.GetChildCount()];
		for (int i = 0; i < grid.GetChildCount(); i++)
		{
			GD.Print("Looping (crafting)");
			_slots[i] = grid.GetChild(i) as CraftingUiSlot;
			if (_slots[i] != null) {
				_slots[i].SlotIndex = i;
				GD.Print($"Set crafting slot {i} SlotIndex to {i}");
			}
		}
		
		// get crafting node
		_playerCrafting = GetParent().GetNode<Crafting>("Crafting");
		if (_playerCrafting == null) {
			GD.PrintErr("Crafting node not found!");
		} else {
			GD.Print("Crafting node successfully referenced.");
		}
		
		// set signal to update UI from Crafting logic node
		_playerCrafting.CraftingChange += Update;
	}
	
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("inventory")) {
			Visible = !Visible;
		}
	}

	public void Update()
	{
		for (int i = 0; i < _slots.Length; i++)
		{
			_slots[i].Clear();
		}
		var craftingGrid = _playerCrafting.GetCraftingGrid();

		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				var entry = craftingGrid[i, j];
				if (entry != null)
				{
					int slotIndex = j * 2 + i;
					if (slotIndex < _slots.Length)
					{
						_slots[slotIndex].SetItem(entry);
					}
				}
			}
		}
	}
}
