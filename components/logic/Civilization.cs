using Godot;
using System;
using System.Collections.Generic;

public partial class Civilization
{

    public string name;

    public int id;
    public List<City> cities;

    public Color territoryColor;

    public bool isPlayer;

    public Civilization(string name)
    {
        this.name = name;
        cities = new List<City>();
    }
}
