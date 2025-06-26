using Godot;
using System;
using System.Collections.Generic;



public class Hex
{


    public City ownerCity;
    public readonly Vector2I coordinates;
    public TerrainType terrainType;

    public int food { get; set; }
    public int production { get; set; }


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

    // TODO configurable resource randomness
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