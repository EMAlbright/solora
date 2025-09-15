using Godot;
using System;

public abstract partial class BaseEnemy : CharacterBody2D
{
	//base stats
	[Export] public int Speed {get; set;} = 35;
	[Export] public float MinDistance {get; set;} = 40.0f;
	[Export] public int MaxHealth {get; set;} = 100;
	[Export] public int AttackDamage {get; set;} = 15;
	// cooldowns maybe
	// [Export]
	// [Export]
	
	// runtime props
	public int Health {get; set;}
	public bool Chase {get; protected set;} = false;
	public Player Hero {get; protected set;} = null;
	public bool PlayerInZone {get; protected set;} = false;
	public bool CanTakeDamage {get; protected set;} = true;
	public bool IsTakingDamage {get; protected set;} = false;
	public bool IsAttacking {get; protected set;} = false;
	private string _currentAnimation = "";
	
	// Timers
	protected Timer takeDamageCooldown;
	protected Timer attackDuration;
	
	// Components
	protected AnimationPlayer animationPlayer;
	protected Sprite2D enemySprite;
	protected Area2D attackHitbox;
	
	private Vector2 _lastDirection = Vector2.Down;
	
	public override void _Ready() {
		Health = MaxHealth;
		
		// get components (assume each enemy has specific namings)
		takeDamageCooldown = GetNode<Timer>("take_damage_cooldown");
		attackDuration = GetNode<Timer>("attack_duration");
		
		InitializeComponents();
	}
	
