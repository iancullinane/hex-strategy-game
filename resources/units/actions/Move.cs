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

        // Remove unit from old location
        var oldHex = unit.map.GetHexAtCoords(unit.unitCoords);
        Unit.unitLocations[oldHex].Remove(unit);
        if (Unit.unitLocations[oldHex].Count == 0)
        {
            Unit.unitLocations.Remove(oldHex);
        }

        // Update unit position
        unit.unitCoords = targetHex.coordinates;
        unit.Position = unit.map.MapToLocal(unit.unitCoords);
        unit.actionPoints -= ActionPointCost;

        // Add unit to new location
        if (Unit.unitLocations.ContainsKey(targetHex))
        {
            Unit.unitLocations[targetHex].Add(unit);
        }
        else
        {
            Unit.unitLocations[targetHex] = new List<Unit> { unit };
        }

        // Recalculate valid moves from new position
        unit.CalculateValidAdjacentTiles();
    }
}
