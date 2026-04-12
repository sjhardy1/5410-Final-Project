using Godot;
using System;

public partial class Hud : CanvasLayer
{
	[Export]
	public NodePath foodLabelPath;
	[Export]
	public NodePath woodLabelPath;
	[Export]
	public NodePath timerLabelPath;

	private Label foodLabel;
	private Label woodLabel;
	private Label timerLabel;
	private RunState runState;
	private Button phaseButton;

	public override void _Ready()
	{
		foodLabel = GetNodeOrNull<Label>(foodLabelPath);
		woodLabel = GetNodeOrNull<Label>(woodLabelPath);
		timerLabel = GetNodeOrNull<Label>(timerLabelPath);

		phaseButton = GetNodeOrNull<Button>("PhaseButton");
		runState = GetNodeOrNull<RunState>("/root/RunState");

		// Keep HUD synced whenever resource values change.
		runState.ResourcesChanged += OnResourcesChanged;
		phaseButton.Pressed += GetNode<SignalBus>("/root/SignalBus").PublishRaidBegin;

		// Initialize text immediately so the HUD is correct on scene load.
		UpdateLabels(runState.Food, runState.Wood);

		runState.TimerChanged += UpdateTimer;

	}

	public override void _ExitTree()
	{
		if (runState != null)
		{
			runState.ResourcesChanged -= OnResourcesChanged;
			runState.TimerChanged -= UpdateTimer;
		}
	}

	private void OnResourcesChanged(int food, int wood, int metaCurrency)
	{
		UpdateLabels(food, wood);
	}

	private void UpdateLabels(int food, int wood)
	{
		if (foodLabel != null)
		{
			foodLabel.Text = $"Food: {food}";
		}
		if (woodLabel != null)
		{
			woodLabel.Text = $"Wood: {wood}";
		}
	}
	private void UpdateTimer(float downtimeTimeRemaining, float raidTimeElapsed)
	{
		if (timerLabel != null)
		{
			if (runState.Phase == RunPhase.Downtime)
			{
				timerLabel.Text = $"Downtime: {Mathf.CeilToInt(downtimeTimeRemaining)}s";
			}
			else if (runState.Phase == RunPhase.Raid)
			{
				timerLabel.Text = $"Raid Time: {Mathf.CeilToInt(raidTimeElapsed)}s";
			}
		}
	}
}
