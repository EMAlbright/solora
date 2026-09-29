using Godot;
using System;
using System.Collections.Generic;

public partial class Crafting : Node
{
	private Inventory _inventory;
	private Recipe _currentRecipe;
	private readonly CraftingEntry[,] _slots = new CraftingEntry[2,2];
	public CraftingEntry CurrentResult { get; private set; }

	
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

	public bool CraftResult(int slotIndex)
	{
		if (CurrentResult == null)
		{
			return false;
		}

		if(!_inventory.TryAddItemAt(slotIndex, CurrentResult.Item, CurrentResult.Quantity))
		{
			return false;
		}

		// Remove ingredients needed
		ConsumeIngredients();

		// Re evaluate if current crafting grid state yields a item
		EvaluateRecipe();

		EmitSignal(SignalName.CraftingChange);
		return true;
	}

	// Remove an item from crafting grid (back to inventory)
	public bool RemoveItem(int x, int y, int slotIndex)
	{
		if (!IsValidSlot(x, y))
		{
			return false;
		}

		CraftingEntry entry = GetItemAt(x, y);

		if (entry == null) return false;

		if(!_inventory.TryAddItemAt(slotIndex, entry.Item, entry.Quantity))
		{
			return false;
		}

		_slots[x, y] = null;

		// Re evaluate if current crafting grid state yields a item
		EvaluateRecipe();

		EmitSignal(SignalName.CraftingChange);
		return true;
	}

	// When not a specific slot
	public bool RemoveItem(int x, int y) {
    	if (!IsValidSlot(x, y))
        	return false;

    	CraftingEntry entry = _slots[x, y];

    	if (entry == null)
        	return false;

   		if (!_inventory.AddItem(entry.Item, entry.Quantity))
        	return false;

    	_slots[x, y] = null;

    	EvaluateRecipe();

    	EmitSignal(SignalName.CraftingChange);

    	return true;
	}
	
	public void ClearGrid(){
		for (int x = 0; x < _slots.GetLength(0); x++)
		{
			for (int y = 0; y < _slots.GetLength(1); y++)
			{
				RemoveItem(x, y);
			}
		}
	}

	public void EvaluateRecipe() {

		Dictionary<string, int> recipe = new Dictionary<string, int>();

		foreach(CraftingEntry entry in GetAllItems())
		{
			string id = entry.Item.ItemId;
			if (recipe.ContainsKey(id))
			{
				recipe[id] += entry.Quantity;
			}
			else
			{
				recipe[id] = entry.Quantity;
			}
		}
		
		_currentRecipe = CraftingDatabase.FindMatch(recipe);

		if(_currentRecipe == null){
			CurrentResult = null;
			GD.Print("Null item match returned");
			return;
		}

		BaseItem resultItem = ItemDatabase.GetItem(_currentRecipe.Output.ItemId);

		if(resultItem == null){
			CurrentResult = null;
			GD.Print("Null item match returned");
		}

		// unique tools/weapons get specific key, otherwise stackable item gets item id as key
		string key = resultItem.IsStackable ? resultItem.ItemId : $"{resultItem.ItemId}_{Guid.NewGuid()}";
		
		CurrentResult = new CraftingEntry(resultItem, recipeOut.Output.Count, key);
		
	}
}
