using Godot;
using Godot.Collections;

[GlobalClass]
public partial class GameDatabase : Resource
{
    [Export] public Array<BuildingDefinition> Buildings { get; set; } = new();
    [Export] public Array<UnitDefinition> Units { get; set; } = new();
    public Array<BuildingDefinition> GetAllBuildings() => Buildings;
    public Array<UnitDefinition> GetAllUnits() => Units;
    public Array<LootDefinitionModel> GetAllLoot()
    {
        var lootDefinitions = new Array<LootDefinitionModel>();
        foreach (var building in Buildings)
        {
            if (building != null)
                lootDefinitions.Add(building);
        }
        foreach (var unit in Units)
        {
            if (unit != null)
                lootDefinitions.Add(unit);
        }
        return lootDefinitions;
    }

    public BuildingDefinition GetBuilding(string id)
    {
        foreach (var building in Buildings)
        {
            if (building != null && building.CoreAttributes.Id == id)
                return building;
        }
        return null;
    }

    public UnitDefinition GetUnit(string id)
    {
        foreach (var unit in Units)
        {
            if (unit != null && unit.CoreAttributes.Id == id)
                return unit;
        }
        return null;
    }
    public LootDefinitionModel GetLootById(string id)
    {
        foreach (var building in Buildings)
        {
            if (building != null && building.CoreAttributes.Id == id)
                return building;
        }
        foreach (var unit in Units)
        {
            if (unit != null && unit.CoreAttributes.Id == id)
                return unit;
        }
        return null;
    }
}