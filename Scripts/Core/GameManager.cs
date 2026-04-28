using Godot;
using System;
using Godot.Collections;

public partial class GameManager : Node
{
    public bool LoadSavedRunOnGameRoot { get; private set; } = true;

    private Dictionary<string, PackedScene> scenes = new Dictionary<string, PackedScene>()
    {
        {"menu", GD.Load<PackedScene>("res://Scenes/UI/MainMenu.tscn")},
        {"game_root", GD.Load<PackedScene>("res://Scenes/GameRoot.tscn")},
        {"settings", GD.Load<PackedScene>("res://Scenes/UI/Settings.tscn")},
        {"upgrades", GD.Load<PackedScene>("res://Scenes/UI/Upgrades.tscn")},
        {"setup", GD.Load<PackedScene>("res://Scenes/UI/RunSetupScreen.tscn")},
    };
    public void SetupNewGame()
    {
        ChangeScene("setup");
    }
    public void StartNewGame()
    {
        LoadSavedRunOnGameRoot = false;
        GetNode<RunState>("/root/RunState").ResetRun();
        ChangeScene("game_root");
    }

    public bool LoadGame()
    {
        if (!GetNode<SaveManager>("/root/SaveManager").HasRunSave())
        {
            return false;
        }

        LoadSavedRunOnGameRoot = true;
        ChangeScene("game_root");
        return true;
    }

    public bool ConsumeLoadSavedRunOnGameRoot()
    {
        bool shouldLoad = LoadSavedRunOnGameRoot;
        LoadSavedRunOnGameRoot = true;
        return shouldLoad;
    }

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
