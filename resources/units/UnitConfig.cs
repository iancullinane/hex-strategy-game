using Godot;

[GlobalClass]
public partial class UnitConfig : Resource
{

    public enum UnitType
    {
        Settler,
        Warrior,
    }

    [Export]
    public UnitType unitType { get; set; }

    [Export]
    public string name { get; set; }

    [Export]
    public int cost { get; set; }

    [Export]
    public int hp { get; set; }

    [Export]
    public int actionPoints { get; set; }

    [Export]
    public int strength { get; set; }

    [Export]
    public Texture2D unitImage { get; set; }
    [Export]
    public Texture2D unitIcon { get; set; }

    [Export]
    public UnitAction[] actions { get; set; } = new UnitAction[] { };

}

