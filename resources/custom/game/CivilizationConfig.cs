using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class CivilizationConfig : Resource
{
    [Export]
    public string Name { get; set; }

    [Export]
    public Color Color { get; set; }

    public bool IsPlayer { get; set; }

    public int Id { get; set; }

    [Export]
    public string[] CityNames { get; set; } = new string[] { };

    [Export]
    public UnitConfig[] Units { get; set; } = new UnitConfig[] { };


    [Export]
    public List<KeyValuePair<string, int>> KeyValuePairs = new List<KeyValuePair<string, int>>();
}