using Godot;
using System;

public partial class ResultUiSlot : Panel
{
	private CraftingEntry StoredCraftEntry;
	private Sprite2D _itemDisplay;
	private Label _quantityLabel;
	private Inventory _pi;
	private Crafting _pc;
	
	[Signal]
	public delegate void ResultSlotChangeEventHandler();
	
	public override void _Ready() {
		Visible = false;
		_itemDisplay = GetNode<Sprite2D>("CenterContainer/Panel/result_item_display");
		_quantityLabel = GetNode<Label>("CenterContainer/Panel/Label");
		_pi = GetParent().GetNode<Inventory>("Inventory");
		_pc = GetParent().GetNode<Crafting>("Crafting");
		_quantityLabel.Visible = false;
		_itemDisplay.Visible = false;
	}
	
	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("inventory")) {
			Visible = !Visible;
		}
	}
	
	public void SetItem(CraftingEntry entry){
		
		if(entry != null){
			
			StoredCraftEntry = entry;
			_itemDisplay.Texture = entry.Item.Icon;
			_itemDisplay.Scale = new Vector2(entry.Item.WorldScale, entry.Item.WorldScale);
			_itemDisplay.Visible = true;
			
			if (entry.Item.IsStackable && entry.Quantity > 1) {
				_quantityLabel.Text = entry.Quantity.ToString();
				_quantityLabel.Visible = true;
			}
			else {
				_quantityLabel.Visible = false;
			}
		}
		else {
			Clear();
		}
	}
	
	//when we remove the crafted item, it will take EVERY AMOUNT out of it 
	// if we decide to only do 1, need way to update since there is no UI/Node logic
	public void RemoveItem(string key, int amount = 1){
		amount = StoredCraftEntry.Quantity;
		if(StoredCraftEntry.Quantity >= amount){
			StoredCraftEntry.Quantity -= amount;
			if(StoredCraftEntry.Quantity <= 0){
				StoredCraftEntry = null;
				_pc.RemoveAllCraftingItems();
				Clear();
			}
			EmitSignal(SignalName.ResultSlotChange);
			return;
		}
	}
	
	public void Clear() {
		StoredCraftEntry = null;
		_itemDisplay.Texture = null;
		_itemDisplay.Visible = false;
		_quantityLabel.Visible = false;
	}
	
	public override Variant _GetDragData(Vector2 atPosition){
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
			{"quantity", StoredCraftEntry.Quantity},
			{"source" ,"result"},
		};
	}
}
