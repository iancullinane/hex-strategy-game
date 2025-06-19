using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnitType = UnitConfig.UnitType;

public partial class Unit : Node2D
{
    public string name = "DEFAULT";
    public UnitType unitType;

    // All units inherit a basic config
    [Export]
    public UnitConfig config { get; set; }

    // Game References
    public int cost;
    public int hp;
    public int movementPoints;
    public Area2D collider;

    // Map and selection symbols
    public Vector2I coords = new Vector2I();
    public Civilization civ;
    private TileMap map;
    public static Dictionary<Hex, List<Unit>> unitLocations = new Dictionary<Hex, List<Unit>>();
    public List<Hex> validMovementTiles;
    public HashSet<TerrainType> impassable = new HashSet<TerrainType>{
        TerrainType.WATER,
        TerrainType.SHALLOW_WATER,
        TerrainType.ICE,
        TerrainType.MOUNTAIN,
    };

    public bool isSelected = false;


    // Shared Graphics Resources, scene and texture
    public static PackedScene unitScene; // why static?
    public static Dictionary<UnitType, Texture2D> unitSceneResources;

    // Signals
    [Signal]
    public delegate void UnitClickedEventHandler(Unit unit);


    public override void _Ready()
    {
        collider = GetNode<Area2D>("Sprite2D/Area2D");
        CalculateValidAdjacentTiles();

        // This is the case that there is already a unit listed at this location,
        // which would mean that List exists already
        if (unitLocations.ContainsKey(map.GetHexAtCoords(this.coords)))
        {
            unitLocations[map.GetHexAtCoords(this.coords)].Add(this);
        }
        else
        {
            unitLocations[map.GetHexAtCoords(this.coords)] = new List<Unit> { this };
        }
    }

    //
    // Constructor
    // ------------------------------------------------------------

    // A note about constructors. A typical C# constructor would be in the form:
    //
    // public Unit(UnitConfig config, Vector2I coords){}
    //
    // However here we use a different approach, and make a new method
    // which is in effect a constructor. This has to do with Godot.
    // Ultimately we are not creating a new instance of scene as in traditional
    // OOP. We need to instantiate and then add to the scene tree. So we
    // make this constructor/not constructor method instead.
    public static Unit CreateUnit(UnitConfig config, TileMap map, Vector2I coords)
    {
        Unit unit = unitScene.Instantiate<Unit>();
        unit.config = config;
        unit.name = config.name;
        unit.unitType = config.unitType;
        unit.coords = coords;
        unit.hp = config.hp;
        unit.movementPoints = config.movementPoints;
        unit.cost = config.cost;
        unit.RefreshVisuals();

        // I sort of vaguely dislike this but it does make the
        // most sense. 
        unit.map = map;
        return unit;
    }

    #region visuals
    //
    // Visuals
    // ------------------------------------------------------------

    private void RefreshVisuals()
    {
        GetNode<Sprite2D>("Sprite2D").Texture = unitSceneResources[config.unitType];
    }

    public static void LoadUnitAssets()
    {
        unitScene = GD.Load<PackedScene>("res://scenes/unit.tscn");

        unitSceneResources = new Dictionary<UnitType, Texture2D>();
        unitSceneResources[UnitType.Warrior] = GD.Load<Texture2D>("res://assets/images/units/warrior.png");
        unitSceneResources[UnitType.Settler] = GD.Load<Texture2D>("res://assets/images/units/settler.png");
    }
    #endregion

    #region selection
    public void SetCiv(Civilization civ)
    {
        this.civ = civ;
        GetNode<Sprite2D>("Sprite2D").Modulate = civ.color;
        this.civ.units.Add(this);
    }

    //
    // Selection & UI
    // ---------------------------------------

    public UnitType GetUnitType()
    {
        return config.unitType;
    }

    public void SetSelected()
    {
        isSelected = true;
        CalculateValidAdjacentTiles();

        Color brighterColor = civ.color * 1.25f;
        GetNode<Sprite2D>("Sprite2D").Modulate = brighterColor;
    }

    public void SetDeselected()
    {
        isSelected = false;
        GetNode<Sprite2D>("Sprite2D").Modulate = civ.color;
    }
    #endregion


    #region interaction

    public void CalculateValidAdjacentTiles()
    {
        List<Hex> validTiles = map.GetSurroundingTiles(coords).
            Where(tile => !impassable.Contains(tile.terrainType)
        ).ToList();

        validMovementTiles = validTiles;
    }
    #endregion



    #region inputhandling

    // Input
    // ------------------------------------------------------------

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.ButtonIndex == MouseButton.Left && eventMouseButton.Pressed)
        {

            var spacesState = GetWorld2D().DirectSpaceState;
            var point = new PhysicsPointQueryParameters2D();
            point.CollideWithAreas = true;
            point.Position = GetGlobalMousePosition();
            var result = spacesState.IntersectPoint(point);
            if (result.Count > 0 && (Area2D)result[0]["collider"] == collider)
            {
                SetSelected();
                EmitSignal(SignalName.UnitClicked, this);
                GetViewport().SetInputAsHandled();
            }
            else
            {
                SetDeselected();
            }
        }
    }
    #endregion
}