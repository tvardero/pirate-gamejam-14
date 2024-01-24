using Godot;
using System;

public partial class MainMenu : Control
{
    [Export] public PackedScene[] LevelsToSelect { get; set; } = Array.Empty<PackedScene>();

    [Export] public string[] LevelNames { get; set; } = Array.Empty<string>();

    public override void _Ready()
    {
        PrepareLevelSelectionMenu(GetNode<VBoxContainer>("Container/LevelSelectMenu"));

        var play = GetNode<Button>("Container/MainMenu/PlayButton");
        play.GrabFocus();
        play.Pressed += LaunchLastPlayed;
        if (GameData.PlayerStats.LastPlayedLevel != null) play.Text = "Continue";

        var select = GetNode<Button>("Container/MainMenu/SelectLevelButton");
        select.Pressed += OpenSelectLevelMenu;

        var options = GetNode<Button>("Container/MainMenu/OptionsButton");
        options.Pressed += OpenOptionsMenu;

        var exit = GetNode<Button>("Container/MainMenu/ExitButton");
        exit.Pressed += () => GetTree().Quit();
    }

    private void PrepareLevelSelectionMenu(Control container)
    {
        var backButton = container.GetNode<Button>("BackToMainMenuButton");
        backButton.Pressed += CloseSelectLevelMenu;
        
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

    private void LaunchLastPlayed()
    {
        var levelPacked = GD.Load<PackedScene>(GameData.PlayerStats.LastPlayedLevel 
                                               ?? "res://scenes/levels/Tutorial001.tscn");
        LaunchScene(levelPacked);
    }

    private void LaunchScene(PackedScene scene)
    {
        GameData.PlayerStats.LastPlayedLevel = scene.ResourcePath;
        GetTree().ChangeSceneToPacked(scene);
    }

    private void OpenSelectLevelMenu()
    {
        GetNode<VBoxContainer>("Container/MainMenu").Hide();
        GetNode<VBoxContainer>("Container/LevelSelectMenu").Show();
    }

    private void CloseSelectLevelMenu()
    {
        GetNode<VBoxContainer>("Container/MainMenu").Show();
        GetNode<VBoxContainer>("Container/LevelSelectMenu").Hide();
    }

    private void OpenOptionsMenu()
    {
        throw new NotImplementedException();
        
        GetNode<VBoxContainer>("Container/MainMenu").Hide();
    }

    private void CloseOptionsMenu()
    {
        throw new NotImplementedException();
        
        GetNode<VBoxContainer>("Container/MainMenu").Show();
    }
}