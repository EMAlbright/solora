
using Godot;

public partial class PlayerAnimation : Node
{
    private AnimatedSprite2D _sprite;
    private AnimationPlayer _animationPlayer;

    private PlayerMovement _movement;
    private PlayerEquipment _equipment;
    private PlayerCombat _combat;

    private string _currentAnimation = "";

    public override void _Ready()
    {
        Player player = GetParent<Player>();
        _sprite = player.GetNode<AnimatedSprite2D>("VisualRoot/AnimatedSprite2D");

        _movement = player.Movement;
        _combat = player.GetNode<PlayerCombat>("Combat");
        _equipment = player.GetNode<PlayerEquipment>("Equipment");
    }

    public void Update()
    {
        // Update Player Facing Direction
        UpdateDirection();

        // Update PLayer Combat
        if (_combat.IsAttacking)
        {
            UpdateCombat();
        }
        // Update Player Charge
        if (_combat.IsCharging)
        {
            UpdateCharging();
        }

        // Update Player Movement
        UpdateMovement();
    }

    private void UpdateDirection()
    {
        Vector2 _direction = _combat.AimDirection;

        if (_direction.X < 0)
        {
            _sprite.FlipH = true;
        } 
        else if (_direction.X > 0)
        {
            _sprite.FlipH = false;
        }
    }

    private void UpdateCharging()
    {
        
    }

    private void UpdateMovement()
    {
        
    }

    private void UpdateCombat()
    {
        switch (_combat.State)
        {
            case PlayerCombat.CombatState.LightAttack:
                Play("Pierce");
                break;
            case PlayerCombat.CombatState.HeavyAttack:
                Play("Heavy");
                break;
        }
    }

    private void Play(string _animation)
    {
        if (_animation == _currentAnimation)
        {
            return;
        }
        _currentAnimation = _animation;
        _sprite.Play(_animation);
    }
}