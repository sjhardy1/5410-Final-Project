using Godot;
using System;

public partial class Settings : CanvasLayer
{
    [Export] private NodePath backButtonPath;
    [Export] private NodePath masterVolumeSliderPath;
    [Export] private NodePath musicVolumeSliderPath;
    [Export] private NodePath sfxVolumeSliderPath;
    public override void _Ready()
    {
       GetNode<Button>(backButtonPath).Pressed += () => GetNode<GameManager>("/root/GameManager").ChangeScene("menu");
       SaveManager saveManager = GetNode<SaveManager>("/root/SaveManager");
       RunState runState = GetNode<RunState>("/root/RunState");
    
         // Initialize sliders with current volume settings
        GetNode<HSlider>(masterVolumeSliderPath).Value = runState.masterVolume;
        GetNode<HSlider>(musicVolumeSliderPath).Value = runState.musicVolume;
        GetNode<HSlider>(sfxVolumeSliderPath).Value = runState.sfxVolume;

        GetNode<HSlider>(masterVolumeSliderPath).ValueChanged += (value) => {
            runState.UpdateVolume((int)value, runState.musicVolume, runState.sfxVolume);
            saveManager.SaveMetaState(runState.ToMetaData());
        };
        GetNode<HSlider>(musicVolumeSliderPath).ValueChanged += (value) => {
            runState.UpdateVolume(runState.masterVolume, (int)value, runState.sfxVolume);
            saveManager.SaveMetaState(runState.ToMetaData());
        };
        GetNode<HSlider>(sfxVolumeSliderPath).ValueChanged += (value) => {
            runState.UpdateVolume(runState.masterVolume, runState.musicVolume, (int)value);
            saveManager.SaveMetaState(runState.ToMetaData());
        };
    }
}
