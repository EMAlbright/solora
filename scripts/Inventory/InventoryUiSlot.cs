using Godot;
using System;
using System.Collections.Generic;

public partial class InventoryUiSlot : Panel
{
	[Export] public int InventorySlotIndex { get; set; }
	public InventoryEntry StoredInvEntry;
	private Sprite2D _itemDisplay;
	private Label _quantityLabel;

	private Inventory _pi;
	private Crafting _pc;
	private Hotbar _ph;
	private ResultUiSlot _pr;

	// Add this to track if this slot is from hotbar
	public bool IsHotbarSlot { get; set; } = false;

	public override void _Ready()
	{
		var world = GetTree().CurrentScene;
		// item sprite to display
		_itemDisplay = GetNode<Sprite2D>("CenterContainer/Panel/item_display");
		// quantity to display
		_quantityLabel = GetNode<Label>("CenterContainer/Panel/Label");
		
		// since hotbar uses inventory ui slot, getting errors for inventory path
		_pi = world.GetNode<Inventory>("Player/Inventory");
		_pc = world.GetNode<Crafting>("Player/Crafting");
		_ph = world.GetNode<Hotbar>("Player/Hotbar");
		_pr = world.GetNode<ResultUiSlot>("Player/result_ui_slot");

		_quantityLabel.Visible = false;
		_itemDisplay.Visible = false;
	}

	public void SetItem(InventoryEntry entry)
	{
		if (entry != null && entry.Item != null && entry.Quantity != null && entry.Key != null)
		{
			StoredInvEntry = entry;
			_itemDisplay.Texture = entry.Item.Icon;
			_itemDisplay.Scale = new Vector2(entry.Item.WorldScale, entry.Item.WorldScale);
			_itemDisplay.RotationDegrees = (entry.Item.Type == ItemType.Weapon || entry.Item.Type == ItemType.Tool) ? 45f : 0f;
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
		StoredInvEntry = null;
		_itemDisplay.Texture = null;
		_itemDisplay.Visible = false;
		_quantityLabel.Visible = false;
	}
	// can data be dropped into inv slot
	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		// assume that item sent back from crafting is dict (like below)
		if (data.VariantType == Variant.Type.Dictionary)
		{
			var dict = (Godot.Collections.Dictionary)data;
			return dict.ContainsKey("item");
		}
		return false;
	}

	// data getting dropped to inventory slot
	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var dict = (Godot.Collections.Dictionary)data;

		var itemId = dict["item"].ToString();
		var key = dict["key"].ToString();
		var qty = (int)dict["quantity"];
		GD.Print(qty);
		var source = dict.ContainsKey("source") ? dict["source"].ToString() : "inventory";
		// dont need source?
		// var source = dict.ContainsKey("source") ? dict["source"].ToString() : "inventory";
		// get BaseItem from DB?
		BaseItem newInvEntry = ItemDatabase.GetItem(itemId);
		// WE ALREADY ADD ITEM TO INVENTORY IN CRAFTING.REMOVEITEM
		// _pi.AddItem(newInvEntry, qty);
		int xPos;
		int yPos;
		int x = InventorySlotIndex % 4;
		int y = InventorySlotIndex / 4;
		// if get coords from crafting is false, then its coming either from the inventory or hotbar
		// this is the crafting source check
		if(_pc.GetCoords(key, out xPos, out yPos))
		{
			_pc.RemoveItem(xPos, yPos);
		}
		else {
			int slot = _pi.CoordsToIndex(x, y);
			GD.Print($"Position trying to drop: ({x}, {y}), " + $"Slot: {slot}");
			if(source == "hotbar"){
				_ph.RemoveItemByKey(key);
			}
			if(source == "inventory"){
				_pi.RemoveItem(key);
			}
			if(source == "result"){
				_pr.RemoveItem(key);
			}
			if (slot > 15 && slot < 20){
				_ph.AddItem(newInvEntry, qty);
			}
			else {
				_pi.AddItemToPosition(newInvEntry, x, y, qty);
			}
			// remove from inventory and place in dropped spot?
		}

	}


	// data sent to crafting slot
	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (StoredInvEntry == null) return new Variant();

		var preview = new TextureRect();
		preview.Texture = _itemDisplay.Texture;
		preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		preview.CustomMinimumSize = new Vector2(16, 16);

		SetDragPreview(preview);

		// Pass the BaseItem and quantity as drag data
		return new Godot.Collections.Dictionary
		{
			{ "key", StoredInvEntry.Key },
			{ "item", StoredInvEntry.Item.ItemId },
			// for now, whenver dragging/dropping, can only drop one at at time
			{ "quantity", 1 },
			{ "source", IsHotbarSlot ? "hotbar" : "inventory" } 
		};
	}
}
