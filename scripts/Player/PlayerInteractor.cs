using Godot;

public partial class PlayerInteractor: Node
{
    private Node2D _owner;
    private Area2D _interactableArea;

    public override void _Ready()
    {
        _owner = GetParent<Node2D>();
        _interactableArea = GetParent<Area2D>();
    }

    public void Update(PlayerInputFrame input)
    {
        if (!input.InteractPressed)
        {
            return;
        }

        TryInteract();
    }

    private void TryInteract()
    {

        // Create interaction context
        InteractionContext context = new (
            Owner: _owner,
            Origin: _owner.GlobalPosition 
        );

        // Find interactable object/scene
        IInteractable target = FindInteractable();

        if (target != null)
        {
            target.Interact(context);
        }

        // If interactable object not found, try to interact with tile/world
        TryWorldInteract(context);
    }

    private IInteractable FindInteractable()
    {
        // Node2D bodies (NPCs)
        foreach(Node2D body in _interactableArea.GetOverlappingBodies())
        {
            if (body is IInteractable interactable)
            {
                return interactable;
            }
            if (body.GetParent() is IInteractable pInteractable)
            {
                return pInteractable;
            }
        }

        // Area2D objects (chests, doors, etc.)
        foreach(Area2D area in _interactableArea.GetOverlappingAreas())
        {
            if (area is IInteractable interactable)
            {
                return interactable;
            }
            if (area.GetParent() is IInteractable pInteractable)
            {
                return pInteractable;
            }
        }
        return null;
    }

    private void TryWorldInteract(InteractionContext context)
    {
        Vector2I tile = WorldManager.WorldToTilePos(context.Origin);

        bool entrance = WorldManager.HasEntranceAt(tile);
        bool exit = WorldManager.HasExitAt(tile);

        if (WorldManager.IsUnderground)
        {
            if (WorldManager.HasExitAt(tile))
            {
                WorldManager.UndergroundToSurface(tile, context.Owner);
            }
            return;
        }
        
        if (WorldManager.HasEntranceAt(tile))
        {
            WorldManager.SurfaceToUnderground(tile, context.Owner);
        }
    }


}