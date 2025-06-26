using Godot;
using System.Collections.Generic;

[GlobalClass, Icon("res://editor/icons/game.png")]
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

        // Initialize logging system
        GameLogger.Initialize(gameConfig);

        map.SetupMap(gameConfig, noiseConfig, uiManager);

        AddChild(map);

        uiManager.StartGamePressed += () => StartGame();
    }

    public void StartGame()
    {
        GameLogger.Info("Game", "Starting game");
        GameLogger.Info("Game", $"With {gameConfig.Civilizations.Length} civilizations");
        uiManager.HideStartGameUi();
        uiManager.ShowGeneralUi();
        map.GenerateTerrain();
        map.GenerateResources();

        map.GenerateCivilizations(gameConfig.Civilizations);

        camera.SetBoundaries(map);
        Vector2I playerStartLocation = map.GetCiv(0).cities[0].centerCoordinates;
        camera.MoveTo(playerStartLocation);
    }


    // Handle the most top level input here
    // camera input is handled as a part of the camera class, not here
    public override void _UnhandledInput(InputEvent @event)
    {
        // If esc pressed clear everything
        if (@event is InputEventKey keyEvent
            && keyEvent.Keycode == Key.Escape
            && keyEvent.Pressed)
        {
            uiManager.HideAllPopups();
            map.ClearHexSelection(); // This now also clears unit selection
            return;
        }

        // Handle mouse button clicks
        if (@event is InputEventMouseButton click && click.Pressed)
        {
            switch (click.ButtonIndex)
            {
                case MouseButton.Left:
                    HandleLeftClick();
                    break;
                case MouseButton.Right:
                    HandleRightClick();
                    break;
            }
        }
    }

    private void HandleLeftClick()
    {
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

    private void HandleRightClick()
    {
        // Right click functionality goes here
        // For example: context menus, unit orders, etc.
        Vector2I selectedCoords = map.GetMapPosition();
        bool coordsInBounds = selectedCoords.X >= 0 && selectedCoords.X < map.width &&
                             selectedCoords.Y >= 0 && selectedCoords.Y < map.height;

        if (!coordsInBounds)
        {
            return;
        }

        map.MoveSelectedUnit(selectedCoords);
    }
}
