using Godot;
using System;
using System.Collections.Generic;

public partial class SelectionUi : Panel
{


    public static Dictionary<TerrainType, string> terrainTypeStrings = new Dictionary<TerrainType, string>
    {
        {TerrainType.PLAINS , "Plains"},
        {TerrainType.MOUNTAIN , "Mountains"},
        {TerrainType.DESERT , "Desert"},
        {TerrainType.FOREST , "Forest"},
        {TerrainType.WATER , "Water"},
        {TerrainType.ICE , "Ice"},
        {TerrainType.SHALLOW_WATER , "Shallow Water"},
        {TerrainType.BEACH , "Beach"},

    };

    public static Dictionary<TerrainType, Texture2D> terrainTypeTextures = new Dictionary<TerrainType, Texture2D>
    {
        {TerrainType.PLAINS , GD.Load<Texture2D>("res://assets/images/terrain_graphics/plains.jpg")},
        {TerrainType.MOUNTAIN , GD.Load<Texture2D>("res://assets/images/terrain_graphics/mountain.jpg")},
        {TerrainType.DESERT , GD.Load<Texture2D>("res://assets/images/terrain_graphics/desert.jpg")},
        {TerrainType.FOREST , GD.Load<Texture2D>("res://assets/images/terrain_graphics/forest.jpg")},
        {TerrainType.WATER , GD.Load<Texture2D>("res://assets/images/terrain_graphics/ocean.jpg")},
        {TerrainType.ICE , GD.Load<Texture2D>("res://assets/images/terrain_graphics/ice.jpg")},
        {TerrainType.SHALLOW_WATER , GD.Load<Texture2D>("res://assets/images/terrain_graphics/shallow.jpg")},
        {TerrainType.BEACH , GD.Load<Texture2D>("res://assets/images/terrain_graphics/beach.jpg")},
    };


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
        terrainImage.Texture = terrainTypeTextures[h.terrainType];
        typeLabel.Text = terrainTypeStrings[h.terrainType];
        foodLabel.Text = $"Food: {h.food}";
        productionLabel.Text = $"Production: {h.production}";
    }
}
