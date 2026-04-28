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
        Rect2 playArea = GetNode<RunState>("/root/RunState").gridBounds;
        int minX = (int)(playArea.Position.X - 1);
        int minY = (int)(playArea.Position.Y - 1);
        int maxX = (int)(playArea.Position.X + playArea.Size.X + 1);
        int maxY = (int)(playArea.Position.Y + playArea.Size.Y + 1);
        ground = GetNode<TileMapLayer>("Ground");
        for (int x = minX - 8 ; x <= maxX + 8; x++)
        {
            for (int y = minY - 4; y <= maxY + 4; y++)
            {
                ground.SetCell(new Vector2I(x, y), 0, new Vector2I(1, 1));
                if(!(x < maxX && x > minX && y < maxY && y > minY))
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
        // Construc world borders
        StaticBody2D borders = new StaticBody2D();
        for(int i = 0; i < 4; i++)
        {
            CollisionShape2D borderShape = new CollisionShape2D();
            WorldBoundaryShape2D worldBoundaryShape = new WorldBoundaryShape2D();
            switch (i)
            {
                case 0: // Top
                    worldBoundaryShape.Normal = new Vector2(0, 1);
                    worldBoundaryShape.Distance = (minY - 1) * tileSize;
                    break;
                case 1: // Bottom
                    worldBoundaryShape.Normal = new Vector2(0, -1);
                    worldBoundaryShape.Distance = -(maxY + 1) * tileSize;
                    break;
                case 2: // Left
                    worldBoundaryShape.Normal = new Vector2(1, 0);
                    worldBoundaryShape.Distance = (minX - 1) * tileSize;
                    break;
                case 3: // Right
                    worldBoundaryShape.Normal = new Vector2(-1, 0);
                    worldBoundaryShape.Distance = -(maxX + 1) * tileSize;
                    break;
            }
            borderShape.Shape = worldBoundaryShape;
            borders.AddChild(borderShape);
        }
        AddChild(borders);
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
