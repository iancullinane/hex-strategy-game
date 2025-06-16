using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnitType = UnitConfig.UnitType;

public partial class Unit : Node2D
{
    public string name = "DEFAULT";

    // Initial files defined by a custom resource    [Export]
    public UnitConfig config { get; set; }

    // In scene references
    public int cost;
    public int paid = 0;
    public int currentHp;
    public int currentMovementPoints;

    // Game References
    public Vector2I coords = new Vector2I();

    public Civilization civ;

    // Unit scene and assets 
    public static PackedScene unitScene; // why static?


    // Type as the key, love it
    public static Dictionary<UnitType, Texture2D> unitSceneResources;

    public Unit()
    {
        // Default constructor needed for Godot scene instantiation
    }

    public Unit(UnitConfig unitConfig)
    {
        config = unitConfig;
        name = config.name;
    }

    public static Unit CreateUnit(UnitConfig config)
    {
        Unit unit = unitScene.Instantiate<Unit>();
        unit.config = config;
        unit.name = config.name;
        return unit;
    }

    private void RefreshVisuals()
    {
        GetNode<Sprite2D>("Sprite2D").Texture = unitSceneResources[config.unitType];
        GetNode<Label>("Label").Text = name;

        if (civ != null)
        {
            GetNode<Label>("Label").Modulate = civ.color;
        }
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
}