using Godot;
using System;
using System.Collections.Generic;

public partial class Crafting : Node
{
	private Inventory _inventory;
	private CraftingEntry _entry;
	private readonly CraftingEntry[,] _slots = new CraftingEntry[2,2];
	
	[Signal]
	public delegate void CraftingChangeEventHandler();
	
	public override void _Ready() {
		_inventory = GetParent().GetNode<Inventory>("Inventory");
	}

	public IEnumerable<CraftingEntry> GetAllItems() {
        for (int x = 0; x < _slots.GetLength(0); x++)
        {
            for (int y = 0; y < _slots.GetLength(1); y++)
            {
                if (_slots[x, y] != null)
                    yield return _slots[x, y];
            }
        }
    }
	
	public CraftingEntry GetItemAt(int x, int y)
	{
		if (!IsValidSlot(x, y))
		{
			return null;
		}
		return _slots[x, y];
	}

	public bool IsValidSlot(int x, int y)
	{
		return x >= 0 && x < _slots.GetLength(0) && y >= 0 && y < _slots.GetLength(1);
	}

	// Place an item into the crafting grid
	public bool PlaceItem(int x, int y, int slotIndex, int amount = 1) {
	
		// Get inventory entry
		InventoryEntry entry = _inventory.GetItemAtIndex(slotIndex);

		if(entry == null)
		{
			return false;
		}
		if(entry.Quantity < amount)
		{
			return false;
		}

		// grab base item and key
		BaseItem item = entry.Item;
		string key = entry.Key;

		// Remove from inventory (may refactor to check slot index of item, i.e. same item in multiple inventory slots)
		if(!_inventory.RemoveItemByKey(key, amount))
		{
			return false;
		}

		// Put into crafting grid
		_slots[x, y] = new CraftingEntry(item, amount, key);
		
		// see if we can craft anything
		EvaluateRecipe();
		
		EmitSignal(SignalName.CraftingChange);
		return true;
	}

	// Remove an item from crafting grid (back to inventory)
	public bool RemoveItem(int x, int y)
	{
		CraftingEntry entry = GetItemAt(x, y);

		if (entry == null) return false;

		// TODO: Add so removing item from crafting grid doesn't auto go back to inventory (may just want to drop)
		if(!_inventory.AddItem(entry.Item, entry.Quantity))
		{
			return false;
		}
		_slots[x, y] = null;

		// Re evaluate if current crafting grid state yields a item
		EvaluateRecipe();

		EmitSignal(SignalName.CraftingChange);
		return true;
	}
	
	public void RemoveAllCraftingItems(){
		for (int x = 0; x < 2; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				_grid[x, y] = null;
			}
		}
		EmitSignal(SignalName.CraftingChange);
	}

	// Clear grid (return items back to inventory)
	public void ClearGrid()
	{
		for (int x = 0; x < 2; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				RemoveItem(x, y);
			}
		}
		EmitSignal(SignalName.CraftingChange);
	}

	// This will eventually check recipe patterns
	public bool TryCraft(int newAmount)
	{
	// compare `_grid` against known recipe layouts.
	// "itemId": quantity
		//TODO: Maybe dont create a dict everytime func is called;; 
		Dictionary<string, int> currItems = new Dictionary<string, int>();
		for (int x = 0; x < _grid.GetLength(0); x++) {
			for (int y = 0; y < _grid.GetLength(1); y++) {
				if (_grid[x, y] != null) {
					if(currItems.ContainsKey(_grid[x,y].Item.ItemId)){
						currItems[_grid[x,y].Item.ItemId] += newAmount;
					}
					else{
						currItems.Add(_grid[x,y].Item.ItemId, _grid[x,y].Quantity);
					}
					GD.Print($"Added {_grid[x,y].Item.ItemId} to crafting list");
				}
			}
		}
		Recipe itemCreated = CraftingDatabase.FindMatch(currItems);
		if(itemCreated == null){
			GD.Print("Null item match returned");
			return false;
		}
		int quantity = itemCreated.Output.Count;
		BaseItem item = ItemDatabase.GetItem(itemCreated.Output.ItemId);
		string key = item.IsStackable ? item.ItemId : $"{item.ItemId}_{Guid.NewGuid()}";
		
		craftableItem = new CraftingEntry(item, quantity, key);
		
		// load recipes when trying to craft only?
		return true;
	}
}
