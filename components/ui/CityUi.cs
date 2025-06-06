using Godot;
using System;

public partial class CityUi : Panel
{


    public Label nameLabel, populationLabel, productionLabel, foodLabel;

    City city;

    public override void _Ready()
    {
        nameLabel = GetNode<Label>("Box/Body/NameLabel");
        populationLabel = GetNode<Label>("Box/Body/PopulationLabel");
        productionLabel = GetNode<Label>("Box/Body/ProductionLabel");
        foodLabel = GetNode<Label>("Box/Body/FoodLabel");
    }

    public void SetCityUi(City city)
    {
        city.CalculateTerritoryResourceTotals();
        this.city = city;
        nameLabel.Text = city.name;
        populationLabel.Text = $"Population: {city.population}";
        productionLabel.Text = $"Production: {city.production}";
        foodLabel.Text = $"Food: {city.food}";
    }
}
