using Godot;
using System;

public partial class UnitUi : Panel
{
    TextureRect unitImage;

    Label unitTypeLabel, actionPointsLabel, strengthLabel, hpLabel;

    VBoxContainer actionsContainer;

    Unit unit;

    public override void _Ready()
    {
        unitImage = GetNode<TextureRect>("MarginContainer/Box/UnitImage");
        unitTypeLabel = GetNode<Label>("MarginContainer/Box/Body/UnitType");
        actionPointsLabel = GetNode<Label>("MarginContainer/Box/Body/AP");
        strengthLabel = GetNode<Label>("MarginContainer/Box/Body/Strength");
        hpLabel = GetNode<Label>("MarginContainer/Box/Body/Health");
        actionsContainer = GetNode<VBoxContainer>("MarginContainer/Box/Body/UnitActionsMenu");
    }

    public void SetUnit(Unit unit)
    {
        this.unit = unit;
        Refresh();
    }

    public void Refresh()
    {
        unitImage.Texture = unit.config.unitImage;
        unitTypeLabel.Text = unit.name;
        hpLabel.Text = $"HP {unit.hp}";
        actionPointsLabel.Text = $"Action points: {unit.actionPoints}";
        strengthLabel.Text = $"Strength: {unit.config.strength}";

        PopulateActionButtons();
    }

    private void PopulateActionButtons()
    {
        // Clear existing action buttons
        foreach (Node child in actionsContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Create buttons for each available action
        if (unit.config.actions != null)
        {
            foreach (UnitAction action in unit.config.actions)
            {
                // Skip Move action since it's handled by right-click
                if (action.Name == "Move")
                {
                    continue;
                }

                GameLogger.Debug("add button");
                Button actionButton = new Button();
                actionButton.Text = action.Name;
                actionButton.Pressed += () => ExecuteAction(action);

                // Disable button if action can't be executed
                bool canExecute = action.CanExecute(unit);
                actionButton.Disabled = !canExecute;

                if (!canExecute)
                {
                    actionButton.Text += " (Unavailable)";
                }
                GameLogger.Debug("add button");
                actionsContainer.AddChild(actionButton);
            }
        }
    }

    private void ExecuteAction(UnitAction action)
    {
        if (action.RequiresTarget)
        {
            // For now, we'll need to handle targeting separately
            // This could be enhanced later with a targeting mode
            GameLogger.Info("UnitUi", $"Action {action.Name} requires target selection");
        }
        else
        {
            // Execute actions that don't require targets (like Build City)
            action.Execute(unit);
            Refresh(); // Refresh UI after action
        }
    }
}
