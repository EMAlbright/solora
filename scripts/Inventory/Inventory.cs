using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Inventory : Node
{
	public const int MainInventorySlots = 16;
	public const int HotBarSlots = 4;
	public const int TotalSlots = 20;
	private readonly InventoryEntry[] _slots = new InventoryEntry[TotalSlots];

	[Signal]
	public delegate void InventoryChangeEventHandler();

	public IEnumerable<InventoryEntry> GetAllItems()
	{
		foreach(InventoryEntry entry in _slots)
		{
			if(entry != null)
			{
				yield return entry;
			}
		}
	}

	// get entry at certain slot (index)
	public InventoryEntry GetItemAtIndex(int index) {
		if (!IsValidSlot(index))
		{
			return null;
		}
		return _slots[index];
	}

	public InventoryEntry GetItemByKey(string key)
	{
		int index = FindItemByKey(key);
		if (index == -1)
		{
			return null;
		}
		return _slots[index];
	}
	

	public int FindItem(string itemId) {
		for (int i = 0; i < TotalSlots; i++){
			if(_slots[i]?.Item.ItemId == itemId)
			{
				return i;
			}
		}
		return -1;
	}

	public int FindItemByKey(string key) {
		for (int i = 0; i < TotalSlots; i++){
			if(_slots[i]?.Key == key)
			{
				return i;
			}
		}
		return -1;
	}

	public int FindEmptySlot() {
		for (int i  = 0; i < MainInventorySlots; i++){
			if (_slots[i] == null)
			{
				return i;
			}
		}
		return -1;
	}

    public bool AddItem(BaseItem item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        if (item.IsStackable)
        {
            int existingIndex = FindItem(item.ItemId);

            if (existingIndex != -1)
            {
                _slots[existingIndex].Quantity += amount;
                EmitSignal(SignalName.InventoryChange);
                return true;
            }

            int emptyIndex = FindEmptySlot();

            if (emptyIndex == -1)
                return false;

            _slots[emptyIndex] = new InventoryEntry(
                item,
                amount,
                item.ItemId
            );

            EmitSignal(SignalName.InventoryChange);
            return true;
        }

        for (int i = 0; i < amount; i++)
        {
            int emptyIndex = FindEmptySlot();

            if (emptyIndex == -1)
                return false;

            string key = $"{item.ItemId}_{Guid.NewGuid()}";

            _slots[emptyIndex] = new InventoryEntry(
                item,
                1,
                key
            );
        }

        EmitSignal(SignalName.InventoryChange);
        return true;
    }

	public bool MoveItem(int fromIndex, int toIndex)
	{
		if(!IsValidSlot(fromIndex) || !IsValidSlot(toIndex))
		{
			return false;
		}
		if(fromIndex == toIndex)
		{
			return false;
		}

		InventoryEntry from = _slots[fromIndex];

		if(from == null)
		{
			return false;
		}

		InventoryEntry to = _slots[toIndex];

		// set new slot for item entry
		if(to == null)
		{
			_slots[toIndex] = from;
			_slots[fromIndex] = null;
			
			EmitSignal(SignalName.InventoryChange);
			return true;
		}

		// stack same item
		if(from.Item.IsStackable && to.Item.ItemId == from.Item.ItemId)
		{
			to.Quantity += from.Quantity;
			_slots[fromIndex] = null;
			
			EmitSignal(SignalName.InventoryChange);
			return true;
		}

		// swap both entries
		_slots[toIndex] = from;
		_slots[fromIndex] = to;

		EmitSignal(SignalName.InventoryChange);
		return true;
	}

	public bool RemoveItemByKey(string key, int amount = 1)
	{
        int index = FindItemByKey(key);

        if (index == -1)
		{
			return false;
		}

        InventoryEntry entry = _slots[index];

        if (entry.Quantity < amount)
		{
			return false;
		}

        entry.Quantity -= amount;

        if (entry.Quantity <= 0)
		{
            _slots[index] = null;
		}

        EmitSignal(SignalName.InventoryChange);
        return true;
	}

	public bool RemoveItem(string itemId, int amount = 1)
	{
		int index = FindItem(itemId);

		if(index == -1)
		{
			return false;
		}

		InventoryEntry entry = _slots[index];
		
		if (entry.Quantity < amount)
		{
			return false;
		}

        entry.Quantity -= amount;

        if (entry.Quantity <= 0)
		{
            _slots[index] = null;
		}

        EmitSignal(SignalName.InventoryChange);
        return true;
	}
	
	private bool IsValidSlot(int index)
	{
		return index >= 0 && index < TotalSlots;
	}
}
