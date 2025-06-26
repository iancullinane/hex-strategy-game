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

    // Unit Stats
    public int cost;
    public int hp;
    public int actionPoints;
    public int strength;

    // Map and selection symbols
    public Vector2I unitCoords = new Vector2I();
    public Area2D collider;
    public Civilization civ;
    public TileMap map;
    // The list of all units on the map essentially, as lists to account for unit stacking
    public static Dictionary<Hex, List<Unit>> unitLocations = new Dictionary<Hex, List<Unit>>();
    public List<Hex> validMovementTiles;
    public HashSet<TerrainType> impassable = new HashSet<TerrainType>{
        TerrainType.WATER,
        TerrainType.SHALLOW_WATER,
        TerrainType.ICE,
        TerrainType.MOUNTAIN,
    };

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

    public void ProcessTurn()
    {
        actionPoints = config.actionPoints; // Reset action points each turn
    }



    #region movement

    public void Move(Hex hex)
    {
        GameLogger.Verbose("Unit Movement", $"Moving unit to position: {hex.coordinates}");
        GameLogger.Verbose("Unit Movement", $"Is unit selected: {map.IsUnitSelected(this)}");
        GameLogger.Verbose("Unit Movement", $"Action points: {actionPoints}");
        GameLogger.Verbose("Unit Movement", $"Valid movement tiles count: {validMovementTiles.Count}");
        GameLogger.Verbose("Unit Movement", $"Is hex in valid tiles: {validMovementTiles.Contains(hex)}");

        if (map.IsUnitSelected(this) && actionPoints > 0)
        {
            if (validMovementTiles.Contains(hex))
            {
                MoveToHex(hex);
                // EmitSignal(SignalName.UnitClicked, this);
            }
            else
            {
                GameLogger.Debug("Unit Movement", "Hex not in valid movement tiles");
            }
        }
        else
        {
            GameLogger.Debug("Unit Movement", "Unit not selected or no action points");
        }
    }

    // TODO: Handle moving multiple places in one turn
    // would like to do A Star with the tilemaplayer
    public void MoveToHex(Hex hexToOccupy)
    {
        GameLogger.Verbose("Unit Movement", "Attempting to move to hex with units check");

        // Check if the hex contains a city of another civilization
        if (hexToOccupy.isCityCenter && map.cities.ContainsKey(hexToOccupy.coordinates))
        {
            City targetCity = map.cities[hexToOccupy.coordinates];
            if (targetCity.civ != this.civ)
            {
                GameLogger.Debug("Unit Movement", $"Move onto city '{targetCity.name}' belonging to {targetCity.civ.name}");
                targetCity.ChangeOwner(this.civ);
            }
        }

        // Check if the hex is unoccupied (either no entry or empty list)
        bool hexIsUnoccupied = !unitLocations.ContainsKey(hexToOccupy) ||
                              (unitLocations.ContainsKey(hexToOccupy) && unitLocations[hexToOccupy].Count == 0);

        GameLogger.Verbose("Unit Movement", $"Hex is unoccupied: {hexIsUnoccupied}");

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
            actionPoints--;


            if (hexToOccupy.isCityCenter &&
                hexToOccupy.ownerCity.civ != this.civ &&
                this.unitType == UnitType.Warrior)
            {
                City targetCity = map.cities[hexToOccupy.coordinates];
                targetCity.ChangeOwner(this.civ);
            }

            GameLogger.Debug("Unit Movement", "Unit moved successfully!");
        }
        else
        {
            var occupyingUnit = unitLocations[hexToOccupy][0];
            if (occupyingUnit.civ != this.civ)
            {
                CalculateCombat(this, occupyingUnit);
            }
            else
            {
                GameLogger.Debug("Unit Movement", "Cannot attack unit from same civilization");
            }
        }
    }

    public void CalculateCombat(Unit beligerant, Unit defender)
    {
        GameLogger.Debug("Unit Combat", $"Defender: {defender.name} with {defender.hp} HP");
        GameLogger.Debug("Unit Combat", $"Beligerant: {beligerant.name} with {beligerant.hp} HP");

        defender.hp -= beligerant.strength;
        beligerant.hp -= defender.strength / 2;

        if (defender.hp <= 0)
        {
            defender.RemoveUnit();
        }

        if (beligerant.hp <= 0)
        {
            beligerant.RemoveUnit();
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
        unit.actionPoints = config.actionPoints;
        unit.cost = config.cost;
        unit.strength = config.strength;
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

    #region actions

    public void RemoveUnit()
    {
        // Clear selection if this unit is currently selected
        if (map.IsUnitSelected(this))
        {
            map.ClearSelectedUnit();
        }

        // Remove from unit locations tracking
        var currentHex = map.GetHexAtCoords(unitCoords);
        if (Unit.unitLocations.ContainsKey(currentHex))
        {
            Unit.unitLocations[currentHex].Remove(this);
            if (Unit.unitLocations[currentHex].Count == 0)
            {
                Unit.unitLocations.Remove(currentHex);
            }
        }

        // Remove from civilization
        civ.units.Remove(this);

        // Remove from scene tree
        QueueFree();
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

    #region AI Actions

    public void AI_RandomMove()
    {
        // Use the Move action instead of built-in movement
        var moveAction = GetActionByName("Move");
        if (moveAction != null && validMovementTiles.Count > 0)
        {
            Random r = new Random();
            int rand = r.Next(validMovementTiles.Count);
            Hex targetHex = validMovementTiles[rand];

            if (moveAction.CanExecute(this, targetHex))
            {
                moveAction.Execute(this, targetHex);
            }
        }
    }

    public void AI_Settle()
    {
        // Use the Build City action
        var buildCityAction = GetActionByName("Build City");
        if (buildCityAction != null && buildCityAction.CanExecute(this))
        {
            buildCityAction.Execute(this);
        }
    }

    private UnitAction GetActionByName(string actionName)
    {
        if (config.actions != null)
        {
            foreach (var action in config.actions)
            {
                if (action.Name == actionName)
                {
                    return action;
                }
            }
        }
        return null;
    }

    #endregion
}