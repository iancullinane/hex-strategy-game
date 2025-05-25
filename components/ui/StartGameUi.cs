using Godot;
using System;

public partial class StartGameUi : Control
{
    [Signal]
    public delegate void StartGamePressedEventHandler();

    public override void _Ready()
    {

        // Anchors to top-left
        AnchorLeft = 0.4f;
        AnchorTop = 0.4f;
        AnchorRight = 0.6f;
        AnchorBottom = 0.6f;

        // Offsets to 0 (or a little padding)
        OffsetLeft = 10;
        OffsetTop = 10;

        var btn = GetNode<Button>("Panel/MarginContainer/StartGameBtn");
        btn.Pressed += () => EmitSignal(SignalName.StartGamePressed);
    }
}
