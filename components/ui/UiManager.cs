using Godot;
using System;

public partial class UiManager : Control
{

    PackedScene selectionUiScene;
    PackedScene startGameUiScene;
    PackedScene generalUiScene;
    PackedScene cityUiScene;
    PackedScene unitUiScene;

    SelectionUi selectionUi;
    StartGameUi startGameUi;
    CityUi cityUi;
    GeneralUi generalUi;
    UnitUi unitUi;

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
        unitUiScene = ResourceLoader.Load<PackedScene>("components/ui/UnitUi.tscn");


        ShowStartGameUi();
    }

    public void SignalEndTurn()
    {
        EmitSignal(SignalName.EndTurn);
        generalUi.IncrementTurn();
        RefreshUI();
    }


    public void RefreshUI()
    {
        if (cityUi is not null) cityUi.Refresh();
        if (unitUi is not null) unitUi.Refresh();
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

    // Separate hide methods for better control
    public void HideAllPopups()
    {
        HideSelectionUi();
        HideCityUi();
        HideUnitUi();
    }

    public void HideSelectionUi()
    {
        if (selectionUi is not null)
        {
            selectionUi.QueueFree();
            selectionUi = null;
        }
    }

    public void HideCityUi()
    {
        if (cityUi is not null)
        {
            cityUi.QueueFree();
            cityUi = null;
        }
    }

    public void HideUnitUi()
    {
        if (unitUi is not null)
        {
            unitUi.QueueFree();
            unitUi = null;
        }
    }

    public void SetCityUi(City city)
    {
        HideAllPopups(); // Cities are exclusive
        cityUi = cityUiScene.Instantiate<CityUi>();
        AddChild(cityUi);
        cityUi.SetCityUi(city);
    }

    public void SetSelectionUi(Hex h)
    {
        // Only hide city UI and selection UI - keep unit UI if active
        HideCityUi();
        HideSelectionUi();

        selectionUi = selectionUiScene.Instantiate<SelectionUi>();
        AddChild(selectionUi);
        selectionUi.SetHex(h);
    }

    public void SetUnitUi(Unit unit)
    {
        // Only hide city UI - keep selection UI if active
        HideCityUi();
        HideUnitUi(); // Replace previous unit UI

        unitUi = unitUiScene.Instantiate<UnitUi>();
        AddChild(unitUi);
        unitUi.SetUnit(unit);
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
