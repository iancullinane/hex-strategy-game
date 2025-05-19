using Godot;
using System;

public partial class SelectionUi : Panel
{


    // Data container
    Hex h = null;

    // UI Accessors
    TextureRect terrainImage;
    Label typeLabel, foodLabel, productionLabel;

    public override void _Ready()
    {
        terrainImage = GetNode<TextureRect>("Box/TerrainImage");
        typeLabel = GetNode<Label>("Box/Body/TypeLabel");
        foodLabel = GetNode<Label>("Box/Body/FoodLabel");
        productionLabel = GetNode<Label>("Box/Body/ProductionLabel");

        // Ensure the panel has a size
        CustomMinimumSize = new Vector2(200, 150);

        // Set background color for visibility (optional)
        // AddThemeColorOverride("background_color", new Color(0.2f, 0.2f, 0.2f, 0.8f));
    }

    public void SetHex(Hex h)
    {
        this.h = h;
        foodLabel.Text = $"Food: {h.food}";
        productionLabel.Text = $"Production: {h.production}";
    }
}
