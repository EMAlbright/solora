using Godot;
using System;

public partial class Bear : BaseEnemy
{
	protected override void InitializeComponents() {
		Speed = 35;
		MinDistance = 40.0f;
		MaxHealth = 100;
		AttackDamage = 15;
	
		// comps
		animationPlayer = GetNode<AnimationPlayer>("BearAnimation");
		enemySprite = GetNode<Sprite2D>("BodySprite");
		attackHitbox = GetNode<Area2D>("attack_hitbox");		
	}
	
	// logic for deciding which attack
	protected override string GetAttackAnimationName() {
		return "Attack_2";
	}
	
	protected override void OnDamageTaken(int damage) {
		return;
	}

}

/*
	public int Speed = 35;
	public float MinDistance = 40.0f;
	public int Health = 100;
	
	public bool Chase = false;
	public Player Hero = null;
	
	public bool Player_in_zone = false;
	public bool Can_take_dmg = true;
	public bool IsTakingDmg = false;
	public bool IsAttacking = false;
	
	private string _currentAnim = "";
	
	// Timers
	private Timer take_damage_cooldown;
	private Timer attack_duration;
	
	// sprite and anim
	private AnimationPlayer _animationPlayer;
	private Sprite2D _bearSprite;
	// health bar
	
	private Area2D _attackHitbox;
	
	public override void _Ready() {
		//timers 
		take_damage_cooldown = GetNode<Timer>("take_damage_cooldown");
		attack_duration = GetNode<Timer>("attack_duration");

		//anim
		_animationPlayer = GetNode<AnimationPlayer>("BearAnimation");
		// sprite
		_bearSprite = GetNode<Sprite2D>("BodySprite");
		
		_attackHitbox = GetNode<Area2D>("attack_hitbox");

	}
	
	public override void _PhysicsProcess(double delta) {
		if(!IsAttacking) {
			if (Chase && Hero != null) {
				Vector2 direction = (Hero.Position - Position).Normalized();
				float distance = Hero.Position.DistanceTo(Position);
				if (distance > MinDistance) {
					Velocity = direction * Speed;
					UpdateAnimation(direction, "Walk");					
				}
				else {
					Velocity = Vector2.Zero;
					UpdateAnimation(direction, "Idle");
				}
			}
			else {
				Velocity = Vector2.Zero;
				_animationPlayer.Play("DownIdle");
			}
		}
		else {
			// no movement during attacj
			Velocity = Vector2.Zero;
		}
		MoveAndSlide();
	}
	
	private void UpdateAnimation(Vector2 dir, string animType) {
		if (dir == Vector2.Zero) {
			return;
		}
		float angle = Mathf.Atan2(dir.Y, dir.X);
		
		string anim = $"Down{animType}";
		if (angle >= -0.3927f && angle < 0.3927f) {
			_bearSprite.FlipH = false;
			anim = $"Side{animType}";	
		}
		else if (angle >= 0.3927f && angle < 1.1781f) {
			_bearSprite.FlipH = false;
			anim = $"SideDown{animType}";	
		}
		else if (angle >= 1.1781f && angle < 1.9635f) {
			_bearSprite.FlipH = false;
			anim = $"Down{animType}";	
		}
		else if (angle >= 1.9635f && angle < 2.7489f) {
			// flip bear sprite
			_bearSprite.FlipH = true;
			anim = $"SideDown{animType}";	
		}
		else if (angle >= 2.7489f || angle < -2.7489f) {
			// flip bear sprite
			_bearSprite.FlipH = true;
			anim = $"Side{animType}"; // left	
		}
		else if (angle >= -2.7489f && angle < -1.9635f) {
			_bearSprite.FlipH = false;
			anim = $"SideUp{animType}";			
		}

		else if (angle >= -1.9635f && angle < -1.1781f) {
			_bearSprite.FlipH = false;
			anim = $"Up{animType}";			
		}

		else if (angle >= -1.1781f && angle < -0.3927f) {
			_bearSprite.FlipH = true;
			anim = $"SideUp{animType}";	
		}
		UpdateAttackHitboxPosition();
		// if bear sprite not playing, play new sprite
		if (_currentAnim != anim) {
			_animationPlayer.Play(anim);
			_currentAnim = anim;
		}
	}
	
	public void Take_damage(int dmg) {
		if (Can_take_dmg && !IsTakingDmg) { 
			Health -= dmg;
			GD.Print("Bear Health: ", Health);
			Can_take_dmg = false;
			IsTakingDmg = true;
			take_damage_cooldown.Start();
			
			// animation to take damage
			// Vector2 direction = (Hero.Position - Position).Normalized();
			// UpdateAnimation(direction, "dmg");
			// Velocity = -direction * 50;
			if (Health <= 0) {
				IsTakingDmg = false;
				// UpdateAnimation(direction, "death");
				// Velocity = Vector2.Zero;
				// remove from scene
				QueueFree();
			}
		}
		
	}
	
	private void UpdateAttackHitboxPosition() {
	if (_bearSprite.FlipH == true) {
		// Bear is facing left, flip the hitbox
		_attackHitbox.Scale = new Vector2(-1, 1);
	} else {
		// Bear is facing right, normal scale
		_attackHitbox.Scale = new Vector2(1, 1);
	}
}
	
	private void _on_detection_area_body_entered(Node2D body) {
		if (body is Player player) {
			Hero = player;
			Chase = true;	
		}
	}
	private void _on_detection_area_body_exited(Node2D body) {
		if (body == Hero) {
			Hero = null;
			Chase = false;	
		}
	}
	
	// if players hitbox enters bears attack hitbox (attack hands) then call take dmg func on player
	private void _on_attack_hitbox_area_entered(Area2D area) {
		if (area.Name == "player_hitbox" && area.GetParent() is Player player) {
			player.Call("Take_damage", 15);
			GD.Print("Bear hit player for 15 damage!");
		}
	}
	
	// play attack animation towards player if in zone
	private void _on_attack_zone_body_entered(Node2D body) {
		if (body is Player player && Hero != null && !IsAttacking) {
			Vector2 direction = (Hero.Position - Position).Normalized();
			IsAttacking = true;
			Player_in_zone = true;
			UpdateAnimation(direction, "Attack_2");
			attack_duration.Start();
		}
	}
	
	private void _on_attack_zone_body_exited(Node2D body) {
		if (body is Player player) {
			Player_in_zone = false;
		}
	}
	
	private void _on_attack_duration_timeout() {
		// only new attack after timeout if p in sonze
		if(Player_in_zone && Hero != null) {
			Vector2 direction = (Hero.Position - Position).Normalized();
			UpdateAnimation(direction, "Attack_2");
			attack_duration.Start(); 
		} 
		else {
			IsAttacking = false;			
		}
	}
	private void _on_take_damage_cooldown_timeout() {
		Can_take_dmg = true;
		IsTakingDmg = false;
	}
*/
