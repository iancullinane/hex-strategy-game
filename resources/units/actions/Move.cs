using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Move : UnitAction
{
    public Move()
    {
        Name = "Move";
        Description = "Move unit to an adjacent tile";
        ActionPointCost = 1;
        RequiresTarget = true;
    }

    public override bool CanExecute(Unit unit, Hex targetHex)
    {
        // Check if unit has action points and target is in valid movement tiles
        return unit.actionPoints >= ActionPointCost && unit.validMovementTiles.Contains(targetHex);
    }

    public override void Execute(Unit unit, Hex targetHex)
    {
        if (!CanExecute(unit, targetHex))
        {
            return;
        }

        unit.MoveToHex(targetHex);
        unit.CalculateValidAdjacentTiles();
    }
}
