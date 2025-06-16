using Godot;
using System;

public partial class UnitUi : Panel
{
    TextureRect unitImage;

    Label unitType, movements, hp;

    Unit unit;

    public override void _Ready()
    {
        unitImage = GetNode<TextureRect>("MarginContainer/Box/UnitImage");
        unitType = GetNode<Label>("MarginContainer/Box/Body/UnitType");
        movements = GetNode<Label>("MarginContainer/Box/Body/Move");
        hp = GetNode<Label>("MarginContainer/Box/Body/Health");
    }

    public void SetUnit(Unit unit)
    {
        this.unit = unit;
        Refresh();
    }

    public void Refresh()
    {
        unitImage.Texture = unit.config.unitImage;
        unitType.Text = unit.name;
        movements.Text = $"Movements {unit.currentMovementPoints}/{unit.config.movementPoints}";
        // hp.Text = unit.config.hp.ToString();
        hp.Text = $"HP {unit.currentHp}/{unit.config.hp}";
    }
}
