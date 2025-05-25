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
    }

    public void ShowStartGameUi()
    {
        if (startGameUi != null)
            startGameUi.QueueFree();

        startGameUi = startGameUiScene.Instantiate<StartGameUi>();

        // Connect the child signal to this class's signal
        startGameUi.StartGamePressed += () => EmitSignal(SignalName.StartGamePressed);

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
        selectionUi = selectionUiScene.Instantiate<SelectionUi>();
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
