using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class InventoryUiSlot : Panel
{
	[Export] public int InventorySlotIndex { get; set; }
	public InventoryEntry StoredInvEntry;
	private Sprite2D _itemDisplay;
	private Label _quantityLabel;
	private Inventory _inventory;
	private Crafting _crafting;

	public override void _Ready()
	{
		Node player = GetTree().CurrentScene.GetNode("Player");

		// item sprite to display
		_itemDisplay = GetNode<Sprite2D>("CenterContainer/Panel/item_display");
		// quantity to display
		_quantityLabel = GetNode<Label>("CenterContainer/Panel/Label");

		_quantityLabel.Visible = false;
		_itemDisplay.Visible = false;

		_inventory = player.GetNode<Inventory>("Inventory");
		_crafting = player.GetNode<Crafting>("Crafting");
	}

	public void SetItem(InventoryEntry entry)
	{
		if(entry?.Item == null)
		{
			Clear();
			return;
		}

		StoredInvEntry = entry;

		// Set display for item
		_itemDisplay.Texture = entry.Item.Icon;
		_itemDisplay.Scale = new Vector2(entry.Item.WorldScale, entry.Item.WorldScale);
		_itemDisplay.RotationDegrees = (entry.Item.Type == ItemType.Weapon || entry.Item.Type == ItemType.Tool) ? 45f : 0f;
		_itemDisplay.Visible = true;

		// if stackable and quantity > 1 show
		if (entry.Item.IsStackable && entry.Quantity > 1) {
				_quantityLabel.Text = entry.Quantity.ToString();
				_quantityLabel.Visible = true;
		} 
		else {
				_quantityLabel.Visible = false;
		}

	}

	public void Clear()
	{
		StoredInvEntry = null;

		_itemDisplay.Texture = null;
		_itemDisplay.Visible = false;

		_quantityLabel.Text = "";
		_quantityLabel.Visible = false;
	}
	// can data be dropped into inv slot
	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		// assume that item sent back from crafting is dict (like below)
		if (data.VariantType != Variant.Type.Dictionary)
		{
			return false;
		}
		
		Dictionary dict = (Dictionary)data;
		
		return dict.ContainsKey("sourceType");
	}

	// data getting dropped to inventory slot
	public override void _DropData(Vector2 atPosition, Variant data)
	{
		Dictionary dict = (Dictionary)data;

		string sourceType = dict["sourceType"].ToString();

		if(sourceType == "inventory")
		{
			int sourceIndex = (int)dict["sourceIndex"];
			_inventory.MoveItem(sourceIndex, InventorySlotIndex);
			return;
		}

		if(sourceType == "crafting")
		{
			int x = (int)dict["craftX"];
			int y = (int)dict["craftY"];

			_crafting.RemoveItem(x, y, InventorySlotIndex);
		}

	}

	// data getting dragged from inventory slot
	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (StoredInvEntry == null) return new Variant();

		TextureRect preview = new TextureRect();
		preview.Texture = _itemDisplay.Texture;
		preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		preview.CustomMinimumSize = new Vector2(16, 16);

		SetDragPreview(preview);

		// Pass the BaseItem and quantity as drag data
		return new Godot.Collections.Dictionary
		{
			{ "sourceType", "inventory" },
			{ "sourceIndex", InventorySlotIndex } 
		};
	}

	public void SetSelected(bool selected)
	{
		// set slot visibility clearly 
	}
}
