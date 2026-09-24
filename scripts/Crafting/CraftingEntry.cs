using Godot;
using System;

public class CraftingEntry{
	public BaseItem Item { get; private set; }
	public int Quantity { get; set; }

	public string Key { get; set; }
	
	public CraftingEntry(BaseItem item, int quantity, string key)
	{
		Item = item;
		Quantity = quantity;
		Key = key;
	}
}
