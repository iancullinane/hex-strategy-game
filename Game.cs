using Godot;
using System;

public partial class Game : Node
{

    public override void _Ready()
    {
        GD.Print("Game ready");
        Camera camera = GetNode<Camera>("Camera");
        TileMap map = GetNode<TileMap>("TileMap");

        // Calculate center position
        Vector2 centerPos = map.MapToLocal(new Vector2I(map.width / 2, map.height / 2));
        camera.Position = centerPos;
    }
}
