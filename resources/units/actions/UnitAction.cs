using Godot;

[GlobalClass]
public abstract partial class UnitAction : Resource
{
    [Export]
    public string Name { get; set; }

    [Export]
    public string Description { get; set; }

    [Export]
    public int ActionPointCost { get; set; } = 1;

    [Export]
    public bool RequiresTarget { get; set; } = false;

    public abstract bool CanExecute(Unit unit, Hex targetHex = null);
    public abstract void Execute(Unit unit, Hex targetHex = null);
}