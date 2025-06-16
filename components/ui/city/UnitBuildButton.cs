using Godot;
using System;

public partial class UnitBuildButton : Button
{
    public UnitConfig unitConfig;

    // Override the default OnPressed signal because it
    // does not provide a way to pass UnitConfig data out like this
    [Signal]
    public delegate void OnPressedEventHandler(UnitConfig config);

    public override void _Ready()
    {
        Pressed += SendUnitConfigData;
    }

    public void SendUnitConfigData()
    {
        EmitSignal(SignalName.OnPressed, unitConfig);
    }
}
