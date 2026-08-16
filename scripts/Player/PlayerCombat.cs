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
    public bool IsChargin => State == CombatState.Charging;
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

    public void UpdateAim(Vector2 direction)
    {
        if (!direction.IsZeroApprox())
        {
            AimDirection = direction.Normalized();
        }
    }
}