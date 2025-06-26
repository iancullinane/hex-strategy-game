using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnitType = UnitConfig.UnitType;

public partial class Civilization
{

    // General housekeeping
    public int id;
    public string name;
    public bool isPlayer;
    public CivilizationConfig config;

    public int maxUnits = 3;
    public int maxUnitsBase = 3;

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
        if (isPlayer)
        {
            foreach (City city in cities)
            {
                city.ProcessTurn();
            }   // TODO: Add player logic
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
                    city.AddToUnitBuildQueue(config.UnitDictionary["Settler"]);
                }
                if (rand > 28)
                {
                    city.AddToUnitBuildQueue(config.UnitDictionary["Warrior"]);
                }
                city.ProcessTurn();
            }

            foreach (Unit unit in units)
            {

                if (unit.unitType is UnitType.Settler)
                {
                    unit.AI_Settle();
                }

                unit.AI_RandomMove();
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

    public void UpdateMaxUnits()
    {
        maxUnits = maxUnitsBase * cities.Count;
    }

    /// <summary>
    /// Gets the total number of units including those on the map and those in production queues
    /// </summary>
    /// <returns>Total unit count (existing + in production)</returns>
    public int GetTotalUnitCount()
    {
        int totalUnits = units.Count; // Units on the map

        // Add units in production queues across all cities
        foreach (City city in cities)
        {
            totalUnits += city.unitBuildQueue.Count;
        }

        return totalUnits;
    }

    /// <summary>
    /// Checks if the civilization can build more units (including queued ones)
    /// </summary>
    /// <returns>True if under the unit limit, false otherwise</returns>
    public bool CanBuildMoreUnits()
    {
        return GetTotalUnitCount() < maxUnits;
    }

}
