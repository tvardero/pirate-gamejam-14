using Godot;
using System;

public partial class PauseMenu : Control
{
    private Control _menu = null!;
    private OptionsMenu _optionsMenu = null!;
    private PackedScene _mainMenuPacked = null!;

    public event Action? Closed;

    public override void _Ready()
    {
        _menu = GetNode<Control>("Menu");
        _menu.Show();
        PrepareMenu(_menu);

        _optionsMenu = GetNode<OptionsMenu>("OptionsMenu");
        _optionsMenu.Hide();
        _optionsMenu.Closed += CloseOptions;

        _mainMenuPacked = GD.Load<PackedScene>("res://scenes/ui/MainMenu.tscn");
    }

    public override void _UnhandledInput(InputEvent input)
    {
        if (input.IsActionPressed(InputActionNames.Escape))
        {
            if (_menu.Visible) Closed?.Invoke();
            if (_optionsMenu.Visible) CloseOptions();
        }

        GetTree().Root.SetInputAsHandled();
    }

    public void GrabFocusOnOpen() => _menu.GetNode<Button>("BackToGame").GrabFocus();

    private void PrepareMenu(Control menu)
    {
        var back = menu.GetNode<Button>("BackToGame");
        back.Pressed += () => Closed?.Invoke();

        var options = menu.GetNode<Button>("Options");
        options.Pressed += () =>
        {
            menu.Hide();
            _optionsMenu.Show();
            _optionsMenu.GetNode<Button>("BackToMainMenuButton").GrabFocus();
        };

        var mainMenu = menu.GetNode<Button>("MainMenu");
        mainMenu.Pressed += GotoMainMenu;
    }

    private void CloseOptions()
    {
        _optionsMenu.Hide();
        _menu.Show();
        _menu.GetNode<Button>("Options").GrabFocus();
    }

    private async void GotoMainMenu()
    {
        await GameData.SaveAsync();

        var mainMenu = _mainMenuPacked.Instantiate<MainMenu>();
        GetTree().Root.GetChild(0).QueueFree();
        GetTree().Root.AddChild(mainMenu);
    }
}