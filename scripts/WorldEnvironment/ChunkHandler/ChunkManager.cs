using Godot;
using System;

public partial class ChunkManager : Node
{
	[Export]
	// tileset to use to render
	private Tileset _tileSet {get; set;}
	// size of chunks
	private Vector2 _chunkSize;
	// # chunks to generate around player
	private int _chunkBuffer = 2;
	protected static ChunkManager _instance;
	// TODO: source has TileMapLayer not CHUNK
	// need to replace logic of tilemaplayer with chunk objects
	protected static Dictionary<Vector2I, Chunk> ActiveChunks = new Dictionary<Vector2I, Chunk>();
	
	//noise
	private FastNoiseLite _noise = new FastNoiseLite();
	
	// list of noise values
	private List<float> _noiseValues = new List<float>();
	
	public ChunkManager {
		_instance = this;
	}
	
	public override void _Ready(){
		_noise.Seed = new Random().Next(-1000000, 1000000);
		_noise.Frequency = 0.01f;
		_chunkSize = new Vector2(32, 32);
		LoadChunksAroundPosition(Vector2.Zero);
		
	}
	
	public void RefreshChunks(Vector2 pos){
		LoadChunksAroundPos(pos);
		UnloadChunksAroundPos(pos);
	}
	
	private void LoadChunksAroundPos(Vector2 pos){
		for(int x = -_chunkBuffer; x <= _chunkBuffer; x++){
			for(int y = -_chunkBuffer; y <= _chunkBuffer; y++){
				Vector2 chunkPos = GetChunkPosition(pos) + new Vector2(x, y);
			}
		}
	}
	
	private void UnloadChunksAroundPos(Vector2 pos){
		List<Vector2> chunksToRemove = new List<Vector2>();
		
		foreach(KeyValuePair<Vector2, TileMapLayer> chunk in _loadedChunks){
			if(!ChunkAroundPosition(pos, chunk.Key)){
				//TODO: Somewhere in here save chunk data
				chunksToRemove.Add(chunk);
				this.RemoveChild(chunk.Value);
				chunk.Value.QueueFree();
			}
		}
		
		foreach(Vector2 pos in chunksToRemove){
			ActiveChunks.remove(pos);
		}
	}
	
	public Vector2 GetChunkPosition(Vector2 pos){
		return new Vector2(Math.FloorToInt(pos.X / (_chunkSize.X * _tileSet.TileSize.X)), Math.FloorToInt(pos.Y / (_chunkSize.Y * _tileSet.TileSize.Y)));
	}
	
	private bool ChunkAroundPosition(Vector2 pos, Vector2 chunkPos){
		for(int x = -_chunkBuffer; x <= _chunkBuffer; x++){
			for(int y = -_chunkBuffer; y <= _chunkBuffer; y++){
				Vector2 chunk = chunkPos + new Vector2(x, y);
				Vector2 worldPos = chunk * _chunkSize * _tileSet.TileSize;
				Rect2 bounds = new Rect2(worldPos, _chunkSize * _tileSet.TileSize);
				
				if(bounds.HasPoint(pos)){
					return true;
				}
			}
		}
		return false;
	}
}
