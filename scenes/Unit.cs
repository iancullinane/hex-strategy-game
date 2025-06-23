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
    public Vector2I unitCoords = new Vector2I();
    public Civilization civ;
    private TileMap map;
    // The list of all units on the map essentially, as lists to account for unit stacking
    public static Dictionary<Hex, List<Unit>> unitLocations = new Dictionary<Hex, List<Unit>>();
    public List<Hex> validMovementTiles;
    public HashSet<TerrainType> impassable = new HashSet<TerrainType>{
        TerrainType.WATER,
        TerrainType.SHALLOW_WATER,
        TerrainType.ICE,
        TerrainType.MOUNTAIN,
    };

    // public bool isSelected = false;


    // Shared Graphics Resources, scene and texture
    public static PackedScene unitScene; // why static?
    public static Dictionary<UnitType, Texture2D> unitSceneResources;

    // Signals
    [Signal]
    public delegate void UnitClickedEventHandler(Unit unit);

    #region ready
    //
    // Ready
    // ------------------------------------------------------------


    public override void _Ready()
    {
        collider = GetNode<Area2D>("Sprite2D/Area2D");
        CalculateValidAdjacentTiles();

        // This is the case that there is already a unit listed at this location,
        // which would mean that List exists already
        if (unitLocations.ContainsKey(map.GetHexAtCoords(this.unitCoords)))
        {
            unitLocations[map.GetHexAtCoords(this.unitCoords)].Add(this);
        }
        else
        {
            unitLocations[map.GetHexAtCoords(this.unitCoords)] = new List<Unit> { this };
        }

        map.uiManager.EndTurn += ProcessTurn;
    }

    public override void _PhysicsProcess(double delta)
    {

    }


    public void ProcessTurn()
    {
        movementPoints = config.movementPoints;
    }



    #region movement

    public void Move(Hex hex)
    {
        GD.Print("Moving unit to position: " + hex.coordinates);
        GD.Print("Is unit selected: " + map.IsUnitSelected(this));
        GD.Print("Movement points: " + movementPoints);
        GD.Print("Valid movement tiles count: " + validMovementTiles.Count);
        GD.Print("Is hex in valid tiles: " + validMovementTiles.Contains(hex));

        if (map.IsUnitSelected(this) && movementPoints > 0)
        {
            if (validMovementTiles.Contains(hex))
            {
                MoveToHex(hex);
                // EmitSignal(SignalName.UnitClicked, this);
            }
            else
            {
                GD.Print("Hex not in valid movement tiles");
            }
        }
        else
        {
            GD.Print("Unit not selected or no movement points");
        }
    }

    // TODO: Handle moving multiple places in one turn
    // would like to do A Star with the tilemaplayer
    public void MoveToHex(Hex hexToOccupy)
    {
        GD.Print("Attempting to move to hex with units check...");

        // Check if the hex is unoccupied (either no entry or empty list)
        bool hexIsUnoccupied = !unitLocations.ContainsKey(hexToOccupy) ||
                              (unitLocations.ContainsKey(hexToOccupy) && unitLocations[hexToOccupy].Count == 0);

        GD.Print("Hex is unoccupied: " + hexIsUnoccupied);

        if (hexIsUnoccupied)
        {
            unitLocations[map.GetHexAtCoords(this.unitCoords)].Remove(this);

            Position = map.MapToLocal(hexToOccupy.coordinates);
            unitCoords = hexToOccupy.coordinates;

            if (!unitLocations.ContainsKey(hexToOccupy))
            {
                Unit.unitLocations[hexToOccupy] = new List<Unit> { this };
            }
            else
            {
                unitLocations[hexToOccupy].Add(this);
            }

            CalculateValidAdjacentTiles();
            movementPoints--;
            GD.Print("Unit moved successfully!");
        }
        else
        {
            GD.Print("Hex is occupied - combat would happen here");
        }
    }

    #endregion
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
        unit.unitCoords = coords;
        unit.hp = config.hp;
        unit.movementPoints = config.movementPoints;
        unit.cost = config.cost;
        unit.RefreshVisuals();

        // I sort of vaguely dislike this but it does make the
        // most sense. 
        unit.map = map;
        return unit;
    }
    #endregion
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
        // Visual changes for selection
        CalculateValidAdjacentTiles();
        Color brighterColor = civ.color * 1.25f;
        GetNode<Sprite2D>("Sprite2D").Modulate = brighterColor;
    }

    public void SetDeselected()
    {
        // Visual changes for deselection
        GetNode<Sprite2D>("Sprite2D").Modulate = civ.color;
    }
    #endregion


    #region interaction

    // CalculateValidAdjacentTiles will take every tile around a hex
    // and evaluate if it is a valid tile. This will likely become more
    // complex as we add more terrain types and other interactions
    public void CalculateValidAdjacentTiles()
    {
        List<Hex> validTiles = map.GetSurroundingTiles(unitCoords).
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
        if (@event is InputEventMouseButton eventMouseButton &&
            eventMouseButton.ButtonIndex == MouseButton.Left &&
            eventMouseButton.Pressed)
        {
            var spacesState = GetWorld2D().DirectSpaceState;
            var point = new PhysicsPointQueryParameters2D();
            point.CollideWithAreas = true;
            point.Position = GetGlobalMousePosition();
            var result = spacesState.IntersectPoint(point);

            if (result.Count > 0 && (Area2D)result[0]["collider"] == collider)
            {
                map.SetSelectedUnit(this);
                EmitSignal(SignalName.UnitClicked, this);
                GetViewport().SetInputAsHandled();
            }
            else
            {
                map.ClearSelectedUnit();
            }
        }
    }
    #endregion
}