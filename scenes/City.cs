using Godot;
using System;
using System.Collections.Generic;

public partial class City : Node2D
{

    // Base Info
    public string name;
    public Color color;
    public Label label;
    public Sprite2D sprite;


    // References
    public TileMap map;
    public Vector2I centerCoordinates;

    // Territory    
    public List<Hex> territory;
    public List<Hex> borderTilePool;
    public List<Hex> borderTiles;

    // Civ Info
    public Civilization civ;

    // Gameplay Data
    public int population = 1;
    public int food = 0;
    public int production = 0;



    public override void _Ready()
    {
        // Get the map
        label = GetNode<Label>("Label");
        sprite = GetNode<Sprite2D>("Sprite2D");
        territory = new List<Hex>();
        borderTilePool = new List<Hex>();

        label.Text = name;
        sprite.Modulate = color;
    }

    public void SetCityName(string newName)
    {
        name = newName;
    }

    public void SetIconColor(Color newColor)
    {
        color = newColor;
    }


    public void AddTerritory(List<Hex> territoryToAdd)
    {

        foreach (Hex hex in territoryToAdd)
        {
            hex.ownerCity = this;
        }
        territory.AddRange(territoryToAdd);
        CalculateTerritoryResourceTotals();
    }

    public void CalculateTerritoryResourceTotals()
    {
        int totalFood = 0;
        int totalProduction = 0;
        foreach (Hex hex in territory)
        {
            totalFood += hex.food;
            totalProduction += hex.production;
        }
        food = totalFood;
        production = totalProduction;
    }

}
