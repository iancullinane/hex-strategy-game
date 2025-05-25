using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum TerrainType
{
    PLAINS,
    WATER,
    DESERT,
    MOUNTAIN,
    ICE,
    SHALLOW_WATER,
    BEACH,
    FOREST,
}

/// <summary>
/// Represents the possible terrain types in the game world.
/// PLAINS: Basic grassland terrain
/// WATER: Deep water bodies
/// DESERT: Arid sandy regions
/// MOUNTAIN: Elevated rocky terrain
/// ICE: Frozen terrain
/// SHALLOW_WATER: Coastal waters
/// BEACH: Sandy coastal areas
/// FOREST: Wooded regions
/// </summary>
public class Hex
{



    public readonly Vector2I coordinates;
    public TerrainType terrainType;

    public int food { get; set; }
    public int production { get; set; }

    public Hex(Vector2I coordinates)
    {
        this.coordinates = coordinates;
    }

    public override string ToString()
    {
        return $"[{coordinates}]->{terrainType}, food: {food}, production:{production}";
    }

    public void SetResources()
    {
        Random r = new Random();

        switch (terrainType)
        {
            case TerrainType.PLAINS:
                food = r.Next(10);
                production = r.Next(10);
                break;
            case TerrainType.MOUNTAIN:
                food = r.Next(2);
                production = r.Next(20);
                break;
            case TerrainType.WATER:
                food = 4;
                production = 0;
                break;
            case TerrainType.DESERT:
                food = r.Next(1);
                production = r.Next(2);
                break;
            case TerrainType.ICE:
                break;
            case TerrainType.FOREST:
                food = r.Next(5);
                production = r.Next(15);
                break;
            default:
                food = 0;
                production = 0;
                break;

        }

    }

}

/// <summary>
/// A hexagonal TileMap representation. Contains a base layer, border layer, and overlay layer.
/// </summary>

public partial class TileMap : Node2D
{

    // Loads
    // ------------------------------------------------------------
    PackedScene cityScene;


    // Ui
    // ------------------------------------------------------------
    UiManager uiManager;

    public delegate void HexSelectedEventHandler(Hex h);
    public event HexSelectedEventHandler SendHexData;
    Vector2I currentSelectedHex = new Vector2I(-1, -1);

    // Map settings
    // ------------------------------------------------------------


    public int height = 100;
    public int width = 60;
    // [Export]
    // public int height = 100;

    // [Export]
    // public int width = 60;

    [Export]
    public NoiseConfig noiseConfig;

    NoiseMapFactory noiseMapFactory;

    // Map data
    Dictionary<Vector2I, Hex> mapData;

    // Map graphics
    // ------------------------------------------------------------
    TileMapLayer baseLayer, borderLayer, overlayLayer;
    Dictionary<TerrainType, Vector2I> terrainTextures;



    public override void _Ready()
    {
        GD.Print($"Load resources and setup");
        cityScene = GD.Load<PackedScene>("res://scenes/city.tscn");

        SetupNodeRefs();
        noiseMapFactory = new NoiseMapFactory(noiseConfig);
        mapData = new Dictionary<Vector2I, Hex>();

        GD.Print($"Creating map with width: {width} and height: {height}");

        InitializeTerrainTextures();
        GenerateTerrain();
        GenerateResources();

        // Reference to uiManager contains the signal, tie the local SendHexData
        // method to uiManager, this will receive a hex from the ui selection
        this.SendHexData += uiManager.SetSelectionUi;
        // Connect the signal
        uiManager.StartGamePressed += uiManager.HideStartGameUi;

        // Create a city
        CreateCity(new Civilization("Test"), new Vector2I(height / 2, width / 2), "Test City");

    }
    private void SetupNodeRefs()
    {
        uiManager = GetNode<UiManager>("/root/Game/CanvasLayer/NewUiManager");
        // uiManager = GetNode<UiManager>("/root/Game/CanvasLayer/UiManager");
        baseLayer = GetNode<TileMapLayer>("BaseLayer");
        borderLayer = GetNode<TileMapLayer>("BorderLayer");
        overlayLayer = GetNode<TileMapLayer>("OverlayLayer");
    }

    private void InitializeTerrainTextures()
    {
        terrainTextures = new Dictionary<TerrainType, Vector2I>
        {
            {TerrainType.PLAINS, new Vector2I(0, 0)},
            {TerrainType.WATER, new Vector2I(1, 0)},
            {TerrainType.DESERT, new Vector2I(0, 1)},
            {TerrainType.MOUNTAIN, new Vector2I(1, 1)},
            {TerrainType.SHALLOW_WATER, new Vector2I(1, 2)},
            {TerrainType.BEACH, new Vector2I(0, 2)},
            {TerrainType.FOREST, new Vector2I(1, 3)},
            {TerrainType.ICE, new Vector2I(0, 3)},
       };
    }


    // City Generation
    // ------------------------------------------------------------
    public void CreateCity(Civilization civ, Vector2I coords, string name)
    {
        City city = cityScene.Instantiate() as City;
        city.map = this;
        civ.cities.Add(city);
        city.civ = civ;

        AddChild(city);

        // Set Color of icon
        // Set the city name
        // Set the coordinates of the city (map & world)
        city.centerCoordinates = coords;
        city.Position = baseLayer.MapToLocal(coords);
        // Adding territory
        // populate borders
    }





    // Terrain Generation
    // ------------------------------------------------------------


