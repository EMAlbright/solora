using Godot;
using System;

public class CraftingEntry{
	public string Key { get; }
	public BaseItem Item { get; private set; }
	public int Quantity {get; set; }
	public string Source { get; set; } = "inventory";
	
	public CraftingEntry(BaseItem item, int quantity, string key)
	{
		Key = key;
		Item = item;
		Quantity = quantity;
	}
}
