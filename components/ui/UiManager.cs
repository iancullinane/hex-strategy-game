using Godot;
using System;

public partial class UiManager : Node2D
{

    PackedScene selectionUiScene;

    SelectionUi selectionUI;

    public override void _Ready()
    {
        selectionUiScene = ResourceLoader.Load<PackedScene>("components/ui/selection_ui.tscn");
    }

    public void HideAllPopups()
    {
        if (selectionUI is not null)
        {
            selectionUI.QueueFree();
            selectionUI = null;
        }
    }

    public void SetSelectionUi(Hex h)
    {
        if (selectionUI is not null) selectionUI.QueueFree();
        // selectionUI = selectionUiScene.Instantiate<SelectionUI>();
        // or
        // selectionUI = (SelectionUI) selectionUiScene.Instantiate();
        selectionUI = (SelectionUi)selectionUiScene.Instantiate();
        AddChild(selectionUI);
        selectionUI.SetHex(h);
    }

}
