using Godot;
using System.Collections.Generic;


public partial class Game : Node
{
    private TileMap map;

    [Export]
    public GameConfig gameConfig;

    [Export]
    public NoiseConfig noiseConfig;
    public UiManager uiManager;
    private Camera camera;

    [Export]
    public CivilizationConfig playerCivilizationConfig;


    public override void _EnterTree()
    {
        //TODO: Should this still be in EnterTree? It has assets but it isn't a scene
        Unit.LoadUnitAssets();
    }

    public override void _Ready()
    {
        uiManager = GetNode<UiManager>("CanvasLayer/NewUiManager");
        camera = GetNode<Camera>("Camera");


        PackedScene tileMapScene = ResourceLoader.Load<PackedScene>("res://scenes/tile_map.tscn");
        map = tileMapScene.Instantiate<TileMap>();

        map.SendCityUiInfo += uiManager.SetCityUi;


        if (playerCivilizationConfig != null)
        {
            var newCivConfigs = new CivilizationConfig[gameConfig.Civilizations.Length + 1];
            newCivConfigs[0] = playerCivilizationConfig;
            newCivConfigs[0].IsPlayer = true;
            gameConfig.Civilizations.CopyTo(newCivConfigs, 1);
            gameConfig.Civilizations = newCivConfigs;
        }

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
        uiManager.ShowGeneralUi();
        map.GenerateTerrain();
        map.GenerateResources();

        map.GenerateCivilizations(gameConfig.Civilizations);

        camera.SetBoundaries(map);
        Vector2I playerStartLocation = map.GetCiv(0).cities[0].centerCoordinates;
        camera.MoveTo(playerStartLocation);


    }


    public override void _UnhandledInput(InputEvent @event)
    {

        if (@event is InputEventKey keyEvent
            && keyEvent.Keycode == Key.Escape
            && keyEvent.Pressed)
        {
            uiManager.HideAllPopups();
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

        if (!coordsInBounds)
        {
            map.ClearHexSelection();
            return;
        }


        map.SetHexSelection(selectedCoords);
        Hex selectedHex = map.GetHexAtMapPosition(selectedCoords);
    }
}
