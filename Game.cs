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


        PackedScene tileMapScene = ResourceLoader.Load<PackedScene>("res://scenes/tile_map.tscn");
        map = tileMapScene.Instantiate<TileMap>();

        // civilizations = new List<Civilization>();
        // foreach (CivilizationConfig civConfig in gameConfig.Civilizations)
        // {
        //     Civilization civ = new Civilization(civConfig.Name, civConfig.Color);
        //     civilizations.Add(civ);
        // }




        map.SetupMap(gameConfig, noiseConfig, uiManager);

        AddChild(map);

        uiManager.StartGamePressed += () => StartGame();




        if (gameConfig.DebugMode)
        {
            GD.Print("Debug mode enabled");
        }
    }

    public void StartGame()
    {
        GD.Print("Starting game");
        GD.Print($"With {gameConfig.Civilizations.Length} civilizations");
        uiManager.HideStartGameUi();
        map.GenerateTerrain();
        map.GenerateResources();




        civilizations = new List<Civilization>();
        for (int i = 0; i < gameConfig.Civilizations.Length; i++)
        {
            GD.Print($"Creating civilization {i}: {gameConfig.Civilizations[i].Name}");
            Civilization civ = new Civilization(i, gameConfig.Civilizations[i]);
            civilizations.Add(civ);

        }

        List<Vector2I> startingLocations = map.GenerateCivStartingLocations(civilizations.Count);


        foreach (Civilization civ in civilizations)
        {
            map.CreateCity(civ, startingLocations[0], civ.name);
            startingLocations.RemoveAt(0);
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
