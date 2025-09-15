using Godot;
using System;
using System.Collections.Generic;

public partial class WeaponBase : Node2D, IEquippable, IUsable
{
	// no duplicate hits
	public HashSet<Node2D>_hitEnemies = new();
	// assigned by player when equipped
	public WeaponItem WeaponData;
	
	// access to weapon node sprite
	private Sprite2D _sprite;
	
	private bool _isUsingWeapon = false;
	
	public override void _Ready() {
		_sprite = GetNode<Sprite2D>("WeaponSprite");
		var area = GetNode<Area2D>("weapon_area2d");
		area.AreaEntered += OnAreaEntered;
	}
	
	public virtual void Use(Player player) {
		GD.Print("Override weapon Use method");
		_isUsingWeapon = true;
		GetTree().CreateTimer(WeaponData.AttackDuration).Timeout += () => {
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
	
	// weapon attack starts, reclear set and show weapon node
	public virtual void OnAttackStart() {
		ResetHits();
	}
	
	public virtual void ResetHits() {
		_hitEnemies.Clear();
	}
	
	// equip handling
	public void OnEquip(Player player, BaseItem item) {
		WeaponData = item as WeaponItem;
		Show();
	}
	
	public void OnUnequip() { Hide(); }
}
