using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Node
{
	private Dictionary<string, InventoryEntry> _items = new();
	public string EquippedItemId { get; private set; } = null;

	[Signal]
	public delegate void InventoryChangeEventHandler();

	public IEnumerable<InventoryEntry> GetAllItems()
	{
		return _items.Values;
	}

	// TODO: do we even need place item
	public void PlaceItem()
	{
		return;
	}

	public void AddItem(BaseItem item, int amount = 1)
	{
		if (item.IsStackable)
		{
			if (_items.ContainsKey(item.ItemId))
			{
				GD.Print("Inventory received " + item.ItemId);
				_items[item.ItemId].Quantity += amount;
			}
			else
			{
				GD.Print("Inventory received " + item.ItemId);
				_items[item.ItemId] = new InventoryEntry(item, amount, item.ItemId);
			}
		}
		else
		{
			for (int i = 0; i < amount; i++)
			{
				string uniqueId = $"{item.ItemId}_{Guid.NewGuid()}";
				_items[uniqueId] = new InventoryEntry(item, 1, uniqueId);
			}
		}
		EmitSignal(SignalName.InventoryChange);
	}

	public bool HasItem(string itemId)
	{
		return _items.ContainsKey(itemId) && _items[itemId].Quantity > 0;
	}

	public BaseItem GetItem(string itemId)
	{
		if (_items.ContainsKey(itemId))
		{
			return _items[itemId].Item;
		}
		else
		{
			GD.PrintErr($"Cannot fetch {itemId} - not found.");
			return null;
		}
	}

	public InventoryEntry GetEntry(string key)
	{
		return _items.ContainsKey(key) ? _items[key] : null;
	}

	public void RemoveItem(string key, int amount = 1)
	{
		foreach (var item in _items)
		{
			GD.Print($"{item.Value.Quantity} of {item.Value.Item.ItemId} in inventory");
		}
		if (_items.ContainsKey(key) && _items[key].Quantity >= amount)
		{
			GD.Print($"Removing Item with ID: {key} in inventory");
			_items[key].Quantity -= amount;
			GD.Print($"{_items[key].Quantity} left of {key} ");

			if (_items[key].Quantity == 0)
			{
				_items.Remove(key);
			}
		}
		else
		{
			GD.Print($"Cannot remove {amount} of {key} - not enough in inventory.");
		}
		EmitSignal(SignalName.InventoryChange);
	}
	
	public void AddItemWithKey(BaseItem item, int amount, string key)
{
	if (_items.ContainsKey(key))
	{
		_items[key].Quantity += amount;
	}
	else
	{
		_items[key] = new InventoryEntry(item, amount, key);
	}
	EmitSignal(SignalName.InventoryChange);
}
}
