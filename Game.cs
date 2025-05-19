using Godot;


public partial class Game : Node
{
    private TileMap map;


    public override void _Ready()
    {
        GD.Print("Game ready");
        Camera camera = GetNode<Camera>("Camera");
        map = GetNode<TileMap>("TileMap");

        // Calculate center position
        Vector2 centerPos = map.MapToLocal(new Vector2I(map.width / 2, map.height / 2));
        camera.Position = centerPos;
    }


    public override void _UnhandledInput(InputEvent @event)
    {

        if (@event is InputEventKey keyEvent
            && keyEvent.Keycode == Key.Escape
            && keyEvent.Pressed)
        {
            map.ClearHexSelection();
            return;
        }

        if (@event is not InputEventMouseButton click ||
            click.ButtonIndex != MouseButton.Left ||
            !click.Pressed)
        {
            return;
        }


        Vector2I selectedCoords = map.GetMapPosition();
        bool coordsInBounds = selectedCoords.X >= 0 && selectedCoords.X < map.width &&
                             selectedCoords.Y >= 0 && selectedCoords.Y < map.height;

        if (!coordsInBounds) return;

        map.SetHexSelection(selectedCoords);
        Hex selectedHex = map.GetHexAtMapPosition(selectedCoords);
        GD.Print($"Selected hex: {selectedHex}");
    }
}
