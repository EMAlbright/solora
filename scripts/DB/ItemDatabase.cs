using Godot;
using System;
using System.Collections.Generic;

public static class ItemDatabase
{
	private static Dictionary<string, BaseItem> _items = new();

public static void LoadAllItems()
{
	GD.Print("Loading all weapons...");
	
	if (!FileAccess.FileExists("res://scripts/DB/weapons.json"))
	{
		GD.PrintErr("weapons.json not found!");
		return;
	}
	
	using var f = FileAccess.Open("res://scripts/DB/weapons.json", FileAccess.ModeFlags.Read);
	if (f == null)
	{
		GD.PrintErr("Failed to open weapons.json!");
		return;
	}
	
	string jsonStr = f.GetAsText();	
	var parsed = Json.ParseString(jsonStr);
	var dict = parsed.As<Godot.Collections.Dictionary>();
	
	GD.Print("Dict: ", dict);
	
	foreach (var pair in dict)
	{
		var entry = pair.Value.As<Godot.Collections.Dictionary>();
		var type = Enum.Parse<ItemType>(entry["Type"].ToString());
		
		BaseItem item = type switch {
			ItemType.Weapon => ParseWeapon(entry),
			ItemType.Tool => ParseTool(entry),
			ItemType.Mat => ParseMaterial(entry),
			_ => throw new Exception("Unsupported item type!")
		};
		
		_items[item.ItemId] = item;
	}
	
	GD.Print("Items loaded: ", _items.Count);
}

	public static BaseItem GetItem(string id)
	{
		if (_items.TryGetValue(id, out var item))
		{
			return item;
		}

		GD.PrintErr($"Weapon with ID '{id}' not found.");
		return null;
	}
	
	public static WeaponItem ParseWeapon(Godot.Collections.Dictionary entry) {
		return new WeaponItem {
			ItemId = entry["ItemId"].ToString(),
			DisplayName = entry["DisplayName"].ToString(),
			Description = entry["Description"].ToString(),
			Damage = (int)(float)entry["Damage"],
			AttackCooldown = (float)entry["AttackCooldown"],
			AttackRange = (float)entry["AttackRange"],
			AttackDuration = (float)entry["AttackDuration"],
			ScenePath = entry["ScenePath"].ToString(),
			Icon = LoadIcon(entry),
			WorldScale = entry.ContainsKey("WorldScale") ? (float)entry["WorldScale"] : 1.0f,
			MaxStackSize = (int)(float)entry["MaxStackSize"],
			Type = ItemType.Weapon
		};
	}
	
	public static ToolItem ParseTool(Godot.Collections.Dictionary entry) {
		return new ToolItem {
			ItemId = entry["ItemId"].ToString(),
			DisplayName = entry["DisplayName"].ToString(),
			Description = entry["Description"].ToString(),
			Damage = (int)(float)entry["Damage"],
			AttackCooldown = (float)entry["AttackCooldown"],
			AttackRange = (float)entry["AttackRange"],
			AttackDuration = (float)entry["AttackDuration"],
			ScenePath = entry["ScenePath"].ToString(),
			Icon = LoadIcon(entry),
			WorldScale = entry.ContainsKey("WorldScale") ? (float)entry["WorldScale"] : 1.0f,
			MaxStackSize = (int)(float)entry["MaxStackSize"],
			Type = ItemType.Tool
		};
	}
	
	public static MaterialItem ParseMaterial(Godot.Collections.Dictionary entry) {
		return new MaterialItem {
			ItemId = entry["ItemId"].ToString(),
			DisplayName = entry["DisplayName"].ToString(),
			Description = entry["Description"].ToString(),
			Icon = LoadIcon(entry),
			WorldScale = entry.ContainsKey("WorldScale") ? (float)entry["WorldScale"] : 1.0f,
			MaxStackSize = (int)(float)entry["MaxStackSize"],
			Type = ItemType.Mat
		};
	}
	
	public static Texture2D LoadIcon(Godot.Collections.Dictionary entry) {
		if (!entry.ContainsKey("Icon")) {
			return null;
		}
		var path = entry["Icon"].ToString();
		var tex = GD.Load<Texture2D>(path);
		if (tex == null) {
			GD.PrintErr("Icon load failed: " + path);
		}
		return tex;
	}
}
