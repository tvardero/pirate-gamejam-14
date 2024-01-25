using Godot;
using System;

public partial class MainMenu : Control
{
    [Export]
    public PackedScene[] LevelsToSelect { get; set; } = Array.Empty<PackedScene>();

    [Export]
    public string[] LevelNames { get; set; } = Array.Empty<string>();

    public override void _Ready()
    {
        var mainMenu = GetNode<VBoxContainer>("Container/MainMenu");
        mainMenu.Show();
        PrepareMainMenu(mainMenu);

        var selectMenu = GetNode<VBoxContainer>("Container/LevelSelectMenu");
        selectMenu.Hide();
        PrepareLevelSelectionMenu(selectMenu);

        var optionsMenu = GetNode<VBoxContainer>("Container/OptionsMenu");
        optionsMenu.Hide();
        PrepareOptionsMenu(optionsMenu);

        GetNode<Button>("Container/MainMenu/PlayButton").GrabFocus();
    }

    private void PrepareMainMenu(Control mainMenu)
    {
        var play = mainMenu.GetNode<Button>("PlayButton");
        play.Pressed += () =>
        {
            var levelPath = GameData.PlayerStats.LastPlayedLevel ?? "res://scenes/levels/Tutorial001.tscn";
            var levelPacked = GD.Load<PackedScene>(levelPath);
            LaunchScene(levelPacked);
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

    private void PrepareLevelSelectionMenu(Control container)
    {
        var backButton = container.GetNode<Button>("BackToMainMenuButton");
        backButton.Pressed += () =>
        {
            GetNode<VBoxContainer>("Container/MainMenu").Show();
            GetNode<VBoxContainer>("Container/LevelSelectMenu").Hide();
            GetNode<Button>("Container/MainMenu/SelectLevelButton").GrabFocus();
        };

        if (LevelsToSelect.Length != LevelNames.Length) throw new ArgumentException("Invalid configuration");
        for (var i = 0; i < LevelsToSelect.Length; i++)
        {
            var name = LevelNames[i];
            var packed = LevelsToSelect[i];
            var button = new Button { Text = name };
            button.Pressed += () => LaunchScene(packed);
            container.AddChild(button);
        }
    }

    private void PrepareOptionsMenu(Control container)
    {
        var backButton = container.GetNode<Button>("BackToMainMenuButton");
        backButton.Pressed += BackToMainMenu;

        var music = container.GetNode<SliderOption>("Music");
        music.ValueChanged += val => GameData.GameOptions.MusicVolume = val;

        var sound = container.GetNode<SliderOption>("Sound");
        sound.ValueChanged += val => GameData.GameOptions.EffectVolume = val;

        var reset = container.GetNode<Button>("ResetButton");
        reset.Pressed += () =>
        {
            GameData.ResetPlayerStats();
            BackToMainMenu();
        };

        void BackToMainMenu()
        {
            GetNode<VBoxContainer>("Container/MainMenu").Show();
            GetNode<VBoxContainer>("Container/OptionsMenu").Hide();
            GetNode<Button>("Container/MainMenu/OptionsButton").GrabFocus();
        }
    }

    private void LaunchScene(PackedScene scene)
    {
        GameData.PlayerStats.LastPlayedLevel = scene.ResourcePath;
        GetTree().ChangeSceneToPacked(scene);
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