using Godot;
using System;
using Godot.Collections;
public partial class Upgrades : CanvasLayer
{
    [Export] private NodePath crystalsLabelPath;
    [Export] private Dictionary<string, NodePath> upgradeButtonPaths;
    [Export] private NodePath returnButtonPath;
    [Export] private PackedScene notifScene;
    private RunState runState;
    public override void _Ready()
    {
        GetNode<Button>(returnButtonPath).Pressed += () => GetNode<GameManager>("/root/GameManager").ChangeScene("menu");
        runState = GetNode<RunState>("/root/RunState");
        SaveManager saveManager = GetNode<SaveManager>("/root/SaveManager");

        UpdateUIElements();

        if (upgradeButtonPaths != null)
        {
            foreach (var entry in upgradeButtonPaths)
            {
                string upgradeId = entry.Key;
                NodePath path = entry.Value;
                Button btn = GetNode<Button>(path);
                btn.Pressed += () => {
                    if(!runState.TryPurchaseUpgrade(upgradeId))
                    {
                        ShowNotification("Not enough crystals to purchase this upgrade.");
                    }
                    saveManager.SaveMetaState(runState.ToMetaData());
                    UpdateUIElements();
                };
            }
        }
    }
    private void ShowNotification(string message)
    {
        Notification notification = notifScene.Instantiate() as Notification;
        notification.Initialize(message);
        AddChild(notification);
    }
    private void UpdateUIElements()
    {
        GetNode<Label>(crystalsLabelPath).Text = "Crystals Owned: " + runState.MetaCurrency;
        foreach (var entry in upgradeButtonPaths)
        {
            string upgradeId = entry.Key;
            NodePath path = entry.Value;
            Button btn = GetNode<Button>(path);
            int level = runState.GetUpgradeLevel(upgradeId);
            int cost = runState.GetUpgradeCost(upgradeId, level);
            int maxLevel = runState.GetUpgradeMaxLevel(upgradeId);
            btn.Text = $"{runState.GetUpgradeDisplayName(upgradeId)} ({level}/{maxLevel})\nCost: {cost} Crystals";
            btn.Disabled = level >= maxLevel;
        }
    }
}
