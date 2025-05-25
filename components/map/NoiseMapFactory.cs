using Godot;
using System;

/// <summary>
/// Factory class for creating noise maps used in terrain generation.
/// Manages different noise generators for various terrain features like base terrain,
/// forests, deserts, and mountains. Each noise generator can be configured with
/// different parameters to create distinct terrain patterns.
/// </summary>
public class NoiseMapFactory
{
    private FastNoiseLite baseNoise;
    private FastNoiseLite forestNoise;
    private FastNoiseLite desertNoise;
    private FastNoiseLite mountainNoise;

    public NoiseMapFactory(NoiseConfig noiseConfig)
    {
        this.baseNoise = noiseConfig.BaseNoise;
        this.forestNoise = noiseConfig.ForestNoise;
        this.desertNoise = noiseConfig.DesertNoise;
        this.mountainNoise = noiseConfig.MountainNoise;
    }

    /// <summary>
    /// Creates a NoiseMapData object configured for the specified terrain type.
    /// </summary>
    /// <param name="type">The type of noise map to create</param>
    /// <returns>A NoiseMapData object configured with the appropriate noise generator</returns>
    public NoiseMapData CreateNoiseMap(NoiseMapType type)
    {
        NoiseMapData noiseData = new NoiseMapData();
        switch (type)
        {
            case NoiseMapType.BASE:
                noiseData.Noise = baseNoise;
                break;
            case NoiseMapType.FOREST:
                noiseData.Noise = forestNoise;
                break;
            case NoiseMapType.DESERT:
                noiseData.Noise = desertNoise;
                break;
            case NoiseMapType.MOUNTAIN:
                noiseData.Noise = mountainNoise;
                break;
        }
        return noiseData;
    }
}

/// <summary>
/// Represents a noise map with its associated noise generator and maximum value.
/// Contains methods for populating a 2D array with noise values and tracking the
/// highest value generated. Used in terrain generation to create varied landscapes
/// by mapping noise values to different terrain types.
/// </summary>
public class NoiseMapData
{
    // Noise is the noise generation object, should be passed in during creation
    public FastNoiseLite Noise { get; set; }
    // MaxValue is the highest value reached during
    // map population, we use it as a clamp
    public float MaxValue { get; private set; }

    public NoiseMapData()
    {
        Noise = new FastNoiseLite();
        MaxValue = 0f;
    }

    public void UpdateMaxValue(float newValue)
    {
        if (newValue > MaxValue)
        {
            MaxValue = newValue;
        }
    }

    /// <summary>
    /// Populates the incoming noise map with values from this objects generator
    /// Accomplishes this by iterating over both dimensions of the incoming map
    /// and setting the value of each cell to the absolute value of the noise
    /// generator at that cell's coordinates.
    /// </summary>
    /// <param name="map">The map to populate</param>
    /// <param name="width">The width of the map</param>
    /// <param name="height">The height of the map</param>
    public void PopulateNoiseMap(float[,] map, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = Math.Abs(Noise.GetNoise2D(x, y));
                map[x, y] = value;
                if (map[x, y] > MaxValue)
                {
                    UpdateMaxValue(map[x, y]);
                }
            }
        }
    }
}

public enum NoiseMapType
{
    BASE,
    FOREST,
    DESERT,
    MOUNTAIN
}

