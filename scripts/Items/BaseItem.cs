using Godot;

public enum ItemType
{
	Weapon,
	Tool,
	Mat,
	Consumable,
	Armor,
	// QuestItem,
}

public enum MaterialType
{
	None,
	Wood,
	Stone,
	Metal,
	Dirt,
	Sand
}

public class BaseItem {
	public string ItemCategory;
	public string ItemId;
	public string DisplayName;
	public string Description;
	public string ScenePath;
	public Texture2D Icon;
	public ItemType Type;
	public float WorldScale;
	public int MaxStackSize = 99;
	public bool IsStackable => MaxStackSize > 1; // no stacking weapons
	
	// item rarity, crafting mat, etc.
	public string[] Tags;
	
	public MaterialType? MaterialType;
	
	public virtual void Use(Node2D user, Node2D target = null) {
		GD.Print($"{DisplayName} used, override the method!");
	}
}
