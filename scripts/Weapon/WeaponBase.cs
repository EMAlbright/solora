using Godot;
using System;
using System.Collections.Generic;

public partial class WeaponBase : Node2D, IEquippable, IWeapon
{
	// no duplicate hits
	public HashSet<Node2D>_hitEnemies = new();
	// assigned by player when equipped
	public WeaponItem WeaponData;
	
	// access to weapon node sprite
	private Sprite2D _sprite;
	
	private bool _isUsingWeapon = false;
	
	private Area2D area;
	private Area2D areaHeavy;
	
	private CollisionShape2D _lightCollision;
	private CollisionShape2D _heavyCollision;
	
	public override void _Ready() {
		_sprite = GetNode<Sprite2D>("WeaponSprite");
		area = GetNode<Area2D>("weapon_area2d");
		areaHeavy = GetNode<Area2D>("Heavyweapon_area2d");
		
		_lightCollision = area.GetNode<CollisionShape2D>("CollisionShape2D");
		_heavyCollision = areaHeavy.GetNode<CollisionShape2D>("CollisionShape2D");

		area.AreaEntered += OnAreaEntered;
		areaHeavy.AreaEntered += OnHeavyAreaEntered;
		
		_lightCollision.Disabled = true;
		_heavyCollision.Disabled = true;
	}
	
	public virtual void LightAttack(Player player){
		_lightCollision.Disabled = false;
		_heavyCollision.Disabled = true;
		_isUsingWeapon = true;
		GetTree().CreateTimer(WeaponData.AttackDuration).Timeout += () => {
			_lightCollision.Disabled = true;
			_isUsingWeapon = false;
		};
	}
	
	public virtual void HeavyAttack(Player player){
		_heavyCollision.Disabled = false;
		_lightCollision.Disabled = true;
		_isUsingWeapon = true;
		// heavy attack lasts 1.3 seconds (normal .65)
		GetTree().CreateTimer(WeaponData.AttackDuration*2).Timeout += () => {
			_heavyCollision.Disabled = true;
			_isUsingWeapon = false;
		};
	}
	
	protected virtual void OnAreaEntered(Area2D area) {
		if(WeaponData == null) {
			GD.Print("Null weapon data");
		}
		if (!Visible || WeaponData == null || !_isUsingWeapon) {
			GD.Print("Not visible");
			return;
		}
		
		Node2D enemy = area.GetParent() as Node2D;
		
		if(area.Name == "enemy_hitbox" && enemy != null && enemy.IsInGroup("enemy") && !_hitEnemies.Contains(enemy)) {
			GD.Print("Hit bear");
			// call the enemies take damage function 
			enemy.Call("TakeDamage", WeaponData.Damage);
			_hitEnemies.Add(enemy);
		}
	}
	
	protected virtual void OnHeavyAreaEntered(Area2D area) {
		if(WeaponData == null) {
			GD.Print("Null weapon data");
		}
		if (!Visible || WeaponData == null || !_isUsingWeapon) {
			GD.Print("Not visible");
			return;
		}
		
		Node2D enemy = area.GetParent() as Node2D;
		
		if(area.Name == "enemy_hitbox" && enemy != null && enemy.IsInGroup("enemy") && !_hitEnemies.Contains(enemy)) {
			GD.Print("Hit bear");
			// call the enemies take damage function 
			enemy.Call("TakeDamage", WeaponData.Damage*1.5);
			_hitEnemies.Add(enemy);
		}
	}
	
	// weapon attack starts, reclear set and show weapon node
	public virtual void OnAttackStart() {
		ResetHits();
	}
	
	public virtual void ResetHits() {
		_hitEnemies.Clear();
	}
	
	// equip handling
	public virtual void OnEquip(Player player, InventoryEntry item) {
		WeaponData = item.Item as WeaponItem;
		Show();
	}
	
	public void OnUnequip() { Hide(); }
}
