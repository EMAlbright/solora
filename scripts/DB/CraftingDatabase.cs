using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class CraftingDatabase : Node
{
	public static CraftingDatabase Instance { get; private set; }
	private Dictionary<string, Recipe> _recipes = new();
	
	public override void _Ready() {
		if (Instance == null) {
			Instance = this;
			LoadRecipes();
		}
	}
	
	public void LoadRecipes() {
		string json = FileAccess.GetFileAsString("res://scripts/DB/recipes.json");
		var loaded = JsonSerializer.Deserialize<List<Recipe>>(json);
		
		foreach(var recipe in loaded) {
			_recipes[recipe.Id] = recipe;
		}
		GD.Print($"Loaded {_recipes.Count} reipces");
		}
	
	public Recipe GetRecipe(string id) {
		return _recipes.TryGetValue(id, out var recipe) ? recipe : null;
	}
	
	public IEnumerable<Recipe> GetAllRecipes() {
		return _recipes.Values;
	}
	
	public Recipe FindMatch(Dictionary<string, int> inputCounts) {
		foreach(var recipe in _recipes.Values) {
			bool match = true;
			foreach(var ing in recipe.Inputs) {
				if (!inputCounts.ContainsKey(ing.ItemId) || inputCounts[ing.ItemId] < ing.Count){
					match = false;
					break;
				}
			}
			if(match) {
				return recipe;
			}
		}
		return null;
	}
		
}
