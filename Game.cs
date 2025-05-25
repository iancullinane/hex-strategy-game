using Godot;
using System.Collections.Generic;


public partial class Game : Node
{
    private TileMap map;

    [Export]
    public GameConfig gameConfig;

    [Export]
    public NoiseConfig noiseConfig;


    // [Export]
    public UiManager uiManager;


    // private TileMap tileMap;
    private Camera camera;

    private List<Civilization> civilizations;


    public override void _Ready()
    {
        uiManager = GetNode<UiManager>("CanvasLayer/NewUiManager");
        camera = GetNode<Camera>("Camera");
        // TODO: Center the camera
        camera.Position = new Vector2I(50 / 2, 50 / 2);

        PackedScene tileMapScene = ResourceLoader.Load<PackedScene>("res://scenes/tile_map.tscn");
        map = tileMapScene.Instantiate<TileMap>();

        map.SetupMap(gameConfig, noiseConfig, uiManager);

        AddChild(map);
        GD.Print(uiManager);

        uiManager.StartGamePressed += () => StartGame();


        civilizations = new List<Civilization>();
        foreach (string civName in gameConfig.CivilizationNames)
        {
            Civilization civ = new Civilization(civName);
            civilizations.Add(civ);
        }


        if (gameConfig.DebugMode)
        {
            GD.Print("Debug mode enabled");
        }
    }

    public void StartGame()
    {
        GD.Print("Starting game");
        uiManager.HideStartGameUi();
        map.GenerateTerrain();
        map.GenerateResources();

        foreach (Civilization civ in civilizations)
        {
            map.CreateCity(civ, map.GetRandomHex().coordinates, civ.name);
        }

        camera.SetBoundaries(map);
        camera.CenterCamera();
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
    }
}
