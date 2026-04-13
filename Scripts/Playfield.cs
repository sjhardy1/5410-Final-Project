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
                    AddChild(tree);
                }
            }
        }
        GetNode<SignalBus>("/root/SignalBus").Placing += () => SetShowShader(true);
        GetNode<SignalBus>("/root/SignalBus").StopPlacing += () => SetShowShader(false);
    }
    private void SetShowShader(bool show)
    {
        if (ground.Material is ShaderMaterial mat)
        {
            mat.SetShaderParameter("show_overlay", show);
        }
    }
    public override void _Process(double delta)
    {
        if(ground.Material is ShaderMaterial mat)
        {
            mat.SetShaderParameter("mouse_position", ground.GetLocalMousePosition());
        }
    }
}
