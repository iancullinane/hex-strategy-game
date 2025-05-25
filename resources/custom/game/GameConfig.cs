using Godot;

[GlobalClass]
public partial class GameConfig : Resource
{
    [Export]
    public int MapWidth { get; set; } = 60;

    [Export]
    public int MapHeight { get; set; } = 100;

    [Export]
    public float CameraZoom { get; set; } = 1.0f;

    [Export]
    public Vector2 CameraOffset { get; set; } = Vector2.Zero;

    [Export]
    public bool DebugMode { get; set; } = false;

    [Export]
    public string[] CivilizationNames { get; set; } = { "Boston" };

}