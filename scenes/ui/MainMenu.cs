using Godot;
using System;

public partial class MainMenu : Control
{
    private const string TutorialLevelName = "Tutorial";
    private VBoxContainer _mainMenu = null!;
    private VBoxContainer _selectMenu = null!;
    private OptionsMenu _optionsMenu = null!;

    [Export]
    public PackedScene[] LevelsToSelect { get; set; } = Array.Empty<PackedScene>();

    [Export]
    public string[] LevelNames { get; set; } = Array.Empty<string>();

    public override void _Ready()
    {
        GetTree().Paused = false;

        _mainMenu = GetNode<VBoxContainer>("Container/MainMenu");
        _mainMenu.Show();
        PrepareMainMenu(_mainMenu);

        _selectMenu = GetNode<VBoxContainer>("Container/LevelSelectMenu");
        _selectMenu.Hide();
        PrepareLevelSelectionMenu(_selectMenu);

        _optionsMenu = GetNode<OptionsMenu>("Container/OptionsMenu");
        _optionsMenu.Hide();
        _optionsMenu.Closed += CloseOptionsMenu;

        GetNode<Button>("Container/MainMenu/PlayButton").GrabFocus();
    }

    public override void _UnhandledInput(InputEvent input)
    {
        if (input.IsActionPressed(InputActionNames.Escape))
        {
            if (_selectMenu.Visible) CloseSelectMenu();
            else if (_optionsMenu.Visible) CloseOptionsMenu();
        }

        GetTree().Root.SetInputAsHandled();
    }

    private void PrepareMainMenu(Control mainMenu)
    {
        var play = mainMenu.GetNode<Button>("PlayButton");
        play.Pressed += () =>
        {
            LaunchScene(GameData.PlayerStats.LastPlayedLevel ?? TutorialLevelName);
        };
        if (GameData.PlayerStats.LastPlayedLevel != null) play.Text = "Continue";

        var select = mainMenu.GetNode<Button>("SelectLevelButton");
        select.Pressed += () =>
        {
            GetNode<VBoxContainer>("Container/MainMenu").Hide();
            GetNode<VBoxContainer>("Container/LevelSelectMenu").Show();
            GetNode<Button>("Container/LevelSelectMenu/BackToMainMenuButton").GrabFocus();
        };

        var options = mainMenu.GetNode<Button>("OptionsButton");
        options.Pressed += () =>
        {
            GetNode<VBoxContainer>("Container/MainMenu").Hide();
            GetNode<VBoxContainer>("Container/OptionsMenu").Show();
            GetNode<Button>("Container/OptionsMenu/BackToMainMenuButton").GrabFocus();
        };

        var exit = mainMenu.GetNode<Button>("ExitButton");
        exit.Pressed += ExitGame;
    }

    private void PrepareLevelSelectionMenu(Control selectMenu)
    {
        var backButton = selectMenu.GetNode<Button>("BackToMainMenuButton");
        backButton.Pressed += CloseSelectMenu;

        if (LevelsToSelect.Length != LevelNames.Length) throw new ArgumentException("Invalid configuration");
        for (var i = 0; i < LevelsToSelect.Length; i++)
        {
            var name = LevelNames[i];
            var button = new Button { Text = name };
            button.Pressed += () => LaunchScene(name);
            button.Disabled = !(name == TutorialLevelName || GameData.PlayerStats.UnlockedLevels.Contains(name));
            selectMenu.AddChild(button);
        }
    }

    private void CloseSelectMenu()
    {
        _mainMenu.Show();
        _selectMenu.Hide();
        _mainMenu.GetNode<Button>("SelectLevelButton").GrabFocus();
    }

    private void CloseOptionsMenu()
    {
        _mainMenu.Show();
        _optionsMenu.Hide();
        GetNode<Button>("Container/MainMenu/OptionsButton").GrabFocus();
    }

    private void LaunchScene(string levelName)
    {
        var gamePacked = GD.Load<PackedScene>("res://scenes/Game.tscn");
        var levelIdx = Array.IndexOf(LevelNames, levelName);
        var levelPacked = LevelsToSelect[levelIdx];
        
        var game = gamePacked.Instantiate<Game>();
        game.ReplaceLevelWith(levelPacked.Instantiate<Node2D>());

        GetTree().Root.GetChild(0).QueueFree();
        GetTree().Root.AddChild(game);
    }

    private async void ExitGame()
    {
        try
        {
            await GameData.SaveAsync();
        }
        finally
        {
            GetTree().Quit();
        }
    }
}