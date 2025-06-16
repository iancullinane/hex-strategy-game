using Godot;

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



}