using Godot;
using System;
using System.Collections.Generic;

public partial class City : Node2D
{

    // a `static` variable is shared by all instances of a class
    public static int POPULATION_GROWTH_THRESHOLD = 15;

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

    // static, all cities have access to this variable
    public static Dictionary<Hex, City> staticInvalidTiles = new Dictionary<Hex, City>();

    // Civ Info
    public Civilization civ;

    // Gameplay Data
    public int food = 0;
    public int production = 0;
    public int population = 1;
    public int populationGrowthThreshold;
    public int populationGrowthTracker;

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

    public void ProcessTurn()
    {
        GD.Print($"Processing turn for {name}");
        CleanUpBorderPool();

        populationGrowthTracker += food;
        if (populationGrowthTracker > populationGrowthThreshold)
        {
            population++;
            populationGrowthTracker = 0;
            populationGrowthThreshold += POPULATION_GROWTH_THRESHOLD;

            //grow territory
            AddRandomNewTile();
            map.UpdateCivTerritoryMap(civ);
        }
    }


    // Settings
    // ------------------------------------------------------------

    public void SetCityName(string newName)
    {
        name = newName;
    }

    public void SetIconColor(Color newColor)
    {
        color = newColor;
    }


    // Territory
    // ------------------------------------------------------------

    public void AddTerritory(List<Hex> territoryToAdd)
    {

        foreach (Hex hex in territoryToAdd)
        {
            hex.ownerCity = this;
            AddValidNeighborsToBorderTilePool(hex);
        }
        territory.AddRange(territoryToAdd);
        CalculateTerritoryResourceTotals();
    }

    public void AddRandomNewTile()
    {
        if (borderTilePool.Count == 0) return;
        Random random = new Random();
        int index = random.Next(borderTilePool.Count);
        this.AddTerritory(new List<Hex> { borderTilePool[index] });
        borderTilePool.RemoveAt(index);
    }

    public void AddValidNeighborsToBorderTilePool(Hex hex)
    {
        List<Hex> validNeighbors = map.GetSurroundingTiles(hex.coordinates);
        foreach (Hex neighbor in validNeighbors)
        {
            if (IsValidNeighbor(neighbor)) borderTilePool.Add(neighbor);
            staticInvalidTiles[neighbor] = this;

        }
    }

    public bool IsValidNeighbor(Hex hex)
    {
        if (hex.terrainType == TerrainType.WATER ||
            hex.terrainType == TerrainType.MOUNTAIN ||
            hex.terrainType == TerrainType.DESERT ||
            hex.terrainType == TerrainType.ICE)
        {
            return false;
        }

        if (staticInvalidTiles.ContainsKey(hex) && staticInvalidTiles[hex] != this)
        {
            return false;
        }

        return true;
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

    public void CleanUpBorderPool()
    {
        List<Hex> toRemove = new List<Hex>();
        foreach (Hex borderTile in borderTilePool)
        {
            if (staticInvalidTiles.ContainsKey(borderTile) && staticInvalidTiles[borderTile] != this)
            {
                toRemove.Add(borderTile);
            }
        }

        foreach (Hex borderTile in toRemove)
        {
            borderTilePool.Remove(borderTile);
        }
    }

}
