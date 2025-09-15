using Godot;
using System;

public partial class World : Node2D
{
public override void _Ready()
{		
	// Load DroppedItem scene
	var droppedItemScene = GD.Load<PackedScene>("res://scenes/dropped_item.tscn");
	
	var treeScene = GD.Load<PackedScene>("res://scenes/tree.tscn");
	// var treeInstance = treeScene.Instantiate() as Node2D;
	// treeInstance.Position = new Vector2(400, 350);
	// AddChild(treeInstance);
	
	var bearScene = GD.Load<PackedScene>("res://scenes/bear.tscn");
	

	var bearInstance = bearScene.Instantiate() as CharacterBody2D;
	bearInstance.Position = new Vector2(300, 420);
	AddChild(bearInstance);
	
	
	// Get item data
	var testPick = ItemDatabase.GetItem("wood_pickaxe");
	var testSword = ItemDatabase.GetItem("wood_sword");
	var testShovel = ItemDatabase.GetItem("wood_shovel");

	for (int i = 0; i < 2; i++)
	{
	var droppedItem = droppedItemScene.Instantiate() as RigidBody2D;
	droppedItem.Position = new Vector2(569 + i * 30, 380);

	if (droppedItem is DroppedItem di) {
		di.Init(testPick);
	}

	AddChild(droppedItem);
	}

	for (int i = 0; i < 2; i++)
	{
	var droppedItem = droppedItemScene.Instantiate() as RigidBody2D;
	droppedItem.Position = new Vector2(569 + i * 30, 420); 

	if (droppedItem is DroppedItem di) {
		di.Init(testSword);
	}

	AddChild(droppedItem);
	}
	
	for (int i = 0; i < 2; i++)
	{
	var droppedItem = droppedItemScene.Instantiate() as RigidBody2D;
	droppedItem.Position = new Vector2(569 + i * 30, 460); 

	if (droppedItem is DroppedItem di) {
		di.Init(testShovel);
	}

	AddChild(droppedItem);
	}
	
}

}
