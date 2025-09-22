using Godot;
using System;

public partial class HotbarUi : Control
{
	private InventoryUiSlot[] _slots;
	private Hotbar _hotbar;

	public override void _Ready()
	{
		var grid = GetNode<GridContainer>("NinePatchRect/HotbarGrid");
		_slots = new InventoryUiSlot[grid.GetChildCount()];

		for (int i = 0; i < grid.GetChildCount(); i++)
		{
			_slots[i] = grid.GetChild(i) as InventoryUiSlot;
			if (_slots[i] != null)
			{	_slots[i].InventorySlotIndex = i+16;
				// Mark this slot as a hotbar slot
				_slots[i].IsHotbarSlot = true;
			}
		}

		var player = GetTree().Root.GetNode<Node>("world/Player");
		_hotbar = player.GetNode<Hotbar>("Hotbar");
		if (_hotbar == null)
		{
			GD.PrintErr("Hotbar node not found");
		}
		_hotbar.HotbarChange += Update;
	}

	public void Update()
	{
		if (_hotbar == null)
		{
			return;
		}

		var hotbarSlots = _hotbar.GetSlots();

		for (int i = 0; i < _slots.Length && i < hotbarSlots.Length; i++)
		{
			if (hotbarSlots[i] != null)
			{
				var entry = _hotbar.GetSlotItem(i);
				_slots[i].SetItem(entry);
			}
			else
			{
				_slots[i].Clear();
			}
		}
	}
}
