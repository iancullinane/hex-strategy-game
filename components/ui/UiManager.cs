using Godot;
using System;

public partial class UiManager : Control
{

    PackedScene selectionUiScene;
    PackedScene startGameUiScene;
    PackedScene generalUiScene;
    PackedScene cityUiScene;

    SelectionUi selectionUi;
    StartGameUi startGameUi;
    CityUi cityUi;
    GeneralUi generalUi;

    // Signals
    // ------------------------------------------------------------

    // I left this with the same name as the other so they are directly related
    [Signal]
    public delegate void StartGamePressedEventHandler();

    [Signal]
    public delegate void EndTurnEventHandler();

    public override void _Ready()
    {
        selectionUiScene = ResourceLoader.Load<PackedScene>("components/ui/SelectionUi.tscn");
        startGameUiScene = ResourceLoader.Load<PackedScene>("components/ui/StartGameUi.tscn");
        cityUiScene = ResourceLoader.Load<PackedScene>("components/ui/CityUi.tscn");
        generalUiScene = ResourceLoader.Load<PackedScene>("components/ui/GeneralUi.tscn");



        ShowStartGameUi();
    }

    public void SignalEndTurn()
    {
        EmitSignal(SignalName.EndTurn);
        generalUi.IncrementTurn();
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

    public void ShowGeneralUi()
    {
        GD.Print("Showing general ui");
        if (generalUi is not null)
            generalUi.QueueFree();

        generalUi = generalUiScene.Instantiate<GeneralUi>();
        Button endTurnButton = generalUi.GetEndTurnButton();
        endTurnButton.Pressed += () => SignalEndTurn();
        AddChild(generalUi);
    }

    public void HideAllPopups()
    {
        if (selectionUi is not null)
        {
            selectionUi.QueueFree();
            selectionUi = null;
        }

        if (cityUi is not null)
        {
            cityUi.QueueFree();
            cityUi = null;
        }
    }

    public void SetCityUi(City city)
    {
        HideAllPopups();
        cityUi = cityUiScene.Instantiate<CityUi>();
        AddChild(cityUi);
        cityUi.SetCityUi(city);
    }

    public void SetSelectionUi(Hex h)
    {
        HideAllPopups();
        if (selectionUi is not null) selectionUi.QueueFree();
        selectionUi = selectionUiScene.Instantiate<SelectionUi>();
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
