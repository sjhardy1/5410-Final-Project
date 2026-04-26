using Godot;
using System;
using Godot.Collections;

public partial class GameManager : Node
{
    private Dictionary<string, PackedScene> scenes = new Dictionary<string, PackedScene>()
    {
        {"menu", GD.Load<PackedScene>("res://Scenes/UI/MainMenu.tscn")},
        {"game_root", GD.Load<PackedScene>("res://Scenes/GameRoot.tscn")},
        {"settings", GD.Load<PackedScene>("res://Scenes/UI/Settings.tscn")},
        {"upgrades", GD.Load<PackedScene>("res://Scenes/UI/Upgrades.tscn")}
    };

    public void ChangeScene(string sceneName)
    {
        if (scenes.ContainsKey(sceneName))
        {
            GetTree().ChangeSceneToPacked(scenes[sceneName]);
        }
        else
        {
            GD.PrintErr("Scene not found: " + sceneName);
        }
    }
}
