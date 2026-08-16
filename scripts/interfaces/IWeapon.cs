using Godot;
using System;

public interface IWeapon {
	float LightAttackDuraction {get;}
	float HeavyAttackDuration {get;}
	float HeavyAttackStaminaCost {get;}
	void LightAttack(AttackContext context);
	void HeavyAttack(AttackContext context);
}
