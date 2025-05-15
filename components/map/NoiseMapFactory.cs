using Godot;
using System;

public class NoiseMapData
{
    public FastNoiseLite Noise { get; set; }
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

    public void PopulateNoiseMap(float[,] map, int width, int height)
    {
        GD.Print("Creating map with width: {0} and height: {1}", width, height);
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

/// <summary>
/// Factory class for creating different types of noise maps used in terrain generation.
/// </summary>
public class NoiseMapFactory
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Creates a noise map with parameters configured based on the specified type.
    /// </summary>
    /// <param name="type">The type of noise map to create (BASE, FOREST, etc)</param>
    /// <returns>A NoiseMapData object containing the configured noise generator</returns>
    /// <exception cref="ArgumentException">Thrown when an unknown noise map type is provided</exception>
    public static NoiseMapData CreateNoiseMap(NoiseMapType type)
    {
        NoiseMapData noiseMapData = new NoiseMapData();
        // This is just to make below here less verbose
        var noise = noiseMapData.Noise;
        noise.Seed = _random.Next(100000);

        switch (type)
        {
            case NoiseMapType.BASE:
                noise.Frequency = 0.008f;
                noise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
                noise.FractalOctaves = 4;
                noise.FractalLacunarity = 2.25f;
                break;
            case NoiseMapType.FOREST:
                noise.Frequency = 0.04f;
                noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
                noise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
                noise.FractalLacunarity = 2.0f;
                break;
            case NoiseMapType.DESERT:
                noise.Frequency = 0.015f;
                noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
                noise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
                noise.FractalOctaves = 3;
                noise.FractalLacunarity = 2f;
                break;
            case NoiseMapType.MOUNTAIN:
                noise.Frequency = 0.0121f;
                noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Value;
                noise.FractalType = FastNoiseLite.FractalTypeEnum.PingPong;
                noise.FractalOctaves = 5;
                noise.FractalLacunarity = 2.0f;
                break;
            default:
                throw new ArgumentException($"Unknown noise map type: {type}");
        }

        noiseMapData.Noise = noise;
        return noiseMapData;
    }
}