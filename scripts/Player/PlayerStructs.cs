using Godot;

public readonly record struct PlayerInputFrame
(
    Vector2 MoveDirection,
    Vector2 AimDirection,
    bool RunHeld,
    bool JumpPressed,
    bool InteractPressed,
    bool PrimaryPressed,
    bool PrimaryHeld,
    bool PrimaryReleased
);

public readonly record struct AttackContext
(
    Node2D Owner,
    Vector2 Origin,
    Vector2 Direction,
    ActorStats Stats
);

public readonly record struct EquipmentContext(
    Node2D Owner,
    InventoryEntry Entry
);

public readonly record struct UseContext(
    Node2D Owner,
    Vector2 Origin,
    Vector2 Direction
);

public readonly record struct InteractionContext(
    Node2D Owner,
    Vector2 Direction
);