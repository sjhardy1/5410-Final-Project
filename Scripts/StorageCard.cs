using Godot;
using System;

public partial class StorageCard : Button
{
	public override void _Ready()
	{
	}
	public void Initialize(string name, GridPlaceable placeable)
	{
		GetNode<Label>("Name").Text = name;
		foreach (Node child in GetChildren())
		{
			if(child is GridPlaceable)
			{
				child.QueueFree();
			}
		}
		AddChild(placeable);
		placeable.Position = new Vector2(50, 50);
		placeable.Scale = new Vector2(0.25f, 0.25f);
	}
}