    public void GenerateTerrain()
    {
        // Local noise maps are for keeping track durint the 
        // actual SetCell calls etc
        float[,] localBaseMap = new float[width, height];
        float[,] localForestMap = new float[width, height];
        float[,] localDesertMap = new float[width, height];
        float[,] localMountainMap = new float[width, height];

        // Generate base terrain noise
        NoiseMapData baseNoiseData = noiseMapFactory.CreateNoiseMap(NoiseMapType.BASE);
        NoiseMapData forestNoiseData = noiseMapFactory.CreateNoiseMap(NoiseMapType.FOREST);
        NoiseMapData desertNoiseData = noiseMapFactory.CreateNoiseMap(NoiseMapType.DESERT);
        NoiseMapData mountainNoiseData = noiseMapFactory.CreateNoiseMap(NoiseMapType.MOUNTAIN);

        // the passed in map is passed by reference and so mutable
        baseNoiseData.PopulateNoiseMap(localBaseMap, width, height);
        forestNoiseData.PopulateNoiseMap(localForestMap, width, height);
        desertNoiseData.PopulateNoiseMap(localDesertMap, width, height);
        mountainNoiseData.PopulateNoiseMap(localMountainMap, width, height);

        // I wonder if I could turn this into something which could be exported
        // OR! even cooler tweaked and re-rendered as the values are edited (or as a CustomResource)
        List<(float Min, float Max, TerrainType Type)> terrainGenValues = new List<(float Min, float Max, TerrainType Type)>
        {
            (0, baseNoiseData.MaxValue/10 * 2.5f, TerrainType.WATER), // The lower 25% of the map will be water, lower here is more like the 0-1 value of the noise
            (baseNoiseData.MaxValue/10 * 2.5f, baseNoiseData.MaxValue/10*4f, TerrainType.SHALLOW_WATER),
            (baseNoiseData.MaxValue/10*4f, baseNoiseData.MaxValue/10*4.5f, TerrainType.BEACH),
            (baseNoiseData.MaxValue/10*4.5f, baseNoiseData.MaxValue/10 + 1f, TerrainType.PLAINS), // The + is a little buffer, this is actually noiseMax itself, though if it is not set correctly map generation will exit early. Here I gave it a huge buffer.
        };

        Vector2 forestGenValues = new Vector2(forestNoiseData.MaxValue / 10 * 5, forestNoiseData.MaxValue + 0.05f);
        Vector2 desertGenValues = new Vector2(desertNoiseData.MaxValue / 10 * 8, desertNoiseData.MaxValue + 0.05f);
        Vector2 mountainGenValues = new Vector2(mountainNoiseData.MaxValue / 10 * 8f, mountainNoiseData.MaxValue + 0.05f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Hex h = new Hex(new Vector2I(x, y));
                float noiseValue = localBaseMap[x, y];

                // Use LINQ
                h.terrainType = terrainGenValues.First(range => noiseValue >= range.Min && noiseValue <= range.Max).Type;

                mapData[new Vector2I(x, y)] = h;

                // Desert
                if (localDesertMap[x, y] >= desertGenValues[0] &&
                localDesertMap[x, y] <= desertGenValues[1] &&
                h.terrainType == TerrainType.PLAINS)
                {
                    h.terrainType = TerrainType.DESERT;
                }

                // Forest
                if (localForestMap[x, y] >= forestGenValues[0] &&
                    localForestMap[x, y] <= forestGenValues[1] &&
                    h.terrainType == TerrainType.PLAINS)
                {
                    h.terrainType = TerrainType.FOREST;
                }

                // Mountain
                if (localMountainMap[x, y] >= mountainGenValues[0] &&
                    localMountainMap[x, y] <= mountainGenValues[1] &&
                    h.terrainType == TerrainType.PLAINS)
                {
                    h.terrainType = TerrainType.MOUNTAIN;
                }



                baseLayer.SetCell(new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
                borderLayer.SetCell(new Vector2I(x, y), 2, new Vector2I(0, 0));
            }

        }

        int maxIce = 5;
        Random r = new Random();
        for (int x = 0; x < width; x++)
        {
            // North Pole
            for (int y = 0; y < r.Next(maxIce) + 1; y++)
            {
                Hex h = mapData[new Vector2I(x, y)];
                h.terrainType = TerrainType.ICE;
                baseLayer.SetCell(new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
            }

            // South Pole
            for (int y = height - 1; y > height - 1 - r.Next(maxIce) - 1; y--)
            {
                Hex h = mapData[new Vector2I(x, y)];
                h.terrainType = TerrainType.ICE;
                baseLayer.SetCell(new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
            }
        }
    }

    public void GenerateResources()
    {
        Random r = new Random();
        foreach (var hex in mapData)
        {
            hex.Value.SetResources();
        }
    }




    // Utils
    // ------------------------------------------------------------

    // Convert map coordinates to local coordinates, since all the a
    // layers are the same size we could use on of them, but we use
    // the baseLayer because obviously
    public Vector2 MapToLocal(Vector2I coords)
    {
        return baseLayer.MapToLocal(coords);
    }

    public Vector2I GetMapPosition()
    {
        return baseLayer.LocalToMap(ToLocal(GetGlobalMousePosition()));
    }

    public Hex GetHexAtMapPosition(Vector2I coords)
    {
        return mapData[coords];
    }

    public void ClearHexSelection()
    {
        overlayLayer.SetCell(currentSelectedHex, -1);
        currentSelectedHex = new Vector2I(-1, -1);
        uiManager.HideAllPopups();
    }

    public void SetHexSelection(Vector2I coords)
    {
        if (currentSelectedHex == coords)
        {
            return;
        }
        overlayLayer.SetCell(currentSelectedHex, -1);
        overlayLayer.SetCell(coords, 2, new Vector2I(0, 1));
        currentSelectedHex = coords;
        SendHexData?.Invoke(mapData[coords]);
    }


}