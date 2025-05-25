using Godot;
using System;
using System.Collections.Generic;

public partial class City : Node2D
{

    // Base Info
    public string name;
    public Label label;
    public Sprite2D sprite;


    // References
    public TileMap map;
    public Vector2I centerCoordinates;

    // Territory    
    public List<Hex> territory;
    public List<Hex> borderTilePool;
    public List<Hex> borderTiles;

    // Civ Info
    public Civilization civ;


    public void _Ready()
    {
        // Get the map
        label = GetNode<Label>("Label");
        sprite = GetNode<Sprite2D>("Sprite2D");
        label.Text = name;
        territory = new List<Hex>();
        borderTilePool = new List<Hex>();

        // Get the center coordinates

    }
}
