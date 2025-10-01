using Godot;
using System;

public partial class MaterialBase : StaticBody2D, IEquippable, IUsable
{
	public MaterialItem MatData;
	public InventoryEntry InvEntry {get; private set;}
	public BaseItem ItemData {get; private set;}
	private Sprite2D currSprite;
	private Inventory _pi;
	private Hotbar _ph;
	private Player _player;
	
	public override void _Ready(){
		var world = GetTree().CurrentScene;
		currSprite = GetNode<Sprite2D>("Sprite2D");
		_pi = world.GetNode<Inventory>("Player/Inventory");
		_ph = world.GetNode<Hotbar>("Player/Hotbar");
		if(currSprite == null){
			GD.Print("Placeable block sprite not found");
		}
	}
	
	public override void _PhysicsProcess(double delta){
		
	}
	
	//TODO: Add player placing animation
	public virtual void Use(Player player){
		_player = player;
		player.PlayerAnimation("Place");
		// play animation, call world manager to place item?
		PlaceItem();
	}
	
	public void OnEquip(Player player, BaseItem item) {
		MatData = item as MaterialItem;
		// set so cant interact
		GD.Print("Mat equipped " + MatData.DisplayName);
		SetCollisionLayerValue(1, false);  
		SetCollisionMaskValue(1, false);
		UpdateIcon();
		Show();
	}
	
	public void OnUnequip() {
		Hide();
	}
	
	// need to take in the entry since we need to remove the item from hotbar/inv
	public void Init(InventoryEntry entry){
		if (entry != null){
			InvEntry = entry;
			ItemData = entry.Item;
		}
		UpdateIcon();
	}
	
	public void UpdateIcon(){
		if(currSprite == null){
			return;
		}
		if(MatData.Icon != null){
			currSprite.Texture = MatData.Icon;
			currSprite.Scale = new Vector2(MatData.WorldScale, MatData.WorldScale);
		}
		else{
			MatData.Icon = null;
		}
	}
	
	public void PlaceItem(){
		//remove from inventory AND hotbar
		// can only place things from hotbar (1 at a time, no need to pass in qty)
		//_pi.RemoveItem(InvEntry.Key);
		//_ph.RemoveItemByKey(InvEntry.Key);
		
		Vector2I tilePos = WorldManager.WorldToTilePos(_player.GlobalPosition);
		WorldManager.PlaceBlock(tilePos + _player.FacingDirection, MatData);
		//place it
	}
}
