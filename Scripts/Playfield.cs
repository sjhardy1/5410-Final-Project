using Godot;
using System;

public partial class Playfield : Node2D
{
    [Export]
    public PackedScene treeScene;
    private int tileSize = 64;
    private TileMapLayer ground;
    public override void _Ready()
    {
        ground = GetNode<TileMapLayer>("Ground");
        for (int x = -20; x <= 20; x++)
        {
            for (int y = -15; y <= 15; y++)
            {
                ground.SetCell(new Vector2I(x, y), 0, new Vector2I(1, 1));
                if(!(x < 12 && x > -12 && y < 10 && y > -10))
                {
                    Vector2 pos = new Vector2(x * tileSize, y * tileSize);
                    AnimatedSprite2D tree = (AnimatedSprite2D)treeScene.Instantiate();
                    tree.Position = pos;
                    tree.Play(GD.Randf() > 0.5f ? "small1" : "small2");
                    tree.ZIndex = 1000;
                    AddChild(tree);
                }
            }
        }
        GetNode<SignalBus>("/root/SignalBus").Placing += ShowShader;
        GetNode<SignalBus>("/root/SignalBus").StopPlacing += HideShader;
    }
    public override void _ExitTree()
    {
        GetNode<SignalBus>("/root/SignalBus").Placing -= ShowShader;
        GetNode<SignalBus>("/root/SignalBus").StopPlacing -= HideShader;
    }
    private void SetShowShader(bool show)
    {
        if (ground.Material is ShaderMaterial mat)
        {
            mat.SetShaderParameter("show_overlay", show);
        }
    }
    private void HideShader() => SetShowShader(false);
    private void ShowShader() => SetShowShader(true);
    public override void _Process(double delta)
    {
        if(ground.Material is ShaderMaterial mat)
        {
            mat.SetShaderParameter("mouse_position", ground.GetLocalMousePosition());
        }
    }
}
