using Godot;
using System;

public partial class StartGameUi : Control
{
    [Signal]
    public delegate void StartGamePressedEventHandler();

    public override void _Ready()
    {
        var btn = GetNode<Button>("Panel/MarginContainer/StartGameBtn");
        btn.Pressed += () => EmitSignal(SignalName.StartGamePressed);
    }
}
