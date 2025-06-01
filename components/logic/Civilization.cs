using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;

public partial class Civilization
{

    public string name;
    public Color color;

    public int id;
    public List<City> cities;
    public string[] cityNames;

    public Color territoryColor;

    public int territoryColorAltTileId;

    public bool isPlayer;


    public Civilization(int id, CivilizationConfig civConfig)
    {
        this.id = id;
        this.name = civConfig.Name;
        this.color = civConfig.Color;
        this.isPlayer = civConfig.IsPlayer;
        this.cityNames = civConfig.CityNames;

        cities = new List<City>();
    }

}
