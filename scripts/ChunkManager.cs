using Godot;
using System;
using System.Collections.Generic;

public partial class ChunkManager : Node
{
	[Export]
	// TILEMAPS
	private TileMapLayer _baseUnderground { get; set; }
	private TileMapLayer _oreUnderground { get; set; }
	private TileMapLayer _baseSurface { get; set; }
	private TileMapLayer _firstSurface { get; set; }
	private TileMapLayer _secondSurface { get; set; }
	private TileMapLayer _objectSurface { get; set; }
	
	private TileMapLayer _underground { get; set; }

	private TileSet _groundTileset;
	private TileSet _objectTileset;
	private TileSet _underGroundTileset;
	
	private Timer chunkTimer;

	// size of chunks
	private const int _chunkSize = 32;
	//size of tile (approx)
	private const int _tileSize = 16;
	// # chunks to generate around player
	private int _chunkBuffer = 2;
	private ChunkManager _instance;
	// TODO: source has TileMapLayer not CHUNK
	// need to replace logic of tilemaplayer with chunk objects
	private Dictionary<Vector2I, Chunk> ActiveChunks = new Dictionary<Vector2I, Chunk>();
	
	//noise
	private FastNoiseLite _noise = new FastNoiseLite();

	// list of noise values
	private List<float> _noiseValues = new List<float>();

	//player pos
	private Vector2I _playerChunkPos;


	public override void _Ready()
	{
		_instance = this;
		_groundTileset = GD.Load<TileSet>("res://scripts/DB/resources/GroundTiles.tres");
		_objectTileset = GD.Load<TileSet>("res://scripts/DB/resources/ObjectTiles.tres");
		_underGroundTileset = GD.Load<TileSet>("res://scripts/DB/resources/ObjectTiles.tres");

		
		_baseSurface =  GetParent().GetNode<TileMapLayer>("ChunkManager/_baseSurface");
		_objectSurface = GetParent().GetNode<TileMapLayer>("ChunkManager/_objectSurface");
		_underground = GetParent().GetNode<TileMapLayer>("ChunkManager/_underground");
		
		_baseSurface.TileSet = _groundTileset;
		_objectSurface.TileSet = _objectTileset;
		_underground.TileSet = _underGroundTileset;
		
		chunkTimer = GetParent().GetNode<Timer>("chunkTimer");
		chunkTimer.WaitTime = 10.0f;
		chunkTimer.OneShot = false;
		chunkTimer.Timeout += OnChunkTimerTimeout;
		
		_noise.Seed = new Random().Next(-1000000, 1000000);
		_noise.Frequency = 0.01f;
		chunkTimer.Start();
		
		_playerChunkPos = Vector2I.Zero;
		RefreshChunks(Vector2.Zero);

	}

	public override void _Process(double delta)
	{
		//RefreshChunks(Player.PlayerInstance.GlobalPosition);

	}
	
	public void OnChunkTimerTimeout(){
		RefreshChunks(Player.PlayerInstance.GlobalPosition);
	}
	
	
	public void RefreshChunks(Vector2 pos){
		Vector2I newPlayerChunkPos = WorldToChunkPosition(pos);
		_playerChunkPos = newPlayerChunkPos;
		// only load/unload if player in new chunk
		LoadChunksAroundPos(pos);
		UnloadChunksAroundPos(pos);
	}

	private void LoadChunksAroundPos(Vector2 pos)
	{
		for (int x = -_chunkBuffer; x <= _chunkBuffer; x++)
		{
			for (int y = -_chunkBuffer; y <= _chunkBuffer; y++)
			{
				Vector2I chunkPos = _playerChunkPos + new Vector2I(x, y);

				if (ActiveChunks.ContainsKey(chunkPos))
				{
					continue;
				}

				// create/generate new chunk
				Chunk chunk = new Chunk(chunkPos);
				chunk.Generate(_noise);

				// store chunk
				ActiveChunks[chunkPos] = chunk;
				RenderChunk(chunk);
				GD.Print($"Loaded chunk: {chunkPos}");
			}
		}
	}
	
	private void RenderChunk(Chunk chunk)
	{
		for(int x = 0; x < _chunkSize; x++)
		{
			for(int y = 0; y < _chunkSize; y++)
			{
				Vector2I playerChunkLocPos = new Vector2I(x, y);
				List<string> tileType = chunk.GetTileType(playerChunkLocPos);

				if (tileType == null) {
					GD.Print($"Null tile to generate at position {x}, {y} ");  
					continue;
				}
				List<TileSetData> tileDataList = new();
				TileSetData tileData = null;
				
				foreach(string type in tileType)
				{
					tileData = TileDatabase.Get(type);
					tileDataList.Add(tileData);
				}
				
				if (tileData == null)
				{
					GD.Print("Tile data not found in tile DB");
				}

				// world pos
				Vector2I worldPos = new Vector2I(
					chunk.ChunkCoords.X * _chunkSize + x,
					chunk.ChunkCoords.Y * _chunkSize + y
				);
				foreach(TileSetData data in tileDataList)
				{
					if(data.Layer == "ground")
					{
					_baseSurface.SetCell(
						worldPos,
						0,
						data.atlasCoords
					);
					}
					if(data.Layer == "object"){
					_objectSurface.SetCell(
						worldPos,
						0,
						data.atlasCoords
					);
				}   
				}
			}
		}
	}

	private void UnloadChunksAroundPos(Vector2 pos)
	{
		List<Vector2> chunksToRemove = new List<Vector2>();

		foreach (var chunk in ActiveChunks)
		{
			Vector2I chunkPos = chunk.Key;
			int x = Mathf.Abs(chunkPos.X - _playerChunkPos.X);
			int y = Mathf.Abs(chunkPos.Y - _playerChunkPos.Y);
			int maxD = Mathf.Max(x, y);

			if (maxD > _chunkBuffer)
			{
				chunksToRemove.Add(chunkPos);
			}
		}

		foreach (Vector2I chunkPos in chunksToRemove)
		{
			EraseChunk(ActiveChunks[chunkPos]);
			ActiveChunks.Remove(chunkPos);

			// TODO: Save chunk data
		}
	}
	
	private void EraseChunk(Chunk chunk)
	{
		for(int x = 0; x < _chunkSize; x++)
		{
			for(int y = 0; y < _chunkSize; y++)
			{
				Vector2I worldPos = new Vector2I(
					chunk.ChunkCoords.X * _chunkSize + x,
					chunk.ChunkCoords.Y * _chunkSize + y
				);
				//_baseUnderground.EraseCell(worldPos);
				//_oreUnderground.EraseCell(worldPos);
				_baseSurface.EraseCell(worldPos);
				//_firstSurface.EraseCell(worldPos);
				//_secondSurface.EraseCell(worldPos);
				_objectSurface.EraseCell(worldPos);
			}
		}
	}
	
	private Vector2I WorldToChunkPosition(Vector2 worldPos)
	{	
		return new Vector2I(
			Mathf.FloorToInt(worldPos.X / _tileSize / _chunkSize),
			Mathf.FloorToInt(worldPos.Y / _tileSize / _chunkSize)
		);
	}
}
