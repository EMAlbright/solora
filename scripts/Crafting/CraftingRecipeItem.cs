using Godot;
using System.Collections.Generic;

public class RecipeInput
{
	public string ItemId { get; set; }
	public int Count { get; set; }
}

public class RecipeOutput
{
	public string ItemId { get; set; }
	public int Count { get; set; }
}

public class Recipe
{
	public string Id { get; set; }
	public List<RecipeInput> Inputs { get; set; }
	public RecipeOutput Output { get; set; }

	// shaped = positions matter, shaped == false = just ingredient counts
	public bool Shaped { get; set; } = false;

	// optional: crafting grid pattern (if shaped)
	public List<List<string>> Pattern { get; set; }
}
