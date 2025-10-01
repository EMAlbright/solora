using Godot;
using System;
using System.Collections.Generic;

public partial class Crafting : Node
{
	private Crafting _crafting;
	private Inventory _inventory;
	private Hotbar _hotbar;
	private CraftingEntry craftableItem;
	private ResultUiSlot resultSlot;

	private CraftingEntry[,] _grid = new CraftingEntry[2,2];
	
	[Signal]
	public delegate void CraftingChangeEventHandler();
	
	public override void _Ready() {
		// get inventory/hotbar node
		_inventory = GetParent().GetNode<Inventory>("Inventory");
		_hotbar = GetParent().GetNode<Hotbar>("Hotbar");
		
		resultSlot = GetParent().GetNode<Panel>("result_ui_slot") as ResultUiSlot;
	}
	
	public IEnumerable<CraftingEntry> GetAllItems() {
		for (int x = 0; x < _grid.GetLength(0); x++) {
			for (int y = 0; y < _grid.GetLength(1); y++) {
				if (_grid[x, y] != null) {
					yield return _grid[x, y];
				}
			}
		}
	}


	public CraftingEntry[,] GetCraftingGrid()
	{
		return _grid;
	}
// Place an item into the crafting grid
public bool PlaceItem(int x, int y, string itemId, string key, int amount = 1, string source = "inventory") 
{
	GD.Print($"Item placed in craft: {itemId} from {source}");
	
	InventoryEntry entry = null;
		if (_grid[x, y] == null)
		{
			// Get the entry from the appropriate source
			if (source == "hotbar")
			{
				GD.Print("Source is hotbar");
				if (_hotbar == null)
				{
					GD.PrintErr("Hotbar not found!");
					return false;
				}

				entry = _hotbar.GetEntryByKey(key);
				if (entry == null)
				{
					GD.PrintErr($"Item with key {key} not found in hotbar");
					return false;
				}

				GD.Print($"Found entry in hotbar: {entry.Item.ItemId}");

				// Remove from hotbar
				_hotbar.RemoveItemByKey(key, amount);
				GD.Print($"Removed {amount} of {key} from hotbar");
			}
			else
			{
				GD.Print("Source is inventory");
				// Remove from inventory
				(int a, int b) = _inventory.FindItemByKey(key);
				entry = _inventory.GetItemAt(a, b);
				if (entry == null)
				{
					GD.PrintErr($"Item with key {key} not found in inventory");
					return false;
				}

				_inventory.RemoveItem(key, amount);
			}

			// Put into crafting grid
			_grid[x, y] = new CraftingEntry(entry.Item, amount, key);
			_grid[x, y].Source = source;
			GD.Print(amount);
			// see if we can craft anything
			if(TryCraft(amount)){
				// we can craft something, call ResultUiSlot to display item that is craftable
				GD.Print(craftableItem.Item.ItemId);
				resultSlot.SetItem(craftableItem);
			};
			GD.Print($"Successfully placed {entry.Item.ItemId} in crafting grid at ({x}, {y})");

			EmitSignal(SignalName.CraftingChange);
			return true;
		}
		else
		{
			return false;
		}
}

	public bool GetCoords(string key, out int x, out int y)
	{
		for (x = 0; x < _grid.GetLength(0); x++)
		{
			for (y = 0; y < _grid.GetLength(1); y++)
			{
				if (_grid[x,y] != null && _grid[x, y].Key == key)
				{
					GD.Print($"Item key: {key} " + $" coords: {x}, {y}");
					return true;
				}
			}
		}
		x = -1;
		y = -1;
		GD.Print("false, no coords");
		return false;
	}
	// Remove an item from crafting grid (back to inventory)
	public void RemoveItem(int x, int y)
	{
		var entry = _grid[x, y];
		if (entry == null) return;

		_inventory.AddItem(entry.Item, entry.Quantity);
		_grid[x, y] = null;
		EmitSignal(SignalName.CraftingChange);
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
