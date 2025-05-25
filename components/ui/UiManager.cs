using Godot;
using System;

public partial class UiManager : Control
{

    PackedScene selectionUiScene;
    PackedScene startGameUiScene;

    SelectionUi selectionUi;
    StartGameUi startGameUi;




    // Signals
    // ------------------------------------------------------------

    // I left this with the same name as the other so they are directly related
    [Signal]
    public delegate void StartGamePressedEventHandler();



    public override void _Ready()
    {
        selectionUiScene = ResourceLoader.Load<PackedScene>("components/ui/selection_ui.tscn");
        startGameUiScene = ResourceLoader.Load<PackedScene>("components/ui/StartGameUi.tscn"); ;

        ShowStartGameUi();
        // startGameUi = (StartGameUi)startGameUiScene.Instantiate();

        // var viewportSize = GetViewport().GetVisibleRect().Size;
        // var uiSize = startGameUi.Size; // Or CustomMinimumSize if Size is zero
        // var centerPos = (viewportSize - uiSize) / 2;
        // startGameUi.Position = centerPos;
        // AddChild(startGameUi);

        // AddChild(startGameUi);
    }

    public void ShowStartGameUi()
    {
        if (startGameUi != null)
            startGameUi.QueueFree();

        startGameUi = (StartGameUi)startGameUiScene.Instantiate();

        // Connect the child signal to this class's signal
        startGameUi.StartGamePressed += () => EmitSignal(SignalName.StartGamePressed);
        // var viewportSize = GetViewport().GetVisibleRect().Size;
        // var uiSize = startGameUi.Size; // Or CustomMinimumSize if Size is zero
        // var centerPos = (viewportSize - uiSize) / 2;
        // startGameUi.Position = centerPos;
        // Centering logic here if needed...

        AddChild(startGameUi);
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
        GD.Print("HideStartGameUi");
        if (startGameUi is not null)
        {
            startGameUi.QueueFree();
            startGameUi = null;
        }
    }

}
