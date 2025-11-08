using Godot;
using System;

public partial class MaterialBase : StaticBody2D, IEquippable, IUsable, IMineable
{
	public MaterialItem MatData;
	public InventoryEntry InvEntry {get; private set;}
	public BaseItem ItemData {get; private set;}
	private Sprite2D currSprite;
	private Inventory _pi;
	private Hotbar _ph;
	public Player _player;
	
	public int Health;
	
	public override void _Ready(){
		var world = GetTree().CurrentScene;
		currSprite = GetNode<Sprite2D>("Sprite2D");
		
		_pi = world.GetNode<Inventory>("Player/Inventory");
		_ph = world.GetNode<Hotbar>("Player/Hotbar");
		if(currSprite == null){
			GD.Print("Placeable block sprite not found");
		}
	}

	public void Init(MaterialItem def, Player player)
	{
		MatData = def;
		Health = def.Health;
		_player = player;
		CallDeferred(nameof(CreateCollision));
	}
	
	// for unique collision shapes
	private void CreateCollision()
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");
		if (sprite?.Texture == null)
		{
			GD.PrintErr("Placeable sprite texture null");
			return;
		}
		var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		var hitBox = GetNode<CollisionShape2D>("block_hitbox/CollisionShape2D");
		if (collisionShape == null || hitBox == null)
		{
			GD.Print("Placeable collisoin shape not found for base or hitbox");
			return;
		}

		// size of shape
		Vector2 textSize = sprite.Texture.GetSize() * sprite.Scale;

		var rectShape = new RectangleShape2D();
		var hitboxShape = new RectangleShape2D();

		hitboxShape.Size = textSize;
		rectShape.Size = textSize;

		hitBox.Shape = hitboxShape;
		collisionShape.Shape = rectShape;
		GD.Print("Generated collision shape");
	}
	
	public void OnDestroyed(){
		if(_player == null){
			GD.Print("null player");
		}
		Vector2I tilePos = WorldManager.WorldToTilePos(_player.GlobalPosition);
		if(WorldManager.IsUnderground){
			WorldManager._undergroundMiningManager.SpawnBaseDroppedItem(tilePos, MatData);
		}
		else{
			WorldManager._surfaceMiningManager.SpawnBaseDroppedItem(tilePos, MatData);
		}
		
		QueueFree();
	}
	
	
	public void Mine(int amount){
		GD.Print("Mat health: " + MatData.Health);
		Health -= amount;
		if (Health <= 0) {
			// run after all physics queries, collisions, signals, and node tree changes
			CallDeferred(nameof(OnDestroyed));
		}
	}
	
	//TODO: Add player placing animation
	public virtual void Use(Player player){
		player.PlayerAnimation("Place");
		// play animation, call world manager to place item?
		PlaceItem(player);
	}
	
	public void OnEquip(Player player, InventoryEntry item) {
		MatData = item.Item as MaterialItem;
		InvEntry = item;
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
	
	public void PlaceItem(Player player){
		//remove from inventory AND hotbar
		// can only place things from hotbar (1 at a time, no need to pass in qty)
		_pi.RemoveItem(InvEntry.Key);
		_ph.RemoveItemByKey(InvEntry.Key);
		if (player.EquippedItemId == InvEntry.Item.ItemId) {
			player.UnequipWeapon();
		}
		Vector2I tilePos = WorldManager.WorldToTilePos(player.GlobalPosition);
		WorldManager.PlaceBlock(tilePos + player.FacingDirection, MatData, player);
		//place it
	}
}
