using Godot;
using Godot.Collections;

public partial class CraftingUiSlot : Panel
{
	[Export] public int SlotX { get; set; }
	[Export] public int SlotY { get; set; }
	public CraftingEntry StoredCraftEntry {get; private set; }
  
	private Sprite2D _itemDisplay;
	private Label _quantityLabel;
	private Crafting _crafting;

	public override void _Ready()
	{
		// item sprite to display
		_itemDisplay = GetNode<Sprite2D>("CenterContainer/Panel/craft_item_display");
		// quantity to display
		_quantityLabel = GetNode<Label>("CenterContainer/Panel/Label");

		_quantityLabel.Visible = false;
		_itemDisplay.Visible = false;
	}

	public void SetItem(CraftingEntry entry)
	{
		if (entry?.Item == null)
		{
			Clear();
			return;
		}

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

	public void Clear()
	{
		StoredCraftEntry = null;

		_itemDisplay.Texture = null;
		_itemDisplay.Visible = false;

		_quantityLabel.Text = "";
		_quantityLabel.Visible = false;
	}

	// can data be dropped into craft slot (assume for now only inventory)
	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (data.VariantType != Variant.Type.Dictionary)
		{
			return false;
		}
		
		Dictionary dict = (Dictionary)data;
		
		return dict.ContainsKey("sourceType") && dict["sourceType"].ToString() == "inventory";
	}

	//data getting dropped to crafting slot
	public override void _DropData(Vector2 atPosition, Variant data)
	{
		Dictionary dict = (Dictionary)data;
		int sourceIndex = (int)dict["sourceIndex"];

		_crafting.PlaceItem(SlotX, SlotY, sourceIndex, 1);
	}

	// data sent to inventory (or other) slot
	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (StoredCraftEntry == null) return new Variant();

		TextureRect preview = new TextureRect();
		preview.Texture = _itemDisplay.Texture;
		preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		preview.CustomMinimumSize = new Vector2(16, 16);

		SetDragPreview(preview);
		
		return new Dictionary
		{
			{ "sourceType", "crafting" },
			{ "craftX", SlotX },
			{"craftY", SlotY}
		};
	}
}
