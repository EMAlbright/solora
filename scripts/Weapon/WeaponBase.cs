using Godot;
using System;
using System.Collections.Generic;
namespace Weapon.Combat;

public partial class WeaponBase : Node2D, IEquippable, IWeapon
{
	// assigned by player when equipped
	public WeaponItem WeaponData {get; private set;}
	
	public virtual float LightAttackDuration => (WeaponData?.AttackDuration ?? 0f) * 2f; 

	public virtual float HeavyAttackDuration => (WeaponData?.AttackDuration ?? 0f) * 2f; 

	public virtual float HeavyAttackStaminaCost => 25f;

	protected Node2D Owner {get; private set;}

	private readonly HashSet<Node2D> _hitTargets = new();
	private CollisionShape2D _lightCollision;
	private CollisionShape2D _heavyCollision;
	private bool _isAttacking;
	private float _damageMultiplier = 1f;
	
	public override void _Ready() {
		Area2D lightArea = GetNode<Area2D>("weapon_area2d");
		Area2D heavyArea = GetNode<Area2D>("Heavyweapon_area2d");
		
		_lightCollision = lightArea.GetNode<CollisionShape2D>("CollisionShape2D");
		_heavyCollision = heavyArea.GetNode<CollisionShape2D>("CollisionShape2D");

		lightArea.AreaEntered += OnLightAreaEntered;
		heavyArea.AreaEntered += OnHeavyAreaEntered;
		
		SetHitBoxesDisabled();
	}
	
	public virtual void LightAttack(AttackContext context){
		StartAttack(
			lightAttack: true,
			dmgMultiplier: 1f
		);

		EndAttackAfter(
			WeaponData.AttackDuration
		);
	}
	
	public virtual void HeavyAttack(AttackContext context){
		StartAttack(
			lightAttack: false,
			dmgMultiplier: 1.5f
		);

		EndAttackAfter(
			WeaponData.AttackDuration
		);
	}
	
	// weapon attack starts, reclear set and show weapon node
	protected virtual void OnAttackStart() {
		ResetHits();
	}

	private void SetHitBoxesDisabled()
	{
		_lightCollision.Disabled = true;
		_heavyCollision.Disabled = true;
	}
	
	protected virtual void ResetHits() {
		_hitTargets.Clear();
	}
	
	// equip handling
	public virtual void OnEquip(EquipmentContext context) {
		Owner = context.Owner; 
		WeaponData = context.Entry.Item as WeaponItem;
		if(WeaponData == null)
		{
			GD.Print("No Weapon Data found when equipping");
			return;
		}
		Show();
	}
	
	public virtual void OnUnequip() { 
		_isAttacking = false;
		SetHitBoxesDisabled();
		
		Hide(); 
		Owner = null;
		WeaponData = null;
	}

	private void StartAttack(bool lightAttack, float dmgMultiplier)
	{
		_damageMultiplier = dmgMultiplier;

		if (lightAttack)
		{
			_lightCollision.Disabled = false;
			_heavyCollision.Disabled = true;
		} 
		else if (!lightAttack)
		{
			_lightCollision.Disabled = true;
			_heavyCollision.Disabled = false;		
		}
	}

	private async void EndAttackAfter(float duration)
	{
		await ToSignal(
			GetTree().CreateTimer(duration),
			SceneTreeTimer.SignalName.Timeout
		);
		_isAttacking = false;
		SetHitBoxesDisabled();
	}

	private void OnLightAreaEntered(Area2D area)
	{	
		TryDamageTarget(area);
	}

	private void OnHeavyAreaEntered(Area2D area)
	{
		TryDamageTarget(area);
	}

	private void TryDamageTarget(Area2D area)
	{
		if (!_isAttacking || WeaponData == null)
		{
			return;
		}

		Node2D target = area.GetParent() as Node2D;

		if (target == null || target == Owner == _hitTargets.Contains(target))
		{
			return;
		}

		if (target is not IDamageable damageable)
		{
			return;
		}

		float damage = WeaponData.Damage * _damageMultiplier;
		damageable.TakeDamage(damage);
		_hitTargets.Add(target);

	}
}
