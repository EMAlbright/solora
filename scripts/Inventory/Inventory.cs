using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Inventory : Node
{
	public static Inventory Instance;
	public InventoryEntry[,] _items = new InventoryEntry[4,5];
	private const int Columns = 4;
	private const int Rows = 5;

	public string EquippedItemId { get; private set; } = null;

    public override void _Ready()
    {
        Instance = this;
    }


	[Signal]
	public delegate void InventoryChangeEventHandler();

	public IEnumerable<InventoryEntry> GetAllItems()
	{
		for (int x = 0; x < Columns; x++) {
			for (int y = 0; y < Rows; y++) {
				if (_items[x, y] != null) {
					yield return _items[x, y];
				}
			}
		}
	}

	public InventoryEntry[,] GetInventoryGrid() 
	{
		return _items;  
	}

	public (int x, int y) FindItem(string itemId) {
		for (int x = 0; x < Columns; x++){
			for (int y = 0; y < Rows; y++) {
				if (_items[x,y] != null && _items[x,y].Item.ItemId == itemId){
					return (x, y);
				}
			}
		}
		return (-1, -1);
	}

	public (int x, int y) FindItemByKey(string key) {
		for (int x  = 0; x < Columns; x++){
			for (int y = 0; y < Rows; y++){
				if (_items[x,y] != null && _items[x,y].Key == key){
					return (x, y);
				}
			}
		}
		return (-1, -1);
	}

	public (int x, int y) FindEmptySlot() {
		for (int x  = 0; x < Columns; x++){
			for (int y = 0; y < Rows; y++){
				if (_items[x,y] == null){
					return (x, y);
				}
			}
		}
		return (-1, -1);
	}

	private (int x, int y) IndexToCoords(int index) {
		return (index % Columns, index / Columns);
	}

	public int CoordsToIndex(int x, int y) {
		// map to a slot index
		// [0 0 0]
		// [0 0 0]
		// -------
		// [0 1 2]
		// [3 4 5]
		return y * Columns + x;
	}

	// get entry at certain point (coords)
	public InventoryEntry GetItemAt(int x, int y) {
		if (x < 0 || x >= Columns || y < 0 || y >= Rows){
			return null;
		}
		return _items[x,y];
	}

	// get entry at certain slot (index)
	public InventoryEntry GetItemAtIndex(int index) {
		var (x, y) = IndexToCoords(index);
		return GetItemAt(x, y);
	}

	public void AddItem(BaseItem item, int amount = 1)
	{
		if (item.IsStackable)
		{
			// try to stack
			var (x, y) = FindItem(item.ItemId);
			if (x != -1){
				GD.Print("Inventory received " + item.ItemId);
				_items[x, y].Quantity += amount;
				EmitSignal(SignalName.InventoryChange);
				return;
			}
		}

		// not stackable, loop through to find empty slot
		for (int i = 0; i < amount; i++){
			var (emptyX, emptyY) = FindEmptySlot();
			if (emptyX != -1){
				// key is item id, if non stackable unique id
				string key = item.IsStackable ? item.ItemId : $"{item.ItemId}_{Guid.NewGuid()}";
				_items[emptyX, emptyY] = new InventoryEntry(item, item.IsStackable ? amount : 1, key);
				GD.Print("Inventory received " + item.ItemId);

				// stackable, jsut one entry
				if(item.IsStackable){
					break;
				}
			}
			// full inventory
			else{
				GD.Print("Inventory full");
				break;
			}
		}
		EmitSignal(SignalName.InventoryChange);
	}

	// add item to certain slot
	public bool AddItemToSlot(BaseItem item, int slotIndex, int amount = 1){
		var (x, y) = IndexToCoords(slotIndex);
		return AddItemToPosition(item, x, y, amount);
	}

	public bool AddItemToPosition(BaseItem item, int x, int y, int amount = 1){
		if (x < 0 || x >= Columns || y < 0 || y >= Rows){
			return false;
		}
		
		// item can be added
		if (_items[x,y] == null) {
			string key = item.IsStackable ? item.ItemId : $"{item.ItemId}_{Guid.NewGuid()}";
			_items[x,y] = new InventoryEntry(item, amount, key);
			EmitSignal(SignalName.InventoryChange);
			return true;
		}
		else if (item.ItemId == _items[x,y].Item.ItemId && item.IsStackable){
			_items[x,y].Quantity += amount;
			EmitSignal(SignalName.InventoryChange);
			return true;
		}
		return false;
	}

	public bool HasItemByKey(string key)
	{
		for (int i = 0; i < Columns; i++)
		{
			for(int j = 0; j < Rows; j++)
			{
				if (_items[i, j] != null && _items[i, j].Key == key)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasItem(string itemId)
	{
		for(int i = 0; i < Columns; i++)
		{
			for(int j = 0; j < Rows; j++)
			{
				if(_items[i, j] != null && _items[i, j].Item.ItemId == itemId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveItemByKey(string key, int amount = 1)
	{
		for (int i = 0; i < Columns; i++){
			for (int j = 0 ; j < Rows; j++){
				
				if (_items[i, j] != null && _items[i,j].Key == key){
					GD.Print(key);
					if(_items[i,j].Quantity >= amount){
						_items[i, j].Quantity -= amount;

						if(_items[i,j].Quantity <= 0){
							_items[i,j] = null;
						}

						EmitSignal(SignalName.InventoryChange);
						return;
					}
					else{
						// not enough quantity to remove,
						// dont have to worry about this for now,
						// can only remove one at a time
						return;
					}
				}
			}
		}
	}

	public void RemoveItem(string itemId, int amount = 1)
	{
		for(int i = 0; i < Columns; i++)
		{
			for(int j = 0; j < Rows; j++)
			{
				if (_items[i, j] != null && _items[i, j].Item.ItemId == itemId)
				{
					if(_items[i, j].Quantity >= amount)
					{
						_items[i, j].Quantity -= amount;
						if(_items[i, j].Quantity <= 0)
						{
							_items[i, j] = null;
						}
						EmitSignal(SignalName.InventoryChange);
						return;
					}
					else
					{
						return;
					}
				}
			}
		}
	}
	
	public void AddItemWithKey(BaseItem item, int amount, string key)
{
	var (x, y) = FindItemByKey(key);
	if (x != -1)
	{
		_items[x, y].Quantity += amount;
	}
	else
	{
		var (eX, eY) = FindEmptySlot();
		if (eX != -1){
			_items[x, y] = new InventoryEntry(item, amount, key);
		}
	}
	EmitSignal(SignalName.InventoryChange);
}
}
