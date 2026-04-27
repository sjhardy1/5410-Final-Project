using Godot;
using System;

public partial class Hud : CanvasLayer
{
	[Export]public NodePath foodLabelPath;
	[Export] public NodePath woodLabelPath;
	[Export] public NodePath timerLabelPath;
	[Export] public PackedScene storageCardScene;
	[Export] public NodePath storagePath;
	[Export] public NodePath phaseButtonPath;
	private Label foodLabel;
	private Label woodLabel;
	private Label timerLabel;
	private RunState runState;
	private Button phaseButton;
	private VBoxContainer storage;

	public override void _Ready()
	{
		foodLabel = GetNodeOrNull<Label>(foodLabelPath);
		woodLabel = GetNodeOrNull<Label>(woodLabelPath);
		timerLabel = GetNodeOrNull<Label>(timerLabelPath);
		storage = GetNodeOrNull<VBoxContainer>(storagePath);
		Button storageButton = storage.GetNodeOrNull<Button>("StorageButton");

		phaseButton = GetNodeOrNull<Button>(phaseButtonPath);
		runState = GetNodeOrNull<RunState>("/root/RunState");

		SignalBus signalBus = GetNode<SignalBus>("/root/SignalBus");

		signalBus.Placing += storageButton.Show;
		signalBus.StopPlacing += storageButton.Hide;

		signalBus.RaidBegin += Hide;
		signalBus.RaidEnded += (int healCost, int repairCost) => Show();

		// Keep HUD synced whenever resource values change.
		runState.ResourcesChanged += OnResourcesChanged;
		phaseButton.Pressed += signalBus.PublishRaidBegin;
		storageButton.Pressed += signalBus.PublishCancelPlacement;

		// Initialize text immediately so the HUD is correct on scene load.
		UpdateLabels(runState.Food, runState.Wood);

		runState.TimerChanged += UpdateTimer;
		ClearStorage();
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
	private void ClearStorage()
	{
		foreach(Node child in storage.GetChildren())
		{
			if(child is StorageCard card)
			{
				card.Exit();
			}
		}
	}
	public void UpdateStorage()
	{
		ClearStorage();
		foreach(GridPlaceable placeable in runState.StoredPlaceables)
		{
			CreateCard(placeable);
		}
	}
	public void CreateCard(GridPlaceable placeable)
	{
		StorageCard card = storageCardScene.Instantiate<StorageCard>();
		card.Initialize(placeable.def.CoreAttributes.DisplayName, placeable);
		card.Pressed += () =>
		{
			card.Exit();
			GetNode<SignalBus>("/root/SignalBus").PublishPlaceableRemovedFromStorage(placeable);
		};
		storage.AddChild(card);
	}
}
