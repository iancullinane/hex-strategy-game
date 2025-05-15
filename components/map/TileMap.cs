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

public class Hex
{
    public readonly Vector2I coordinates;
    public TerrainType terrainType;

    public Hex(Vector2I coordinates)
    {
        this.coordinates = coordinates;
    }


}

public partial class TileMap : Node2D
{
    [Export]
    public int height = 100;

    [Export]
    public int width = 60;

    [Export]
    FastNoiseLite noise;

    // Map data
    TileMapLayer baseLayer, borderLayer, overlayLayer;
    Dictionary<Vector2I, Hex> mapData;

    Dictionary<TerrainType, Vector2I> terrainTextures;


    public override void _Ready()
    {
        GD.Print($"Creating map with width: {width} and height: {height}");
        baseLayer = GetNode<TileMapLayer>("BaseLayer");
        borderLayer = GetNode<TileMapLayer>("BorderLayer");
        overlayLayer = GetNode<TileMapLayer>("OverlayLayer");

        // Again could be a type or something and then place files
        // or whatever directly into the mapData
        mapData = new Dictionary<Vector2I, Hex>();


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

        GenerateTerrain();
    }


    public void GenerateTerrain()
    {
        // Local noise maps are for keeping track durint the 
        // actual SetCell calls etc
        float[,] localBaseMap = new float[width, height];
        float[,] localForestMap = new float[width, height];
        float[,] localDesertMap = new float[width, height];
        float[,] localMountainMap = new float[width, height];

        // Generate base terrain noise
        NoiseMapData baseNoiseData = NoiseMapFactory.CreateNoiseMap(NoiseMapType.BASE);
        NoiseMapData forestNoiseData = NoiseMapFactory.CreateNoiseMap(NoiseMapType.FOREST);
        NoiseMapData desertNoiseData = NoiseMapFactory.CreateNoiseMap(NoiseMapType.DESERT);
        NoiseMapData mountainNoiseData = NoiseMapFactory.CreateNoiseMap(NoiseMapType.MOUNTAIN);

        // the passed in map is passed by reference and so mutable
        baseNoiseData.PopulateNoiseMap(localBaseMap, width, height);
        forestNoiseData.PopulateNoiseMap(localForestMap, width, height);
        desertNoiseData.PopulateNoiseMap(localDesertMap, width, height);
        mountainNoiseData.PopulateNoiseMap(localMountainMap, width, height);


        GD.Print($"noiseMax: {baseNoiseData.MaxValue}");
        // I wonder if I could turn this into something which could be exported
        // OR! even cooler tweaked and re-rendered as the values are edited (or as a CustomResource)
        List<(float Min, float Max, TerrainType Type)> terrainGenValues = new List<(float Min, float Max, TerrainType Type)>
        {
            (0, baseNoiseData.MaxValue/10 * 2.5f, TerrainType.WATER), // The lower 25% of the map will be water, lower here is more like the 0-1 value of the noise
            (baseNoiseData.MaxValue/10 * 2.5f, baseNoiseData.MaxValue/10*4f, TerrainType.SHALLOW_WATER),
            (baseNoiseData.MaxValue/10*4f, baseNoiseData.MaxValue/10*4.5f, TerrainType.BEACH),
            (baseNoiseData.MaxValue/10*4.5f, baseNoiseData.MaxValue/10 + 1f, TerrainType.PLAINS), // The + is a little buffer, this is actually noiseMax itself, though if it is not set correctly map generation will exit early. Here I gave it a huge buffer.
            // (noiseMax/10*5f, noiseMax/10*5.5f, TerrainType.FOREST),
            // (noiseMax/10*5.5f, noiseMax/10*6f, TerrainType.MOUNTAIN),
            // (noiseMax/10*6f, noiseMax/10*6.5f, TerrainType.ICE),
        };

        // Forest gen
        Vector2 forestGenValues = new Vector2(forestNoiseData.MaxValue / 10 * 4, forestNoiseData.MaxValue + 0.05f);
        Vector2 desertGenValues = new Vector2(desertNoiseData.MaxValue / 10 * 9, desertNoiseData.MaxValue + 0.05f);
        Vector2 mountainGenValues = new Vector2(mountainNoiseData.MaxValue / 10 * 2, mountainNoiseData.MaxValue + 0.05f);

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

                // Forest
                if (localForestMap[x, y] >= mountainGenValues[0] &&
                    localForestMap[x, y] <= mountainGenValues[1] &&
                    h.terrainType == TerrainType.PLAINS)
                {
                    h.terrainType = TerrainType.MOUNTAIN;
                }



                baseLayer.SetCell(new Vector2I(x, y), 0, terrainTextures[h.terrainType]);
                borderLayer.SetCell(new Vector2I(x, y), 2, new Vector2I(0, 0));
            }
        }
    }

    // Convert map coordinates to local coordinates, since all the a
    // layers are the same size we could use on of them, but we use
    // the baseLayer because obviously
    public Vector2 MapToLocal(Vector2I coords)
    {
        return baseLayer.MapToLocal(coords);
    }
}