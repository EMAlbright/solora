using Godot;
using System;
using System.Collections.Generic;

public partial class NavigationManager : Node
{
	public static NavigationManager NavInstance {get; private set;}
	
	private AStarGrid2D _astar;
	private HashSet<Vector2I> _grid = new();
	
	public int TileSize = 16;
	public Vector2I MapSize = new(150, 100);
	
	public override void _Ready(){
		if (NavInstance != null) {
			QueueFree();
			return;
		}
		NavInstance = this;
		
		_astar = new AStarGrid2D();
		_astar.Region = new Rect2I(Vector2I.Zero, MapSize);
		_astar.CellSize = new Vector2(TileSize, TileSize);
		_astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		_astar.Update();
		
	}
	public void RegisterObstacle(Vector2I cell){
		if (!_grid.Contains(cell)){
			_grid.Add(cell);
			_astar.SetPointSolid(cell, true);
		}
	}
	
	public void UnRegisterObstacle(Vector2I cell){
		if(_grid.Contains(cell)){
			_grid.Remove(cell);
			_astar.SetPointSolid(cell, false);
		}
	}
	
	public List<Vector2I> GetAstarPath(Vector2I start, Vector2I end){
		// start or end is block
		if(_astar.IsPointSolid(start) || _astar.IsPointSolid(end)){
			return new List<Vector2I>();
		}
		var p = _astar.GetIdPath(start, end);
		return new List<Vector2I>(p);
	}
	
	public Vector2I WorldToGrid(Vector2 worldPos){
		return new Vector2I(Mathf.FloorToInt(worldPos.X / TileSize), Mathf.FloorToInt(worldPos.Y / TileSize));
	}
	
	public Vector2 GridToWorld(Vector2I gridPos){
		return gridPos * TileSize + new Vector2(TileSize / 2f, TileSize / 2f);
	}
	
}
