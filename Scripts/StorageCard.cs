using Godot;
using System;

public partial class StorageCard : Button
{
	public override void _Ready()
	{
		
	}
	public void Initialize(string name, GridPlaceable placeable)
	{
		foreach(Node child in GetChildren())
		{
			if(child is GridPlaceable)
			{
				child.QueueFree();
			}
		}
		GetNode<Label>("Name").Text = name;
		placeable.Position = new Vector2(50, 50);
		placeable.Scale = new Vector2(0.25f, 0.25f);
		placeable.storage = true;
		AddChild(placeable);
	}
	public void Exit()
	{
		foreach(Node child in GetChildren())
		{
			if(child is GridPlaceable placeable)
			{
				RemoveChild(child);
				placeable.Position = Vector2.Zero;
				placeable.Scale = Vector2.One;
				placeable.storage = false;
			}
		}
		QueueFree();
	}
}
