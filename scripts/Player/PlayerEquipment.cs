using Godot;

public partial class PlayerEquipment: Node
{
    [Signal]
    public delegate void EquipmentChangedEventHandler();

    public Node2D EquippedNode { get; private set; }
    public InventoryEntry EquippedEntry { get; private set; }

    public bool HasEquippedItem => EquippedNode != null && EquippedEntry != null;

    public string EquippedItemId => EquippedEntry?.Item?.ItemId;

    private Node2D _pivot;
    private Node2D _owner;
    private EquipmentContext _context;

    public override void _Ready()
    {
        _owner = GetParent<Node2D>();

        _pivot = _owner.GetNode<Node2D>(
            "VisualRoot/WeaponPivot"
        );
    }

    public bool Equip(InventoryEntry entry) {
		if (entry?.Item == null) {
			return false;
		}
		
        // Attempt to equip same item we already have
		if (EquippedEntry != null && EquippedEntry.Key == entry.Key) {
			return false;
		}

        Node2D instance = CreateEquippedInstance(entry);

        if(instance == null) {
            return false;
        }

        Unequip();
        _pivot.AddChild(instance);

        EquippedNode = instance;
        EquippedEntry = entry;

        _context = new(
            Owner: _owner,
            Entry: entry
        );

        if(instance is IEquippable equippable)
        {
            equippable.OnEquip(
                _context
            );
        }

        // Emit Equipment Signal
        EmitSignal(
            SignalName.EquipmentChanged
        );

        return true;
	}

    public void Unequip()
    {
        if(EquippedNode == null) {
            EquippedEntry = null;
            return;
        }

        if(EquippedNode is IEquippable equippable) {
            equippable.OnUnequip();
        }

        // Remove unequipped node from scene
        EquippedNode.QueueFree();

        EquippedNode = null;
        EquippedEntry = null;

        EmitSignal(
            SignalName.EquipmentChanged
        );
    }

    // Get equipped node if it is of type T
    // i.e. 
    // GetEquipped<IWeapon>() -> Sword, Bow, etc. null otherwise
    // GetEquipped<IMaterial>() -> Dirt, Rock, etc. null otherwise
    public T GetEquipped<T>() where T: class
    {
        return EquippedNode as T;
    }

    private Node2D CreateEquippedInstance(InventoryEntry entry)
    {

		if (entry.Item.Type == ItemType.Mat) {

			PackedScene materialScene = GD.Load<PackedScene>("res://scenes/placeable_block.tscn");
			if(materialScene == null)
            {
                GD.PushError("Placeable Block Scene Could Not Be Loaded");
                return null;
            }
            return materialScene.Instantiate<Node2D>();
		}
		
		if (string.IsNullOrWhiteSpace(entry.Item.ScenePath)) {
            GD.PushError($"Item '{entry.Item.ItemId}' has no scene");
            return null;
		}

        PackedScene scene = GD.Load<PackedScene>(entry.Item.ScenePath);

		if(scene == null)
            {
                GD.PushError($"Scene not found: {entry.Item.ScenePath}");
                return null;
            }
		
		return scene.Instantiate<Node2D>();
    }

}