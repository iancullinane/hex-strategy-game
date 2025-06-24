using Godot;
using System;
using System.Collections.Generic;

public class BuildQueueItem
{
    public UnitConfig config;
    public int paid = 0;

    public BuildQueueItem(UnitConfig unitConfig)
    {
        config = unitConfig;
    }
}

public partial class City : Node2D
{

    // a `static` variable is shared by all instances of a class
    public int POPULATION_GROWTH_THRESHOLD = 15;
    public static double POPULATION_GROWTH_RATE = 0.15;

    // Base Info
    public string name;
    public Color color;
    public Label label;
    public Sprite2D sprite;


    // References
    public TileMap map;

    // Territory    
    public Vector2I centerCoordinates;
    public List<Hex> territory;
    public List<Hex> borderTilePool;

    // static, all cities have access to this variable
    public static Dictionary<Hex, City> staticInvalidTiles = new Dictionary<Hex, City>();

    // Civ Info
    public Civilization civ;

    // Gameplay Data
    public int size = 0;
    public int food = 0;
    public int production = 0;
    public int population = 1;
    public int populationGrowthThreshold;
    public int populationGrowthTracker;

    // Production
    // Units - Changed to use UnitConfig with progress tracking
    public List<BuildQueueItem> unitBuildQueue;

    // Selection state
    public bool isSelected = false;

    // Signals
    [Signal]
    public delegate void CityClickedEventHandler(City city);


    public override void _Ready()
    {
        // Get the map
        label = GetNode<Label>("Label");
        sprite = GetNode<Sprite2D>("Sprite2D");
        territory = new List<Hex>();
        borderTilePool = new List<Hex>();
        unitBuildQueue = new List<BuildQueueItem>();

        // Initialize population growth threshold
        populationGrowthThreshold = POPULATION_GROWTH_THRESHOLD;

        label.Text = name;
        sprite.Modulate = color;
    }

    public void ProcessTurn()
    {
        CleanUpBorderPool();

        populationGrowthTracker += food;
        if (populationGrowthTracker > populationGrowthThreshold)
        {
            population++;
            populationGrowthTracker = 0;
            populationGrowthThreshold += (int)(POPULATION_GROWTH_THRESHOLD * POPULATION_GROWTH_RATE);

            //grow territory
            AddRandomNewTile();
            map.UpdateCivTerritoryMap(civ);
        }

        ApplyProduction();
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


    // Production
    // ------------------------------------------------------------

    public void AddToUnitBuildQueue(UnitConfig unitConfig)
    {
        unitBuildQueue.Add(new BuildQueueItem(unitConfig));
    }

    public void SpawnUnit(BuildQueueItem buildItem)
    {
        Unit unitToSpawn = Unit.CreateUnit(buildItem.config, map, GetRandomTerritoryTile().coordinates);
        unitToSpawn.Position = map.MapToLocal(unitToSpawn.unitCoords);
        unitToSpawn.SetCiv(civ);

        // Connect unit signal to UI manager
        unitToSpawn.UnitClicked += map.uiManager.SetUnitUi;


        map.AddChild(unitToSpawn);
    }

    public void ApplyProduction()
    {
        if (unitBuildQueue.Count == 0) return;
        BuildQueueItem itemInProduction = unitBuildQueue[0];
        itemInProduction.paid += production;
        if (itemInProduction.paid >= itemInProduction.config.cost)
        {
            SpawnUnit(itemInProduction);
            unitBuildQueue.RemoveAt(0);
        }
    }

    // Territory
    // ------------------------------------------------------------

    public void AddTerritory(List<Hex> territoryToAdd)
    {

        foreach (Hex hex in territoryToAdd)
        {
            hex.ownerCity = this;
            AddValidNeighborsToBorderTilePool(hex);
            size++;
        }
        territory.AddRange(territoryToAdd);

        CalculateTerritoryResourceTotals();

        // Refresh highlighting if this city is selected
        if (isSelected)
        {
            map.HighlightCityTerritory(this, true);
        }
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

        // Check if tile is already owned by another city
        if (hex.ownerCity != null && hex.ownerCity != this)
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


    public Hex GetRandomTerritoryTile()
    {
        if (territory.Count == 0)
            return null;

        Random random = new Random();
        int index = random.Next(territory.Count);
        return territory[index];
    }

    // Selection methods
    // ------------------------------------------------------------

    public void SetSelected()
    {
        isSelected = true;
        // Brighten territory tiles
        map.HighlightCityTerritory(this, true);
    }

    public void SetDeselected()
    {
        isSelected = false;
        // Return territory tiles to normal
        map.HighlightCityTerritory(this, false);
    }

}
