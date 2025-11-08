using Godot;
using System;
using System.Collections.Generic;
public partial class Sword : WeaponBase
{
	public override void LightAttack(Player player){
		base.LightAttack(player);
		player.PlayerAnimation("Pierce", WeaponData.AttackDuration);
		base.OnAttackStart();
	}
	
	public override void HeavyAttack(Player player){
		// heavy attacks lose stamina
		if(player.Stamina >= 25){
			player.Stamina -= 25;
		}
		else{
			player._attackInProgress = false;
			return;
		}
		base.HeavyAttack(player);
		player.PlayerAnimation("HeavyAttack", WeaponData.AttackDuration*2);
		base.OnAttackStart();
	}
	
}
