using Godot;
using System;

public partial class GeneralUi : Panel
{

    int turn = 0;
    Label turnLabel;

    public override void _Ready()
    {
        turnLabel = GetNode<Label>("Container/LabelContainer/TurnLabel");
        turnLabel.Text = "Turn " + turn;


    }

    public void IncrementTurn()
    {
        turn++;
        turnLabel.Text = "Turn " + turn;
    }

    public Button GetEndTurnButton()
    {
        return GetNode<Button>("Container/ButtonContainer/EndTurnButton");
    }

}
