using Godot;

public partial class PlayerCombat : Node
{
    public enum CombatState
    {
        Idle,
        LightAttack,
        HeavyAttack,
        Charging
    }

    private const float HeavyAttackThreshold = 0.5f;

    public CombatState State {get; private set;} = CombatState.Idle;

    public bool IsAttacking => State == CombatState.LightAttack || State == CombatState.HeavyAttack;
    public bool IsCharging => State == CombatState.Charging;
    public Vector2 AimDirection {get; private set;} = Vector2.Down;
    private float _attackHoldTime;

    private Player _player;
    private PlayerEquipment _equipment;
    private ActorStats _stats;

    public override void _Ready()
    {
        _player = GetParent<Player>();

        _equipment = _player.GetNode<PlayerEquipment>("Equipment");
        _stats = _player.GetNode<ActorStats>("ActorStats");
    }

    public void Update(PlayerInputFrame input, double delta)
    {
        UpdateAim(input.AimDirection);

        if(IsAttacking) return;

        if(_equipment.GetEquipped<IChargeable>() is IChargeable chargeable)
        {
            HandleChargeable(chargeable, input, (float)delta);
            return;
        }

        if(_equipment.GetEquipped<IWeapon>() is IWeapon weapon)
        {
            HandleWeapon(weapon, input, (float)delta);
            return;
        }
        if(_equipment.GetEquipped<IUsable>() is IUsable usable)
        {
            HandleUsable(usable, input);
            return;
        }
    }

    private void UpdateAim(Vector2 direction)
    {
        if (!direction.IsZeroApprox())
        {
            AimDirection = direction.Normalized();
        }
    }

    private void HandleWeapon(IWeapon weapon, PlayerInputFrame input, float delta)
    {
        if (input.PrimaryHeld)
        {
            _attackHoldTime += delta;
        }
        if (!input.PrimaryReleased)
        {
            return;
        }

        AttackContext context = CreateAttackContext();
        
        bool heavyAttack = _attackHoldTime >= HeavyAttackThreshold;
        _attackHoldTime = 0f;

        if(heavyAttack)
        {
            TryHeavyAttack(weapon, context);
        }
        else
        {
            TryLightAttack(weapon, context);
        }
    }
    private void TryHeavyAttack(IWeapon weapon, AttackContext context)
    {
        State = CombatState.HeavyAttack;
        weapon.HeavyAttack(context);
        StartAttackTimer(
            weapon.HeavyAttackDuration
        );
    }

    private void TryLightAttack(IWeapon weapon, AttackContext context)
    {
        State = CombatState.LightAttack;
        weapon.LightAttack(context);
        StartAttackTimer(
            weapon.LightAttackDuration
        );
    }

    private void HandleChargeable(IChargeable chargeable, PlayerInputFrame input, float delta)
    {
        AttackContext context = CreateAttackContext();

        if (input.PrimaryPressed)
        {
            State = CombatState.Charging;
            chargeable.BeginCharge(context);
        }
        if (input.PrimaryHeld && IsCharging)
        {
            chargeable.Charge(context, delta);
        }
        if(input.PrimaryReleased && IsCharging)
        {
            chargeable.Release(context);
            State = CombatState.Idle;
        }
    }

    private void HandleUsable(IUsable usable, PlayerInputFrame input)
    {
        if (!input.PrimaryPressed)
        {
            return;
        }

        UseContext context = CreateUseContext();

        usable.Use(context);
    }

    private AttackContext CreateAttackContext()
    {
        return new AttackContext (
            Owner: _player,
            Origin: _player.GlobalPosition,
            Direction: AimDirection,
            Stats: _stats
        );
    }

    private UseContext CreateUseContext()
    {
        return new UseContext (
            Owner: _player,
            Origin: _player.GlobalPosition,
            Direction: AimDirection
        );
    }

    private async void StartAttackTimer(float duration)
    {
        await ToSignal(
            GetTree().CreateTimer(duration), 
            SceneTreeTimer.SignalName.Timeout
        );
        State = CombatState.Idle;
    }
}