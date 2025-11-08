using Godot;
using System;
using System.Collections.Generic;

public partial class Hotbar : Node
{
	private Player _player;
	private const int MaxSlots = 4;
	
	private InventoryEntry[] _slots = new InventoryEntry[MaxSlots];
	
	private int _selectedSlot = 0;
	
	// inventory ref for overflow 
	private Inventory _playerInventory;
	
	[Signal]
	public delegate void HotbarChangeEventHandler();
	
	public override void _Ready()
	{
		_playerInventory = GetParent().GetNode<Inventory>("Inventory");
		if (_playerInventory == null)
		{
			GD.PrintErr("Player inventory not found!");
		}
	}
	
	public override void _Process(double delta)
	{
		HandleHotbarInput();
	}
	
	public void HandleHotbarInput()
	{
		for(int i = 0 ; i < MaxSlots; i++)
		{
			if(Input.IsActionJustPressed($"hotbar_slot_{i+1}"))
			{
				SelectSlot(i);
				break;
			}
		}
	}
	
	public InventoryEntry[] GetSlots() => _slots;
	
	public bool AddItem(BaseItem item, int amount = 1)
	{
		if (item.IsStackable)
		{
			// First, try to stack with existing items in hotbar
			for (int i = 0; i < MaxSlots; i++)
			{
				if (_slots[i] != null && _slots[i].Item.ItemId == item.ItemId)
				{
					_slots[i].Quantity += amount;
					EmitSignal(SignalName.HotbarChange);
					return true;
				}
			}
			
			// Find empty slot for new stackable item
			for (int i = 0; i < MaxSlots; i++)
			{
				if (_slots[i] == null)
				{
					// Use item's ItemId as key for stackable items
					_slots[i] = new InventoryEntry(item, amount, item.ItemId);
					EmitSignal(SignalName.HotbarChange);
					return true;
				}
			}
		}
		else
		{
			// Non-stackable items need unique IDs
			for (int i = 0; i < MaxSlots; i++)
			{
				if (_slots[i] == null)
				{
					string uniqueId = $"{item.ItemId}_{Guid.NewGuid()}";
					_slots[i] = new InventoryEntry(item, 1, uniqueId);
					EmitSignal(SignalName.HotbarChange);
					return true;
				}
			}
		}
		
		// Hotbar full
		return false;
	}
	
	public void RemoveItem(int slotIndex, int amount = 1)
	{
		GD.Print("Remove item in hotbar hit");
		if (slotIndex < 0 || slotIndex >= MaxSlots || _slots[slotIndex] == null)
		{
			GD.Print("something else");
			return;
		}
		
		_slots[slotIndex].Quantity -= amount;
		GD.Print(_slots[slotIndex].Item.ItemId);
		GD.Print(_slots[slotIndex].Quantity);
		if(_slots[slotIndex].Quantity <= 0)
		{
			_slots[slotIndex] = null;
			GD.Print("No more in hotbar");
		}
		EmitSignal(SignalName.HotbarChange);
	}
	
	// Add this method to remove items by key (for crafting system)
	public void RemoveItemByKey(string key, int amount = 1)
	{
		for (int i = 0; i < MaxSlots; i++)
		{
			if (_slots[i] != null && _slots[i].Key == key)
			{
				GD.Print("Removing in hotbar");
				_slots[i].Quantity -= amount;
				GD.Print("After removing " + _slots[i].Quantity);
				if (_slots[i].Quantity <= 0)
				{
					_slots[i] = null;
				}
				EmitSignal(SignalName.HotbarChange);
				return;
			}
		}
	}
	
	// Add this method to get entry by key (for crafting system)
	public InventoryEntry GetEntryByKey(string key)
	{
		for (int i = 0; i < MaxSlots; i++)
		{
			if (_slots[i] != null && _slots[i].Key == key)
			{
				return _slots[i];
			}
		}
		return null;
	}
	
	public void SelectSlot(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= MaxSlots)
		{
			return;
		}
		_selectedSlot = slotIndex;
		GD.Print($"Selected slot {slotIndex + 1}");
		
		EquipItemFromSlot(slotIndex);
	}
	
	private void EquipItemFromSlot(int slotIndex)
	{
		// get player
		var player = GetParent() as Player;
		_player = player;
		if (player == null)
		{
			GD.Print("In hotbar, parent null");
			return;
		}
		
		// no item, unequip current
		if (_slots[slotIndex] == null)
		{
			player.UnequipWeapon();
			return;
		}
		
		var item = _slots[slotIndex];

		// for now, only handle weapons and tools
		if(item.Item.Type == ItemType.Weapon || item.Item.Type == ItemType.Tool || item.Item.Type == ItemType.Mat)
		{
			GD.Print(item.Item.Type);
			player.EquipFromHotbar(item);
		}
	}
	
	public int GetSelectedSlot() => _selectedSlot;
	
	public InventoryEntry GetSelectedItem()
	{
		return _slots[_selectedSlot];
	}
	
	public InventoryEntry GetSlotItem(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= MaxSlots)
		{
			return null;
		}
		return _slots[slotIndex];
	}
	
	public bool IsSlotEmpty(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= MaxSlots)
		{
			return true;
		}
		return _slots[slotIndex] == null;
	}
}
