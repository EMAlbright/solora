using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public static class CraftingDatabase
{
	private static Dictionary<string, Recipe> _recipes = new();
	
	public static void LoadRecipes() {
		string json = FileAccess.GetFileAsString("res://scripts/DB/recipes.json");
		var loaded = JsonSerializer.Deserialize<List<Recipe>>(json);
		
		foreach(var recipe in loaded) {
			_recipes[recipe.Id] = recipe;
		}
		GD.Print($"Loaded {_recipes.Count} reipces");
	}
	
	public static Recipe GetRecipe(string id) {
		return _recipes.TryGetValue(id, out var recipe) ? recipe : null;
	}
	
	public static Recipe FindMatch(Dictionary<string, int> inputCounts) {
		GD.Print("hit finding recipe...");
		foreach(var recipe in _recipes.Values) {
			bool match = true;
			foreach(var ing in recipe.Inputs) {
				GD.Print(ing.ItemId);
				GD.Print(ing.Count);
				if (!inputCounts.ContainsKey(ing.ItemId) || inputCounts[ing.ItemId] < ing.Count){
					GD.Print("No match");
					match = false;
					break;
				}
			}
			if(match) {
				GD.Print("Match found");
				return recipe;
			}
		}
		return null;
	}
		
}
