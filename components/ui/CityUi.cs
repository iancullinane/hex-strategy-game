using Godot;
using System;
using System.Collections;

public partial class CityUi : Panel
{


    public Label nameLabel, sizeLabel, populationLabel, productionLabel, foodLabel;

    City city;

    public override void _Ready()
    {
        nameLabel = GetNode<Label>("Margin/Box/Body/NameLabel");
        sizeLabel = GetNode<Label>("Margin/Box/Body/SizeLabel");
        // StatContainer is a VBoxContainer
        populationLabel = GetNode<Label>("Margin/Box/Body/StatContainer/PopulationLabel");
        productionLabel = GetNode<Label>("Margin/Box/Body/StatContainer/ProductionLabel");
        foodLabel = GetNode<Label>("Margin/Box/Body/StatContainer/FoodLabel");
    }


    public void ConnectUnitBuildSignals(City city)
    {
        VBoxContainer buildButtons = GetNode<VBoxContainer>("Margin/Box/Body/BuildOptionsContainer/OptionList");

        // Clear existing buttons
        foreach (Node child in buildButtons.GetChildren())
        {
            child.QueueFree();
        }

        // Get available units from civilization config
        // For now, fallback to hardcoded list if civ doesn't have units defined
        UnitConfig[] availableUnits;
        if (city.civ != null && HasUnitsInCivConfig(city))
        {
            // TODO: Get from city.civ.unitConfigs when that's implemented
            availableUnits = GetDefaultUnits();
        }
        else
        {
            availableUnits = GetDefaultUnits();
        }

        // Create buttons dynamically
        foreach (UnitConfig unitConfig in availableUnits)
        {
            // Load the button scene and create instance
            PackedScene buttonScene = GD.Load<PackedScene>("res://components/ui/city/unit_build_button.tscn");
            UnitBuildButton button = buttonScene.Instantiate<UnitBuildButton>();

            // Configure the button
            button.unitConfig = unitConfig;
            button.Text = unitConfig.name;

            // Connect signals
            button.OnPressed += city.AddToUnitBuildQueue;
            button.OnPressed += RefreshFromConfig;

            // Add to container
            buildButtons.AddChild(button);
        }
    }

    private UnitConfig[] GetDefaultUnits()
    {
        return new UnitConfig[] {
            GD.Load<UnitConfig>("res://resources/units/base/warrior.tres"),
            GD.Load<UnitConfig>("res://resources/units/base/settler.tres")
        };
    }

    private bool HasUnitsInCivConfig(City city)
    {
        // TODO: Check if city.civ has unit configs when that's implemented
        return false;
    }

    public void RefreshFromConfig(UnitConfig config)
    {
        Refresh();
    }

    public void SetCityUi(City city)
    {
        // city.CalculateTerritoryResourceTotals();
        this.city = city;
        Refresh();
        ConnectUnitBuildSignals(city);
    }

    public void Refresh()
    {
        nameLabel.Text = city.name;
        sizeLabel.Text = $"Level {city.size} city of {city.civ.name} ";
        populationLabel.Text = $"Population: {city.population}";
        productionLabel.Text = $"Production: {city.production}";
        foodLabel.Text = $"Food: {city.food}";

        PopulateUnitBuildQueueUI(city);
    }

    public void Refresh(Unit unit)
    {
        Refresh();
    }



    public void PopulateUnitBuildQueueUI(City inCity)
    {
        VBoxContainer unitBuildQueueContainer = GetNode<VBoxContainer>("Margin/Box/Body/QueueContainer/InProduction/ProductionList");

        foreach (Node n in unitBuildQueueContainer.GetChildren())
        {
            unitBuildQueueContainer.RemoveChild(n);
            n.QueueFree();
        }

        for (int i = 0; i < city.unitBuildQueue.Count; i++)
        {
            BuildQueueItem item = city.unitBuildQueue[i];

            if (i == 0)
            {
                unitBuildQueueContainer.AddChild(new Label() { Text = $"Building {item.config.name} {item.paid}/{item.config.cost}" });
            }
            else
            {
                unitBuildQueueContainer.AddChild(new Label() { Text = $"Building {item.config.name} {item.config.cost}" });
            }
        }
    }
}
