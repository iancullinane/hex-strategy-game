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

    private Dictionary<string, UnitConfig> _unitDictionary;
    public Dictionary<string, UnitConfig> UnitDictionary
    {
        get
        {
            if (_unitDictionary == null)
            {
                _unitDictionary = new Dictionary<string, UnitConfig>();
                foreach (var unit in Units)
                {
                    if (unit != null)
                        _unitDictionary[unit.name] = unit;
                }
            }
            return _unitDictionary;
        }
    }

}