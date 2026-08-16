using Godot;

public sealed class PlayerMovement
{
    private readonly CharacterBody2D _body;
    private readonly Node2D _visualRoot;

    // Private Constants
    private const float BaseSpeed = 75f;
	private const float RunSpeed = 150f;
    private const float UndergroundJumpSpeed = 200f;
    private const float SurfaceJumpSpeed = 200f;
    private const float SurfaceJumpGravity = 750f; 

    // Privates
    private float _surfaceJumpHeight;
    private float _surfaceJumpVelocity;

    private float _speedBonus;
    private float _speedPenalty;
    private float _speedMultiplier = 1f;
    private Vector2 _inputDirection = Vector2.Zero;


    // Publics
	public bool IsRunning {get; private set; }
    public bool IsSurfaceJumping {get; private set; }
    public float CurrentSpeed {get; private set; } = BaseSpeed;

    // Arrow Funcs
    public Vector2 InputDirection => _inputDirection;
    public bool IsUndergroundAirborne => !_body.IsOnFloor();
    public bool IsUndergroundRising => 
        IsUndergroundAirborne && _body.Velocity.Y < 0f;
    public bool IsUndergroundFalling =>
        IsUndergroundAirborne && _body.Velocity.Y > 0f;
    public bool IsMoving => !_inputDirection.IsZeroApprox();

    public PlayerMovement(CharacterBody2D body, Node2D visualRoot)
    {
        _body = body;
        _visualRoot = visualRoot;
    }

    public void Update(
        Vector2 inputDirection,
        bool runHeld,
        bool jumpPressed,
        bool canRun,
        MovementMode movementMode,
        double delta
    ) {
        _inputDirection = inputDirection;

        UpdateRunningState(runHeld, canRun);
        UpdateCurrentSpeed();

        switch (movementMode)
        {
            case MovementMode.Surface:
                UpdateSurfaceMovement();
                UpdateSurfaceJump(jumpPressed, (float)delta);
                break;

            case MovementMode.Underground:
                UpdateUndergroundMovement((float)delta);

                if (jumpPressed)
                {
                    TryUndergroundJump();
                }

                break;
        }
    }

    private void UpdateRunningState(bool runHeld, bool canRun)
    {
        IsRunning = runHeld && canRun && IsMoving;
    }

    private void UpdateCurrentSpeed()
    {
        float startingSpeed = IsRunning ? RunSpeed : BaseSpeed;
        float modifiedSpeed = startingSpeed + _speedBonus - _speedPenalty;

        CurrentSpeed = Mathf.Max(modifiedSpeed * _speedMultiplier, 0f);
    }
    private void UpdateUndergroundMovement(float delta)
    {
        Vector2 velocity = _body.Velocity;
        if (!_body.IsOnFloor())
        {
            velocity += _body.GetGravity() * delta;
        }

        velocity.X =
            _inputDirection.X * CurrentSpeed;

        _body.Velocity = velocity;
    }

    private void UpdateSurfaceMovement()
    {
        _body.Velocity = _inputDirection * CurrentSpeed;
    }

    private void TryUndergroundJump()
    {
        if(!_body.IsOnFloor())
        {
            return; 
        }

        Vector2 velocity = _body.Velocity;
        velocity.Y = -UndergroundJumpSpeed;

        _body.Velocity = velocity;    
    }

    private void UpdateSurfaceJump(bool jumpPressed, float delta)
    {
        // TODO: Probably a way around having the surface be detectable
        // Right now, floor is just visual tiles so no way to detect position based on tiles
        // We can do this underground since it's a platformer and all tiles have collision box
        if (jumpPressed && !IsSurfaceJumping)
        {
            StartSurfaceJump();
        }

        if (!IsSurfaceJumping)
        {
            return;
        }

        _surfaceJumpVelocity -= SurfaceJumpGravity * delta;
        _surfaceJumpHeight += _surfaceJumpVelocity * delta;

        if(_surfaceJumpHeight <= 0f)
        {
            FinishSurfaceJump();
        }

        UpdateVisualHeight();
    }

    private void StartSurfaceJump()
    {
        IsSurfaceJumping = true;

        _surfaceJumpHeight = 0f;
        _surfaceJumpVelocity = SurfaceJumpSpeed;
    }

    private void FinishSurfaceJump()
    {
        IsSurfaceJumping = false;

        _surfaceJumpHeight = 0f;
        _surfaceJumpVelocity = 0f;
    }
    private void UpdateVisualHeight()
    {
        _visualRoot.Position = new Vector2(
            _visualRoot.Position.X,
            -_surfaceJumpHeight
        );
    }
    public void SetSpeedBonus(float bonus)
    {
        _speedBonus = Mathf.Max(bonus, 0f);
    }
    public void SetSpeedPenalty(float penalty)
    {
        _speedPenalty = Mathf.Max(penalty, 0f);
    }
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = Mathf.Max(multiplier, 0f);
    }

    public void ResetSpeedMods()
    {
        _speedBonus = 0f;
        _speedPenalty = 0f;
        _speedMultiplier = 1f;
    }
    // Stop movement
    public void Stop()
    {
        _inputDirection = Vector2.Zero;
        IsRunning = false;
        CurrentSpeed = BaseSpeed;

        _body.Velocity = new Vector2(0f, _body.Velocity.Y);

    }
}
