using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public partial class Player : CharacterBody2D
{
	public Vector2 AimDirection;
	//itlsef
	public static Player PlayerInstance { get; private set; }

	private PlayerMovement _playerMovement;
	public PlayerMovement Movement => _playerMovement;

	// current equipped node
	public Node2D EquippedNode {get; private set;}
	public string EquippedItemId {get; private set;}
	
	// vars
	public int Speed = 75;
	public int BaseSpeed = 75;
	public int RunSpeed = 150;
	public double Health = 1000;
	public double Stamina = 100000;
	public float JumpPower = 200;
	public float HorizontalPower = 25;
	
	private float _attackHoldTime = 0f;
	private float _heavyAttackThreshold = 0.5f;
	
	public float height = -5f;
	public float sideHeight = -5f;
	public float verticalVelocity = 0f;
	public float horizontalVelocity = 0.15f;
	public float gravity = 750f; 
	
	public bool isRunning = false;
	public bool isJumping = false;
	
	private Vector2 _inputDirection = Vector2.Zero;
	
	private String _direction = "Down";
	
	public bool PlayerAlive = true;
	public bool _attackInProgress = false;
	public bool Enemy_in_range = false;
	public bool Enemy_attack_cooldown = true;
	public bool Can_take_dmg = true;
	public bool IsTakingDmg = false;
	
	// timers
	private Timer _attack_cooldown;
	private Timer _deal_attack_timer;
	private Timer _take_damage_cooldown;
	private Timer _regen_timer;
	private Timer _regen_stamina_timer;
	
	// healthbar
	private ProgressBar _healthBar;
	// stamina
	private ProgressBar _staminaBar;
	
	// sprite/node/anim
	private AnimationPlayer _animationPlayer;
	private Node2D _weaponPivot;
	private Sprite2D _playerSprite;
	
	public Vector2 basePosition;
	
	// facing direction for mining
	public Vector2I FacingDirection {get; private set;} = Vector2I.Down;
	// FacingDirection = new Vector2I(Mathf.Sign(input.X), Mathf.Sign(input.Y));
	// Inventory
	public Inventory Inventory { get; private set; }
	
	// cam
	private Camera2D _camera;
	public bool IsUnderground = false;

	public override void _Ready()
	{
		ItemDatabase.LoadAllItems();
		CraftingDatabase.LoadRecipes();

		PlayerInstance = this;

		Node2D visualRoot = GetNode<Node2D>("VisualRoot");
		_playerMovement = new PlayerMovement(this, visualRoot);

		// initialize the world maanger to give it tile map
		if (!WorldManager.IsInitialized)
		{
			var groundTilemap = GetParent().GetNode<TileMapLayer>("ground");
			var crackedTilemap = GetParent().GetNode<TileMapLayer>("tile_crack");
			var f = GetParent().GetNode<TileMapLayer>("flowers");
			var p = GetParent().GetNode<Node2D>("placeables");
			var m = GetParent().GetNode<TileMapLayer>("mountains");
			var o = GetParent().GetNode<TileMapLayer>("objects");
			var baseUnderground = GetParent().GetNode<TileMapLayer>("base_underground");
			var oreUnderground = GetParent().GetNode<TileMapLayer>("ore_underground");
			WorldManager.Initialize(groundTilemap, crackedTilemap, f, m, o, baseUnderground, oreUnderground, p);
		}


		Inventory = GetNode<Inventory>("Inventory");
		_animationPlayer = GetNode<AnimationPlayer>("Sprites/PlayerAnimation");
		_playerSprite = GetNode<Sprite2D>("Sprites/BodySprite");
		_weaponPivot = GetNode<Node2D>("Sprites/WeaponPivot");
		_attack_cooldown = GetNode<Timer>("attack_cooldown");
		_deal_attack_timer = GetNode<Timer>("deal_attack_timer");
		_take_damage_cooldown = GetNode<Timer>("take_damage_cooldown");
		_healthBar = GetNode<ProgressBar>("healthbar");
		_staminaBar = GetNode<ProgressBar>("staminabar");
		_regen_timer = GetNode<Timer>("regen_timer");
		_regen_stamina_timer = GetNode<Timer>("regen_stamina_timer");

		_camera = GetNode<Camera2D>("world_camera");

		_regen_timer.Start();
		_healthBar.MaxValue = Health;
		_healthBar.Value = Health;
		_staminaBar.MaxValue = Stamina;
		_staminaBar.Value = Stamina;

		PlayIdleAnimation();
	}

	public override void _ExitTree()
	{
		if(PlayerInstance == this)
		{
			PlayerInstance = null;
		}
	}

	public override void _PhysicsProcess(double delta) {
		HandleUndergroundCheck();
		HandleInput();
		HandleJump();
		HandleStamina(delta);
   		if (IsUnderground) {
			// Underground: PLATFORMER MOVEMENT
			if (!IsOnFloor()) {
				Velocity += GetGravity() * (float)delta;
			}
			Velocity = new Vector2(_inputDirection.X * Speed, Velocity.Y);
   		} 
		else {
			// Surface: TOP DOWN MOVEMENT
			HandleSurfaceJump((float)delta);
			Velocity = _inputDirection.Normalized() * Speed;
		}
		MoveAndSlide();
	}
	
	// player interactor?
	public void SetUndergroundMode(bool isUnderground) {
		IsUnderground = isUnderground;
	
		if (IsUnderground) {
		// Underground: side-scrolling collision layer
		CollisionMask = 2; // Underground tiles collision layer
		
		// Adjust camera for side-scrolling view
		// _camera.Zoom = new Vector2(3.0f, 3.0f); // Closer zoom
		// _camera.Offset = new Vector2(0, -50); // Show more ground below player
		
		GD.Print("Player switched to underground mode");
		}
		else {
		// Surface: top-down collision layer  
		CollisionMask = 1; // Surface tiles collision layer
		
		// Adjust camera for top-down view
		_camera.Zoom = new Vector2(2.0f, 2.0f); // Normal zoom
		_camera.Offset = Vector2.Zero; // Center on player
		
		GD.Print("Player switched to surface mode");
		}
	}
	
	// player interactor?
	private void CheckForEntrances() {
		if (WorldManager.IsUnderground) return;
		Vector2I currentTile = WorldManager.WorldToTilePos(GlobalPosition);
		GD.Print($"Current Tile Surface: {currentTile}");
		var ent = WorldManager.HasEntranceAt(currentTile);
		GD.Print($"Entrance: {ent}");
		if (ent)
		{
			WorldManager.SurfaceToUnderground(currentTile, this);
		}
	}
	
	// player interactor?
	private void CheckForExits() {
		if (!WorldManager.IsUnderground) return; 
		Vector2I currentTile = WorldManager.WorldToTilePos(GlobalPosition);
		GD.Print($"Current Tile Underground: {currentTile}");
		var ex = WorldManager.HasExitAt(currentTile);
		GD.Print($"Exit: {ex}");
		if (ex) {
			WorldManager.UndergroundToSurface(currentTile, this);
	}
	}
	
	// player interactor?
	private void HandleUndergroundCheck() {
		if (Input.IsActionJustPressed("Interact")) {
			if(WorldManager.IsUnderground) {
				GD.Print("underground");
				CheckForExits();
			}
			else {
				GD.Print("surface");
				CheckForEntrances();
			}
		}
	}
	
	// player input
	private void HandleInput()
	{
		_inputDirection = Vector2.Zero;
		isRunning = Input.IsActionPressed("run") && Stamina > 0;
		Speed = isRunning ? RunSpeed : BaseSpeed;

		if (Input.IsActionPressed("ui_right"))
		{
			_inputDirection.X += 1;
			FacingDirection = Vector2I.Right;
			_direction = "Side";
			_playerSprite.FlipH = false;
		}
		else if (Input.IsActionPressed("ui_left"))
		{
			_inputDirection.X -= 1;
			FacingDirection = Vector2I.Left;
			_direction = "Side";
			_playerSprite.FlipH = true;
		}
		if (Input.IsActionPressed("ui_down"))
		{
			_inputDirection.Y += 1;
			FacingDirection = Vector2I.Down;
			_direction = "Down";
			_playerSprite.FlipH = false;
		}
		else if (Input.IsActionPressed("ui_up"))
		{
			_inputDirection.Y -= 1;
			FacingDirection = Vector2I.Up;
			_direction = "Up";
			_playerSprite.FlipH = false;
		}

		if (_attackInProgress)
		{
			return;
		}

		if (_inputDirection != Vector2.Zero)
		{
			if (isRunning)
			{
				PlayRunAnimation();
			}
			else
			{
				PlayWalkAnimation();
			}
		}
		else
		{
			PlayIdleAnimation();
		}

		if(EquippedNode is IChargeable chargeable && !_attackInProgress && !isRunning)
		{
			if (Input.IsActionJustPressed("PrimaryAction"))
			{
				chargeable.DrawWeapon(this);
			}
			if (Input.IsActionPressed("PrimaryAction"))
			{
				
			}
			if (Input.IsActionJustReleased("PrimaryAction"))
			{
				chargeable.Release(this);
			}
		}
		
		if (EquippedNode is IWeapon weapon && !_attackInProgress && !isRunning) {
			if (Input.IsActionPressed("PrimaryAction")) {
				_attackHoldTime += (float)GetPhysicsProcessDeltaTime();
			}

			if (Input.IsActionJustReleased("PrimaryAction")) {
				_attackInProgress = true;

				if (_attackHoldTime >= _heavyAttackThreshold) {
					weapon.HeavyAttack(this);
				} else {
		   	 		weapon.LightAttack(this);
	   			}
				_attackHoldTime = 0f;
			}
		}

		// "attack" basically prime Use of item (e)
		// the above handles weapons, this handles tools/placeables?
		if (Input.IsActionJustPressed("PrimaryAction") && !_attackInProgress && !isRunning && EquippedNode is IUsable usable)
		{
			_attackInProgress = true;
			usable.Use(this);
		}

		// weapon switching
	}
	
	// (stats)
	private void HandleStamina(double delta) {
	// drain stamina if running and moving
	if (isRunning && _inputDirection != Vector2.Zero && Stamina > 0) {
		Stamina -= 30 * delta; 
		Stamina = Math.Max(Stamina, 0);
		_staminaBar.Value = Stamina;

		// stop regen while draining
		if (!_regen_stamina_timer.IsStopped())
			_regen_stamina_timer.Stop();
	}
	// start regen only when not running and stamina isn't full
	else if (!isRunning && Stamina < _staminaBar.MaxValue && _regen_stamina_timer.IsStopped()) {
		_regen_stamina_timer.Start();
		}
	}
	
	// player movements
	private void HandleJump() {
		if (Input.IsActionJustPressed("jump") && IsOnFloor()) {
		//	StartJump();
		}
	}
	
	// player movements
	private void HandleSurfaceJump(float delta) {
		if (Input.IsActionJustPressed("jump") && !isJumping) {
			verticalVelocity = JumpPower;
			horizontalVelocity = HorizontalPower;
			isJumping = true;
			basePosition = Position;
		}

		if (isJumping) {
			verticalVelocity -= gravity * delta;
			height += verticalVelocity * delta;
			sideHeight += horizontalVelocity * delta;

			if (height <= 0f) {
				height = 0f;
				verticalVelocity = 0f;
				horizontalVelocity = 0f;
				isJumping = false;
			}

			Position = basePosition + new Vector2(sideHeight, -height);
		}
	}
	
	// player movements
	private void StartJump() {
		// isJumping = true;
		// Velocity = new Vector2(Velocity.X, -JumpPower);
		PlayRunAnimation();
	}

	// player movement (or animation)
	public bool GetFacingLeft() {
		return _playerSprite.FlipH;
	}
	
	// duration change later?


	// animation controller
	public void PlayerAnimation(string actionName, float duration = 0.45f) {
		string dir = _direction;
		if(_direction == "Side" && _playerSprite.FlipH) {
			dir = "LeftSide";
		}
		
		string animationName = $"{dir}{actionName}";
		_animationPlayer.Play(animationName);
		if (EquippedNode != null) {
			
			var swordSprite = EquippedNode.GetNodeOrNull<Sprite2D>("WeaponSprite");
			if (swordSprite != null) {
				swordSprite.FlipV = (animationName == "LeftSidePierce");
		}
	}
		GetTree().CreateTimer(duration).Timeout += () => { 
			_attackInProgress = false; 
			if (EquippedNode != null) {
				var swordSprite = EquippedNode.GetNodeOrNull<Sprite2D>("WeaponSprite");
				if (swordSprite != null) {
					swordSprite.FlipV = false;
			}
		}
		};
	}
	
	// add bool, when false starts cooldown (invulnerability period for player)
	// llava, mob spamming where players needs time between next damage taken
	// default, player can be hit multiple times no cooldown

	// (stats)
	public void Take_damage(int dmg, bool force = false) {
		if((Can_take_dmg && !IsTakingDmg && PlayerAlive) || force) {
			Health -= dmg;
			Health = Math.Max(Health, 0);
			
			_healthBar.Value = Health;
			
			GD.Print("Player health: ", Health);
			
			if(!force) {
				Can_take_dmg = false;
				IsTakingDmg = true;
				_take_damage_cooldown.Start();	
			}
			
			if(Health <= 0) {
				PlayerAlive = false;
				IsTakingDmg = false;
				QueueFree();
			}
		}
	}
	// attack IP check (stats)
	private void _on_deal_attack_timer_timeout() {
		_deal_attack_timer.Start();
		_attackInProgress = false;
	}
	
	// taking dmg check (stats)
	private void _on_take_damage_cooldown_timeout() {
		Can_take_dmg = true;
		IsTakingDmg = false;
	}
	
	// health regen (stats)
	private void _on_regen_timer_timeout() {
		if (!PlayerAlive) return;
		
		double regen = 5;
		Health = Math.Min(Health + regen, _healthBar.MaxValue);
		_healthBar.Value = Health;
	}
	
	// stamina regen (stats)
	private void _on_regen_stamina_timer_timeout() {
		if (!PlayerAlive) return;
		
		double regen = 5;
		Stamina = Math.Min(Stamina+ regen, _staminaBar.MaxValue);
		_staminaBar.Value = Stamina;
	}
	
	// equipment
	public void EquipFromHotbar(InventoryEntry item) {
		if (item == null || EquippedItemId == item.Item.ItemId) {
			return;
		}
		
		EquippedNode = EquipmentManager.EquipItem(item, _weaponPivot, this, EquippedNode);
		if (EquippedNode == null) {
			GD.PrintErr("Failed to equip item.");
			return;
		}
		EquippedItemId = item.Item.ItemId;	
	}
	
	// equipment
	public void UnequipWeapon() {
		EquipmentManager.UnequipItem(EquippedNode);
		EquippedNode = null;
		EquippedItemId = "";
	}	
	
	// animation
	private void PlayWalkAnimation() {
		if(EquippedNode != null) {
			EquippedNode.Show();
		}
		_animationPlayer.Play($"{_direction}Walk");
	}
	
	// animation
	private void PlayRunAnimation() {
		if(EquippedNode != null) {
			EquippedNode.Hide();
		}
		_animationPlayer.Play($"{_direction}Run");
	}
	
	// animation
	private void PlayIdleAnimation() {
		if(EquippedNode != null) {
			EquippedNode.Show();
		}
		_animationPlayer.Play($"{_direction}Idle");
	}
}
