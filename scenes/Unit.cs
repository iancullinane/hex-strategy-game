using Godot;
using System;
using System.Collections.Generic;
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

    public Vector2I coords = new Vector2I();



    public Civilization civ;
    public bool isSelected = false;

    // Unit scene and assets 
    public static PackedScene unitScene; // why static?


    // Shared Graphics Resources
    public static Dictionary<UnitType, Texture2D> unitSceneResources;

    // Signals
    [Signal]
    public delegate void UnitClickedEventHandler(Unit unit);


    public override void _Ready()
    {
        collider = GetNode<Area2D>("Sprite2D/Area2D");
    }


    public Unit()
    {
        // Default constructor needed for Godot scene instantiation
    }

    public Unit(UnitConfig unitConfig)
    {
        config = unitConfig;
        name = config.name;
        // We set these from the config, but now they
        // are basically free of it and handled independently
        hp = config.hp;
        movementPoints = config.movementPoints;
        cost = config.cost;
    }

    public static Unit CreateUnit(UnitConfig config, Vector2I coords)
    {
        Unit unit = unitScene.Instantiate<Unit>();
        unit.config = config;
        unit.name = config.name;
        unit.unitType = config.unitType;
        unit.coords = coords;
        unit.RefreshVisuals();
        return unit;
    }

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


    public void SetCiv(Civilization civ)
    {
        this.civ = civ;
        GetNode<Sprite2D>("Sprite2D").Modulate = civ.color;
        this.civ.units.Add(this);
    }

    public UnitType GetUnitType()
    {
        return config.unitType;
    }

    public void SetSelected()
    {
        isSelected = true;
        Color brighterColor = civ.color * 1.25f;
        GetNode<Sprite2D>("Sprite2D").Modulate = brighterColor;
    }

    public void SetDeselected()
    {
        isSelected = false;
        GetNode<Sprite2D>("Sprite2D").Modulate = civ.color;
    }

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
}