	public override void _PhysicsProcess(double delta) {
		if (!IsAttacking) {
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
				PlayIdleAnimation();
			}
		}
		else {
			// No movement during attack
			Velocity = Vector2.Zero;
		}
		AvoidEnemyCollision();
		MoveAndSlide();
	}
	
	// Abstract - implement in child
	protected abstract void InitializeComponents();
	protected abstract string GetAttackAnimationName();
	
	protected virtual void UpdateAnimation(Vector2 dir, string animType) {
		if (dir == Vector2.Zero) {
			return;
		} else {
			_lastDirection = dir; 
		}

		float angle = Mathf.Atan2(dir.Y, dir.X);
		
		string anim = $"Down{animType}";
		if (angle >= -0.3927f && angle < 0.3927f) {
			enemySprite.FlipH = false;
			anim = $"Side{animType}";	
		}
		else if (angle >= 0.3927f && angle < 1.1781f) {
			enemySprite.FlipH = false;
			anim = $"SideDown{animType}";	
		}
		else if (angle >= 1.1781f && angle < 1.9635f) {
			enemySprite.FlipH = false;
			anim = $"Down{animType}";	
		}
		else if (angle >= 1.9635f && angle < 2.7489f) {
			// flip bear sprite
			enemySprite.FlipH = true;
			anim = $"SideDown{animType}";	
		}
		else if (angle >= 2.7489f || angle < -2.7489f) {
			// flip bear sprite
			enemySprite.FlipH = true;
			anim = $"Side{animType}"; // left	
		}
		else if (angle >= -2.7489f && angle < -1.9635f) {
			enemySprite.FlipH = false;
			anim = $"SideUp{animType}";			
		}

		else if (angle >= -1.9635f && angle < -1.1781f) {
			enemySprite.FlipH = false;
			anim = $"Up{animType}";			
		}

		else if (angle >= -1.1781f && angle < -0.3927f) {
			enemySprite.FlipH = true;
			anim = $"SideUp{animType}";	
		}
		
		// for flipping area2d for left attacks
		UpdateAttackHitboxPosition();
		
		// if anim not playing, play new anim
		if (_currentAnimation != anim) {
			animationPlayer.Play(anim);
			_currentAnimation = anim;
		}
	}
	
	// for attack hitbox when attack to the left
	protected virtual void UpdateAttackHitboxPosition() {
		if (enemySprite != null && attackHitbox != null) {
			if (enemySprite.FlipH)
			{
				attackHitbox.Scale = new Vector2(-1, 1);
			}
			else
			{
				attackHitbox.Scale = new Vector2(1, 1);
			}
		}
	}
	
	protected virtual void AvoidEnemyCollision() {
		Vector2 separation = Vector2.Zero;
		int nearbyCount = 0;

		foreach (Node enemy in GetTree().GetNodesInGroup("enemy")) {
   		if (enemy == this) continue;

			var other = enemy as CharacterBody2D;
			float distance = GlobalPosition.DistanceTo(other.GlobalPosition);
	
			if (distance < 50.0f) {
				Vector2 away = (GlobalPosition - other.GlobalPosition).Normalized();
				separation += away / distance; 
				nearbyCount++;
			}
		}
		
		if (Hero != null) {
			float playerDist = GlobalPosition.DistanceTo(Hero.GlobalPosition);
			if (playerDist < 30.0f) {
				Vector2 awayFromPlayer = (GlobalPosition - Hero.GlobalPosition).Normalized();
				separation += awayFromPlayer / playerDist;
				nearbyCount++;
			}
		}

		if (nearbyCount > 0) {
			separation /= nearbyCount;
			Velocity += separation * 1000.0f; 
		}
	}
	
	public virtual void TakeDamage(int damage) {
		if (CanTakeDamage && !IsTakingDamage) {
			Health -= damage;
			GD.Print($"{GetType().Name} Health: {Health}");
			CanTakeDamage = false;
			IsTakingDamage = true;
			takeDamageCooldown.Start();
			
			// anim (or other) when enemy takes dmg
			OnDamageTaken(damage);
			
			if (Health <= 0) {
				OnDeath();
			}
		}
	}
	
	// anim (or other) when enemy takes dmg
	protected virtual void OnDamageTaken(int damage) {
		return;
	}
	
	protected virtual void OnDeath() {
		IsTakingDamage = false;
		QueueFree();
	}
	
	// SIGNALS
	
	// players hitbox enters enemies attack hitbox then call take dmg func on player
	private void _on_attack_hitbox_area_entered(Area2D area) {
		if (area.Name == "player_hitbox" && area.GetParent() is Player player) {
			// player takes damage no cooldown
			player.Take_damage(AttackDamage, true);
			GD.Print($"{GetType().Name} hit player for {AttackDamage} damage!");
		}
	}
	
	// player enters attack zone of enemy
	private void _on_attack_zone_body_entered(Node2D body) {
		if (body is Player player && Hero != null && !IsAttacking) {
			Vector2 direction = (Hero.Position - Position).Normalized();
			IsAttacking = true;
			PlayerInZone = true;
			// implement get attack animation name in child class
			// could randomize attack moves
			UpdateAnimation(direction, GetAttackAnimationName());
			attackDuration.Start();
		}
	}
	
	// player leaves attack zone of enemy
	private void _on_attack_zone_body_exited(Node2D body) {
		if (body is Player player) {
			PlayerInZone = false;
		}
	}
	
	// player enter detection zone
	private void _on_detection_area_body_entered(Node2D body) {
		if (body is Player player) {
			Hero = player;
			Chase = true;	
		}
	}
	
	//player exit detection zone
	private void _on_detection_area_body_exited(Node2D body) {
		if (body == Hero) {
			Hero = null;
			Chase = false;
		}
	}
	
	// timer for attacking
	private void _on_attack_duration_timeout() {
		// only new attack after timeout if p in sonze
		if(PlayerInZone && Hero != null) {
			Vector2 direction = (Hero.Position - Position).Normalized();
			UpdateAnimation(direction, GetAttackAnimationName());
			attackDuration.Start(); 
		} 
		else {
			IsAttacking = false;			
		}
	}
	
	// attack cooldown
	private void _on_take_damage_cooldown_timeout() {
		CanTakeDamage = true;
		IsTakingDamage = false;
	}
		
	// Virtual method for default idle animation
	protected virtual void PlayIdleAnimation() {
		if (animationPlayer != null)
		{
			UpdateAnimation(_lastDirection, "Idle");
		}
	}
	
}
