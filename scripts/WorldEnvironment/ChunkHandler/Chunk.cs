using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Chunk
{
	public const int ChunkSize = 32;
	public Vector2I ChunkCoords;

	public Dictionary<Vector2I, List<string>> tileTypes = new Dictionary<Vector2I, List<string>>();

	// get counts of what tileNames have
	// this used for randomly placing objects (trees)

	public List<String> tileNames = ["grass", "tree"];

	public Chunk(Vector2I pos)
	{
		ChunkCoords = pos;
	}
	
	public void Generate(FastNoiseLite noiseLite)
	{
		string tileType = "tree";
		int objCount = 0;
		Vector2I localTilePos = new();
		while (objCount < 3){
			int objX = Random.Shared.Next(1, 32);
			int objY = Random.Shared.Next(1, 32);
			localTilePos = new Vector2I(objX, objY);
			if (!tileTypes.ContainsKey(localTilePos))
			{
				tileTypes[localTilePos] = new List<string>();
			}

			tileTypes[localTilePos].Add(tileType);
			objCount++;
		}
		
		for (int x = 0; x < ChunkSize; x++)
		{
			for (int y = 0; y < ChunkSize; y++)
			{
				localTilePos = new Vector2I(x, y);
				if (!tileTypes.ContainsKey(localTilePos))
				{
					tileTypes[localTilePos] = new List<string>();
				}
				tileType = "grass";
				tileTypes[localTilePos].Add(tileType);
			}
		}
	}
	
	public List<string> GetTileType(Vector2I tilePos)
	{
		return tileTypes.GetValueOrDefault(tilePos);
	}
}
