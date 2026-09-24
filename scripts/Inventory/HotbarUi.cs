using Godot;
using System;

public partial class HotbarUi : Control
{
	private const int HotbarIndex = 16;
	private InventoryUiSlot[] _slots;
	private Hotbar _hotbar;
	private Inventory _inventory;

	public override void _Ready()
	{
		GridContainer grid = GetNode<GridContainer>("NinePatchRect/HotbarGrid");
		
		_slots = new InventoryUiSlot[grid.GetChildCount()];

		for (int i = 0; i < _slots.Length; i++)
		{
			_slots[i] = grid.GetChild<InventoryUiSlot>(i);

			if (_slots[i] != null)
			{	
				_slots[i].InventorySlotIndex = i + HotbarIndex;
				// Mark this slot as a hotbar slot
				_slots[i].IsHotbarSlot = true;
			}
		}

		// Get node, player, and hotbar
		Node player = GetTree().CurrentScene.GetNode<Node>("Player");
		_inventory = player.GetNode<Inventory>("Inventory");
		_hotbar = player.GetNode<Hotbar>("Hotbar");

		_inventory.InventoryChange += UpdateItems;
		_hotbar.HotbarSelectionChanged += UpdateSelection;

		// Initialize hotbar and inventory changes
		UpdateItems();
		UpdateSelection(_hotbar.GetSelectedSlot());
	}

	public void UpdateItems()
	{

		for (int i = 0; i < _slots.Length; i++)
		{
			int inventoryIndex = HotbarIndex + i;

			InventoryEntry entry = _inventory.GetItemAtIndex(inventoryIndex);

			if(entry != null)
			{
				_slots[i].SetItem(entry);
			} else {
				_slots[i].Clear();
			}			
		}
	}

	public void UpdateSelection(int slot)
	{
		for (int i = 0; i < _slots.Length; i++)
		{
			// set inventory UI slot to current slot, otherwise set to false
			_slots[i].SetInventoryUISlot(i == slot);
		}
	}
}
