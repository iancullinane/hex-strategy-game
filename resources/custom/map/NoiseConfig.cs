using Godot;

[GlobalClass]
public partial class NoiseConfig : Resource
{
    [Export] public FastNoiseLite BaseNoise { get; set; }
    [Export] public FastNoiseLite ForestNoise { get; set; }
    [Export] public FastNoiseLite DesertNoise { get; set; }
    [Export] public FastNoiseLite MountainNoise { get; set; }
}
