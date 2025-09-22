using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;

public partial class CraftingUiSlot : Panel
{
	[Export] public int SlotIndex { get; set; }

	private Crafting _pc;
	public CraftingEntry StoredCraftEntry;

	private Sprite2D _itemDisplay;
	private Label _quantityLabel;

	private Inventory _playerInventory;
	private Hotbar _playerHotbar;

	public override void _Ready()
	{
		// item sprite to display
		_itemDisplay = GetNode<Sprite2D>("CenterContainer/Panel/craft_item_display");
		// quantity to display
		_quantityLabel = GetNode<Label>("CenterContainer/Panel/Label");
		_pc = GetParent().GetParent().GetParent().GetParent().GetNode<Crafting>("Crafting");
		_playerInventory = GetParent().GetParent().GetParent().GetParent().GetNode<Inventory>("Inventory");
		_playerHotbar = GetParent().GetParent().GetParent().GetParent().GetNode<Hotbar>("Hotbar");

		_quantityLabel.Visible = false;
		_itemDisplay.Visible = false;
	}

	public void SetItem(CraftingEntry entry)
	{
		if (entry != null && entry.Item != null && entry.Quantity != null && entry.Key != null)
		{
			StoredCraftEntry = entry;
			_itemDisplay.Texture = entry.Item.Icon;
			_itemDisplay.Scale = new Vector2(entry.Item.WorldScale, entry.Item.WorldScale);
			_itemDisplay.Visible = true;

			// if stackable and quantity > 1 show
			if (entry.Item.IsStackable && entry.Quantity > 1)
			{
				_quantityLabel.Text = entry.Quantity.ToString();
				_quantityLabel.Visible = true;
			}
			else
			{
				_quantityLabel.Visible = false;
			}
		}
		else
		{
			Clear();
		}
	}

	public void Clear()
	{
		_itemDisplay.Texture = null;
		_itemDisplay.Visible = false;
		_quantityLabel.Visible = false;
	}

	// can data be dropped into craft slot
	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (data.VariantType == Variant.Type.Dictionary)
		{
			var dict = (Godot.Collections.Dictionary)data;
			return dict.ContainsKey("item");
		}
		return false;
	}

	//drop data from inventory slot to crafting slot
	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var dict = (Godot.Collections.Dictionary)data;

		var itemId = dict["item"].ToString();
		var key = dict["key"].ToString();
		var qty = (int)dict["quantity"];
		var source = dict.ContainsKey("source") ? dict["source"].ToString() : "inventory";

		GD.Print($"id: {itemId} - key: {key} - amount: {qty} - source: {source}");

		InventoryEntry entry = null;

		// Get the entry from the appropriate source
		if (source == "hotbar")
		{
			entry = _playerHotbar.GetEntryByKey(key);
		}
		else
		{
			(int a, int b)= _playerInventory.FindItemByKey(key);
			entry = _playerInventory.GetItemAt(a, b);
		}

		if (entry == null)
		{
			GD.PrintErr($"Could not find entry with key {key} in {source}");
			return;
		}
		GD.Print($"CraftingUiSlot SlotIndex: {SlotIndex}");
		int x = SlotIndex % 2;
		int y = SlotIndex / 2;
		GD.Print($"Calculated position: x={x}, y={y}");
		GD.Print("entry: " + entry.Item.ItemId);

		// find way to replace item already in crafting back to inv
		if(_pc.PlaceItem(x, y, itemId, key, qty, source)) {
			var newCraftingEntry = new CraftingEntry(entry.Item, qty, key);
			newCraftingEntry.Source = source;
			SetItem(newCraftingEntry);
		};
	}

	// data sent to inventory slot
	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (StoredCraftEntry == null) return new Variant();

		var preview = new TextureRect();
		preview.Texture = _itemDisplay.Texture;
		preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		preview.CustomMinimumSize = new Vector2(16, 16);

		SetDragPreview(preview);
		
		return new Godot.Collections.Dictionary
		{
			{"key", StoredCraftEntry.Key},
			{"item", StoredCraftEntry.Item.ItemId},
			{"quantity", StoredCraftEntry.Quantity}
		};
	}


}
