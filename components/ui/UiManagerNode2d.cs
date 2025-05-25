using Godot;
using System;

public partial class UiManagerNode : Node2D
{

    PackedScene selectionUiScene;
    PackedScene startGameUiScene;

    SelectionUi selectionUi;
    StartGameUi startGameUi;

    public override void _Ready()
    {
        selectionUiScene = ResourceLoader.Load<PackedScene>("components/ui/selection_ui.tscn");
        startGameUiScene = ResourceLoader.Load<PackedScene>("components/ui/start_game_ui.tscn"); ;

        startGameUi = (StartGameUi)startGameUiScene.Instantiate();

        // Get viewport size and center the UI
        // var viewportSize = GetViewport().GetVisibleRect().Size;
        // var centerPos = viewportSize / 2;
        // startGameUi.Position = centerPos;
        // AddChild(startGameUi);
    }

    public void HideAllPopups()
    {
        if (selectionUi is not null)
        {
            selectionUi.QueueFree();
            selectionUi = null;
        }
    }

    public void SetSelectionUi(Hex h)
    {
        if (selectionUi is not null) selectionUi.QueueFree();
        // selectionUI = selectionUiScene.Instantiate<SelectionUI>();
        // or
        // selectionUI = (SelectionUI) selectionUiScene.Instantiate();
        selectionUi = (SelectionUi)selectionUiScene.Instantiate();
        AddChild(selectionUi);
        selectionUi.SetHex(h);
    }

    public void HideStartGameUi()
    {
        if (startGameUi is not null)
        {
            startGameUi.QueueFree();
            startGameUi = null;
        }
    }

}
