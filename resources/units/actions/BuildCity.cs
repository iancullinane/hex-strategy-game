using Godot;

[GlobalClass]
public partial class BuildCity : UnitAction
{
    public BuildCity()
    {
        Name = "Build City";
        Description = "Found a new city at this location";
        ActionPointCost = 2;
        RequiresTarget = false;
    }

    public override bool CanExecute(Unit unit, Hex targetHex = null)
    {
        if (unit.actionPoints < ActionPointCost) return false;

        var currentHex = unit.map.GetHexAtCoords(unit.unitCoords);

        // Check if current tile is suitable for a city
        if (currentHex.terrainType == TerrainType.WATER ||
            currentHex.terrainType == TerrainType.SHALLOW_WATER ||
            currentHex.terrainType == TerrainType.ICE ||
            currentHex.terrainType == TerrainType.MOUNTAIN ||
            currentHex.terrainType == TerrainType.DESERT)
        {
            return false;
        }

        // Check if there's already a city here
        if (currentHex.isCityCenter) return false;

        // Check if tile is too close to another city
        if (City.staticInvalidTiles.ContainsKey(currentHex))
        {
            return false;
        }

        return true;
    }

    public override void Execute(Unit unit, Hex targetHex = null)
    {
        if (!CanExecute(unit, targetHex))
        {
            GameLogger.Warning("BuildCity", "Cannot build city at this location");
            return;
        }

        var cityName = unit.civ.GetNextCityName();
        GameLogger.Info("BuildCity", $"Building city '{cityName}' at {unit.unitCoords}");

        // Create the city
        unit.map.CreateCity(unit.civ, unit.unitCoords, cityName);

        // Remove the settler unit (it becomes the city)
        unit.RemoveUnit();
    }
}