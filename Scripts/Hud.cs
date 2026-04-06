using Godot;
using System;

public partial class Hud : CanvasLayer
{
	[Export]
	public NodePath foodLabelPath;

	private Label foodLabel;
	private RunState runState;

	public override void _Ready()
	{
		foodLabel = GetNodeOrNull<Label>(foodLabelPath);
		if (foodLabel == null)
		{
			GD.PushWarning("Hud: Food label not found. Assign foodLabelPath in the inspector.");
			return;
		}

		runState = GetNodeOrNull<RunState>("/root/RunState");
		if (runState == null)
		{
			GD.PushWarning("Hud: RunState autoload not found at /root/RunState.");
			return;
		}

		// Keep HUD synced whenever resource values change.
		runState.ResourcesChanged += OnResourcesChanged;

		// Initialize text immediately so the HUD is correct on scene load.
		UpdateFoodLabel(runState.Food);
	}

	public override void _ExitTree()
	{
		if (runState != null)
		{
			runState.ResourcesChanged -= OnResourcesChanged;
		}
	}

	private void OnResourcesChanged(int food, int wood, int metaCurrency)
	{
		UpdateFoodLabel(food);
	}

	private void UpdateFoodLabel(int food)
	{
		if (foodLabel != null)
		{
			foodLabel.Text = $"Food: {food}";
		}
	}
}
