using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

public partial class Civilization
{

    // General housekeeping
    public int id;
    public string name;
    public bool isPlayer;
    public CivilizationConfig config;


    // City values
    public string[] cityNames;
    public List<City> cities;
    public List<Unit> units;

    //  Display
    public Color color;
    public Color territoryColor;
    public int territoryColorAltTileId;



    public Civilization(int id, CivilizationConfig civConfig)
    {
        this.config = civConfig;
        this.isPlayer = civConfig.IsPlayer;
        this.id = id;
        this.name = civConfig.Name;
        this.color = civConfig.Color;
        this.cityNames = civConfig.CityNames;

        cities = new List<City>();
        units = new List<Unit>();
    }


    public void ProcessTurn()
    {
        foreach (City city in cities)
        {
            city.ProcessTurn();
        }
        // Add AI
        if (!isPlayer)
        {

            Random r = new Random();
            // TODO: Add AI logic
            foreach (City city in cities)
            {
                int rand = r.Next(30);
                if (rand > 27)
                {
                    city.AddToUnitBuildQueue(UnitType.Warrior);
                }
            }
        }
    }


    public string GetNextCityName()
    {
        if (cityNames == null || cityNames.Length == 0)
        {
            return "New City";
        }

        string nextName = cityNames[0];
        cityNames = cityNames.Skip(1).ToArray();
        return nextName;
    }

}
