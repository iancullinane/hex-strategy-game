using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
    CIV_COLOR_BASE,
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

    public City ownerCity;

    public bool isCityCenter = false;

    public Hex(Vector2I coordinates)
    {
        this.coordinates = coordinates;
        ownerCity = null;
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

    // Map graphics
    // ------------------------------------------------------------
    TileMapLayer baseLayer, civLayer, borderLayer, overlayLayer;
    Dictionary<TerrainType, Vector2I> terrainTextures;

    TileSetAtlasSource terrainAtlas;

    // Ui
    Vector2I currentSelectedHex = new Vector2I(-1, -1);
    // ------------------------------------------------------------
    UiManager uiManager;

    // Signals
    // ------------------------------------------------------------
    public delegate void HexSelectedEventHandler(Hex h);
    public event HexSelectedEventHandler SendHexData;

    [Signal]
    public delegate void SendCityUiInfoEventHandler(City c);



    // Map settings
    // ------------------------------------------------------------
    public int height = 10;
    public int width = 10;

    public NoiseConfig noiseConfig;

    NoiseMapFactory noiseMapFactory;

    // Game data
    Dictionary<Vector2I, Hex> mapData;

    public Dictionary<Vector2I, City> cities;
    public List<Civilization> civilizations;

    public override void _Ready()
    {
        GD.Print($"Load resources and setup");
        cityScene = GD.Load<PackedScene>("res://scenes/city.tscn");
        this.SendHexData += uiManager.SetSelectionUi;
        uiManager.EndTurn += ProcessTurn;

        InitializeTerrainTextures();
        SetupNodeRefs();
    }


    public void SetupMap(GameConfig config, NoiseConfig gameNoiseConfig, UiManager uiManager)
    {
        GD.Print($"Setting up map with width: {config.MapWidth} and height: {config.MapHeight}");

        cities = new Dictionary<Vector2I, City>();

        width = config.MapWidth;
        height = config.MapHeight;
        noiseConfig = gameNoiseConfig;
        noiseMapFactory = new NoiseMapFactory(noiseConfig);
        mapData = new Dictionary<Vector2I, Hex>();
        this.uiManager = uiManager;
    }


    private void SetupNodeRefs()
    {
        uiManager = GetNode<UiManager>("/root/Game/CanvasLayer/NewUiManager");
        // uiManager = GetNode<UiManager>("/root/Game/CanvasLayer/UiManager");
        baseLayer = GetNode<TileMapLayer>("BaseLayer");
        civLayer = GetNode<TileMapLayer>("CivLayer");
        borderLayer = GetNode<TileMapLayer>("BorderLayer");
        overlayLayer = GetNode<TileMapLayer>("OverlayLayer");

        terrainAtlas = civLayer.TileSet.GetSource(0) as TileSetAtlasSource;
    }

    // City Generation
    // ------------------------------------------------------------

    public List<Vector2I> GenerateCivStartingLocations(int numLocations)
    {
        List<Vector2I> locations = new List<Vector2I>();
        List<Vector2I> plainsTiles = new List<Vector2I>();

        ForEachHex((x, y, hex) =>
        {
            if (hex.terrainType == TerrainType.PLAINS)
            {
                plainsTiles.Add(new Vector2I(x, y));
            }
        });

        Random r = new Random();
        for (int i = 0; i < numLocations; i++)
        {
            Vector2I coords = new Vector2I();

            bool valid = false;
            int counter = 0;

            while (!valid && counter < 10000)
            {
                coords = plainsTiles[r.Next(plainsTiles.Count)];
                valid = IsValidLocation(coords, locations);
                counter++;
            }

            plainsTiles.Remove(coords);
            foreach (Hex h in GetSurroundingTiles(coords))
            {
                foreach (Hex j in GetSurroundingTiles(h.coordinates))
                {
                    foreach (Hex k in GetSurroundingTiles(j.coordinates))
                    {
                        plainsTiles.Remove(h.coordinates);
                        plainsTiles.Remove(j.coordinates);
                        plainsTiles.Remove(k.coordinates);
                    }
                }

            }

            locations.Add(coords);
        }

        return locations;
    }

    private bool IsValidLocation(Vector2I coord, List<Vector2I> locations)
    {
        if (coord.X < 3 || coord.X > width - 3 ||
            coord.Y < 3 || coord.X > width - 3)
        {
            return false;
        }

        foreach (Vector2I location in locations)
        {
            if (Math.Abs(coord.X - location.X) <= 20 &&
                Math.Abs(coord.Y - location.Y) <= 20)
            {
                return false;
            }
        }

        return true;
    }

    public void CreateCity(Civilization civ, Vector2I cityCoords, string name)
    {
        City city = cityScene.Instantiate() as City;
        city.map = this;
        civ.cities.Add(city);
        city.civ = civ;

        city.SetCityName(name);
        city.SetIconColor(civ.color);
        AddChild(city);
        city.centerCoordinates = cityCoords;
        mapData[cityCoords].isCityCenter = true;
        city.Position = baseLayer.MapToLocal(cityCoords);

        // Add the city's center tile
        city.AddTerritory(new List<Hex> { mapData[cityCoords] });
        // // Add a ring of tiles around the city
        List<Hex> surroundingTiles = GetRandomSurroundingTile(cityCoords);

        foreach (Hex hex in surroundingTiles)
        {
            if (hex.ownerCity == null)
            {
                hex.ownerCity = city;
                city.AddTerritory(new List<Hex> { hex });
            }
        }
        UpdateCivTerritoryMap(civ);

        cities[cityCoords] = city;
    }

    public void UpdateCivTerritoryMap(Civilization civ)
    {
        foreach (City c in civ.cities)
        {
            foreach (Hex h in c.territory)
            {
                civLayer.SetCell(h.coordinates, 0, terrainTextures[TerrainType.CIV_COLOR_BASE], civ.territoryColorAltTileId);
            }
        }
    }

    public List<Hex> GetSurroundingTiles(Vector2I coords)
    {
        List<Hex> surroundingTiles = new List<Hex>();

        foreach (Vector2I coord in baseLayer.GetSurroundingCells(coords))
        {
            if (HexInBounds(coord))
            {
                surroundingTiles.Add(mapData[coord]);
            }
        }

        return surroundingTiles;
    }

    public List<Hex> GetRandomSurroundingTile(Vector2I coords)
    {
        List<Hex> surroundingTiles = GetSurroundingTiles(coords);
        if (surroundingTiles.Count == 0)
            return new List<Hex>();

        Random random = new Random();
        int index = random.Next(surroundingTiles.Count);
        return new List<Hex> { surroundingTiles[index] };
    }


    // Generators
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



                baseLayer.SetCell(new Vector2I(x, y), 1, terrainTextures[h.terrainType]);
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
                baseLayer.SetCell(
                    new Vector2I(x, y),
                    0,
                    terrainTextures[h.terrainType]);
            }
        }
        GD.Print("Terrain generated");
    }

    public void GenerateResources()
    {
        Random r = new Random();
        foreach (var hex in mapData)
        {
            hex.Value.SetResources();
        }
    }


    public void GenerateCivilizations(CivilizationConfig[] civConfigs)
    {
        List<Vector2I> startingLocations = GenerateCivStartingLocations(civConfigs.Length);

        civilizations = new List<Civilization>();
        for (int i = 0; i < civConfigs.Length; i++)
        {
            Civilization civ = new Civilization(i, civConfigs[i]);
            civilizations.Add(civ);

        }

        foreach (Civilization civ in civilizations)
        {

            int altTileId = terrainAtlas.CreateAlternativeTile(terrainTextures[TerrainType.CIV_COLOR_BASE]);
            terrainAtlas.GetTileData(terrainTextures[TerrainType.CIV_COLOR_BASE], altTileId).Modulate = civ.color;

            civ.territoryColorAltTileId = altTileId;

            CreateCity(civ, startingLocations[0], civ.GetNextCityName());
            startingLocations.RemoveAt(0);
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


    // 
    // 
    // 

    public void ProcessTurn()
    {
        GD.Print("Processing turn");
    }


    /// <summary>
    /// Converts the current global mouse position to map coordinates
    /// </summary>
    /// <returns>The map coordinates (Vector2I) corresponding to the current mouse position</returns>
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

    public bool HexInBounds(Vector2I coords)
    {
        return (coords.X >= 0 && coords.X < width &&
                coords.Y >= 0 && coords.Y < height);
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

        if (cities.ContainsKey(coords))
        {
            EmitSignal(SignalName.SendCityUiInfo, cities[coords]);
            return;
        }

        SendHexData?.Invoke(mapData[coords]);
    }

    public Hex GetRandomHex()
    {
        if (mapData.Count == 0)
        {
            return null;
        }

        Random random = new Random();
        int index = random.Next(mapData.Count);
        return mapData.ElementAt(index).Value;
    }


    // Graphics
    // ------------------------------------------------------------
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
            {TerrainType.CIV_COLOR_BASE, new Vector2I(0, 3)},
       };
    }



    public void ForEachHex(Action<int, int, Hex> action)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var coords = new Vector2I(x, y);
                if (mapData.TryGetValue(coords, out Hex hex))
                {
                    action(x, y, hex);
                }
            }
        }
    }


    public Civilization GetCiv(int id)
    {
        return civilizations.FirstOrDefault(c => c.id == id);
    }





}