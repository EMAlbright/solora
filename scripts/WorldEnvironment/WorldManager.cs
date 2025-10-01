using Godot;
using System;
using System.Collections.Generic;

public static class WorldManager
{
	private static MiningManager _surfaceMiningManager;
	private static MiningManager _undergroundMiningManager;
	
	private static TileMapLayer[] _surfaceLayers;
	private static TileMapLayer[] _undergroundLayers;
	
	// underground tilemaplayers
	private static TileMapLayer _baseUnderground;
	private static TileMapLayer _oreUnderground;
	
	// above ground tilemaplayers
	// different way to structure tile map layers?
	private static TileMapLayer _groundTilemap; 
	private static TileMapLayer _crackedTilemap;
	private static TileMapLayer _flowers;
	private static Node2D _placeables;
	private static TileMapLayer _mountains; 
	private static TileMapLayer _objects;
	
	// track entrances
	private static HashSet<Vector2I> _entrances = new();
	// track exits
	private static HashSet<Vector2I> _exits = new();
	public static bool _isUnderground = false;
	
	public static void Initialize(TileMapLayer ground, TileMapLayer cracks, TileMapLayer f, TileMapLayer m, TileMapLayer o, TileMapLayer bU, TileMapLayer oU, Node2D p)
	{
		_groundTilemap = ground;
		_crackedTilemap = cracks;
		_flowers = f;
		_placeables = p;
		_mountains = m;
		_objects = o;
		_baseUnderground = bU;
		_oreUnderground = oU;
		_surfaceLayers = new TileMapLayer[] { _groundTilemap, _flowers, _mountains, _objects };
		_undergroundLayers = new TileMapLayer[] { _baseUnderground, _oreUnderground };
		_surfaceMiningManager = new MiningManager(_groundTilemap, _crackedTilemap);
		_undergroundMiningManager = new MiningManager(_baseUnderground, _crackedTilemap, _oreUnderground);
		
		ShowSurface();
		
		GD.Print("World Manager initialized");
	}

	// access to mining, return instance of mining manager depending on underground or surface
	public static MiningManager Mining => _isUnderground ? _undergroundMiningManager : _surfaceMiningManager;

	public static void PlaceBlock(Vector2I tilePos, MaterialItem mat){
		var scene = GD.Load<PackedScene>("res://scenes/placeable_block.tscn");
		var block = scene.Instantiate<MaterialBase>();
		
		block.MatData = mat;
		var sprite = block.GetNode<Sprite2D>("Sprite2D");
		sprite.Texture = mat.Icon;
		int tileSize = 16; 
		block.Position = new Vector2(tilePos.X * tileSize, tilePos.Y * tileSize);
		
		_placeables.AddChild(block);
	}
	
	// access to tile map ops
	public static Vector2I WorldToTilePos(Vector2 worldPos)
	{
		var currLevel = _isUnderground ? _baseUnderground : _groundTilemap;
		return currLevel?.LocalToMap(worldPos) ?? Vector2I.Zero;
	}
	
	// no duplicate entrances
	public static bool HasEntranceAt(Vector2I pos) => _entrances.Contains(pos);
	public static bool HasExitAt(Vector2I pos) => _exits.Contains(pos);
	
	public static void AddEntrance(Vector2I entrancePosition) {
		_entrances.Add(entrancePosition);
		GD.Print($"Entrance created at {entrancePosition}");
	}
	
	public static void AddExit(Vector2I exitPosition) {
		_exits.Add(exitPosition);
		// Remove tile in underground layer to create the exit shaft
		_baseUnderground.SetCell(exitPosition, -1);
		GD.Print($"Underground exit created at {exitPosition}");
	}
	
		// Transition methods
	public static void SurfaceToUnderground(Vector2I entrancePos, Player player) {
		if (!_entrances.Contains(entrancePos)) 
		{
			GD.Print($"No entrance found at {entrancePos}");
			return;
		}
		
		_isUnderground = true;
		ShowUnderground();
		
		// Position player at the entrance position in underground
		Vector2I undergroundTilePos = new Vector2I(entrancePos.X, 0);
		Vector2 undergroundWorldPos = _baseUnderground.MapToLocal(undergroundTilePos);
		AddExit(undergroundTilePos);
		player.GlobalPosition = undergroundWorldPos;
		player.SetUndergroundMode(true);
		
		GD.Print($"Player transitioned to underground at {undergroundWorldPos}");
	}
	
	public static void UndergroundToSurface(Vector2I exitPos, Player player) {
		_isUnderground = false;
		ShowSurface();
		
		// Position player at corresponding surface position
		Vector2 surfaceWorldPos = _groundTilemap.MapToLocal(exitPos);
		player.GlobalPosition = surfaceWorldPos;
		player.SetUndergroundMode(false);
		
		GD.Print($"Player returned to surface at {surfaceWorldPos}");
	}
	
	public static void GroundToSurface() {
		
	}
	
	public static void SurfaceToGround() {
		
	}
	
	private static void ShowUnderground() {
		foreach (var layer in _surfaceLayers) {
			layer.Visible = false;
		}
		foreach (var layer in _undergroundLayers) {
			layer.Visible = true;
		}
	}
	private static void ShowSurface() {
		foreach (var layer in _surfaceLayers) {
			layer.Visible = true;
		}
		foreach (var layer in _undergroundLayers) {
			layer.Visible = false;
		}	
	}
	
	public static bool IsInitialized => _surfaceMiningManager != null && _undergroundMiningManager != null;
	public static bool IsUnderground => _isUnderground;

}
