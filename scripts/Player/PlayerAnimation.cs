
using System;
using Godot;

public partial class PlayerAnimation : Node
{
    public enum FacingDirection
    {
        Up,
        Down,
        Side
    }

    public FacingDirection Direction {get; private set;} = FacingDirection.Down;
    private AnimatedSprite2D _sprite;
    private AnimationPlayer _animationPlayer;

    private PlayerMovement _movement;
    private PlayerEquipment _equipment;
    private PlayerCombat _combat;

    private string _currentAnimation = "";

    public override void _Ready()
    {
        Player player = GetParent<Player>();
        _sprite = player.GetNode<AnimatedSprite2D>("Sprites/BodySprite");
        _animationPlayer = player.GetNode<AnimationPlayer>("Sprites/PlayerAnimation");

        _movement = player.Movement;
        _combat = player.GetNode<PlayerCombat>("Combat");
        _equipment = player.GetNode<PlayerEquipment>("Equipment");
    }

    public void Update()
    {
        // Update Player Facing Direction
        UpdateDirection();

        if(_combat.State == PlayerCombat.ActionState.Using)
        {
            UpdateUse();
            return;
        }

        // Update PLayer Combat
        if (_combat.IsAttacking)
        {
            UpdateCombat();
            return;
        }
        // Update Player Charge
        if (_combat.IsCharging)
        {
            UpdateCharging();
            return;
        }

        // Update Player Movement
        UpdateMovement();
    }

    private void SetEquippedVisible(bool visible)
    {
        Node2D equipped = _equipment.EquippedNode;

        if(equipped == null)
        {
            return;
        }

        if (visible)
        {
            equipped.Show();
        }
        else
        {
            equipped.Hide();
        }
    }

    private void UpdateDirection()
    {
        Vector2 direction;

        // Sprite faces entity being attacked (or charging)
        if(_combat.IsAttacking || _combat.IsCharging)
        {
            direction = _combat.AimDirection;
        }
        // Otherwise, sprite faces the direction player is moving
        else
        {
            direction = _movement.InputDirection;
        }

        if (direction.IsZeroApprox())
        {
            return;
        }



        if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
        {
            Direction = FacingDirection.Side;
            _sprite.FlipH = direction.X < 0;
        } 
        else if (direction.Y < 0)
        {
            Direction = FacingDirection.Up;
            _sprite.FlipH = false;
        }
        else
        {
            Direction = FacingDirection.Down;
            _sprite.FlipH = false;
        }
    }

    private void UpdateUse()
    {
        ToolBase Tool = _equipment.GetEquipped<ToolBase>();
        if (Tool == null)
        {
            return;
        }
        string animation = Tool.UseAnimation.ToString();
        Play(animation);
    }
    private void UpdateCharging()
    {
        Play("Charge");
    }

    private void UpdateMovement()
    {
        if (_movement.InputDirection.IsZeroApprox())
        {
            SetEquippedVisible(true);
            Play("Idle");
            return;
        }
        
        if (_movement.IsRunning)
        {
            SetEquippedVisible(false);
            Play("Run");
            return;
        }
        
        SetEquippedVisible(true);
        Play("Walk");
    }

    private void UpdateCombat()
    {
        switch (_combat.State)
        {
            case PlayerCombat.ActionState.LightAttack:
                Play("Pierce");
                break;
            case PlayerCombat.ActionState.HeavyAttack:
                Play("Heavy");
                break;
        }
    }

    private void Play(string action)
    {
        string animationName = $"{Direction}{action}";
        if (_animationPlayer.CurrentAnimation == animationName)
        {
            return;
        }
        _animationPlayer.Play(animationName);
    }
}