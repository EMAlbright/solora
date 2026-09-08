using Godot;

public partial class Player : CharacterBody2D
{
    public static Player PlayerInstance { get; private set; }

    public PlayerMovement Movement { get; private set; }

    private readonly PlayerInput _input = new();

    private PlayerCombat _combat;
    private PlayerAnimation _animation;
    private PlayerInteractor _interactor;
    private PlayerEquipment _equipment;
    private ActorStats _stats;

    private Camera2D _camera;

    public bool IsUnderground { get; private set; }

    public override void _Ready()
    {
        PlayerInstance = this;

        Node2D visualRoot = GetNode<Node2D>("VisualRoot");

        Movement = new PlayerMovement(
            this,
            visualRoot
        );

        _combat = GetNode<PlayerCombat>("Combat");
        _animation = GetNode<PlayerAnimation>("Animation");
        _interactor = GetNode<PlayerInteractor>("Interactor");
        _equipment = GetNode<PlayerEquipment>("Equipment");
        _stats = GetNode<ActorStats>("ActorStats");

        _camera = GetNode<Camera2D>("world_camera");

        ItemDatabase.LoadAllItems();
        CraftingDatabase.LoadRecipes();

        InitializeWorld();
    }

    public override void _PhysicsProcess(double delta)
    {
        PlayerInputFrame input = _input.Read(this);

        MovementMode movementMode =
            IsUnderground
                ? MovementMode.Underground
                : MovementMode.Surface;

        bool canRun = _stats.CurrentStamina > 0f;

        Movement.Update(
            input.MoveDirection,
            input.RunHeld,
            input.JumpPressed,
            canRun,
            movementMode,
            delta
        );

        if (Movement.IsRunning)
        {
            _stats.UseStamina(30f * (float)delta);
        }

        _combat.Update(input, delta);
        _interactor.Update(input);
        _animation.Update();

        MoveAndSlide();
    }

    public void SetUndergroundMode(bool isUnderground)
    {
        IsUnderground = isUnderground;

        if (IsUnderground)
        {
            CollisionMask = 2;

            GD.Print("Player switched to underground mode");
        }
        else
        {
            CollisionMask = 1;

            _camera.Zoom = new Vector2(2f, 2f);
            _camera.Offset = Vector2.Zero;

            GD.Print("Player switched to surface mode");
        }
    }

    private void InitializeWorld()
    {
        if (WorldManager.IsInitialized)
            return;

        Node world = GetParent();

        TileMapLayer ground =
            world.GetNode<TileMapLayer>("ground");

        TileMapLayer cracks =
            world.GetNode<TileMapLayer>("tile_crack");

        TileMapLayer flowers =
            world.GetNode<TileMapLayer>("flowers");

        Node2D placeables =
            world.GetNode<Node2D>("placeables");

        TileMapLayer mountains =
            world.GetNode<TileMapLayer>("mountains");

        TileMapLayer objects =
            world.GetNode<TileMapLayer>("objects");

        TileMapLayer baseUnderground =
            world.GetNode<TileMapLayer>("base_underground");

        TileMapLayer oreUnderground =
            world.GetNode<TileMapLayer>("ore_underground");

        WorldManager.Initialize(
            ground,
            cracks,
            flowers,
            mountains,
            objects,
            baseUnderground,
            oreUnderground,
            placeables
        );
    }

    public override void _ExitTree()
    {
        if (PlayerInstance == this)
        {
            PlayerInstance = null;
        }
    }
}