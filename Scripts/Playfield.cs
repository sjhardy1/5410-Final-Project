using Godot;
using System;

public partial class Playfield : Node2D
{
    [Export]
    public PackedScene treeScene;
    private int tileSize = 64;
    public override void _Ready()
    {
        TileMapLayer ground = GetNode<TileMapLayer>("Ground");
        for (int x = -10; x <= 10; x++)
        {
            for (int y = -10; y <= 10; y++)
            {
                ground.SetCell(new Vector2I(x, y), 0, new Vector2I(1, 1));
                if(!(x < 7 && x > -7 && y < 7 && y > -7))
                {
                    Vector2 pos = new Vector2(x * tileSize, y * tileSize);
                    AnimatedSprite2D tree = (AnimatedSprite2D)treeScene.Instantiate();
                    tree.Position = pos;
                    tree.Play(GD.Randf() > 0.5f ? "small1" : "small2");
                    AddChild(tree);
                }
            }
        }
    }
}
