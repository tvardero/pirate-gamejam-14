using Godot;
using System;

public partial class OptionsMenu : VBoxContainer
{
    [Export]
    public bool ShowReset { get; set; }
    
    public event Action? Closed;
    
    public override void _Ready()
    {
        var backButton = GetNode<Button>("BackToMainMenuButton");
        backButton.Pressed += () => Closed?.Invoke();

        var music = GetNode<SliderOption>("Music");
        music.CurrentValue = GameData.GameOptions.MusicVolume;
        music.ValueChanged += val => GameData.GameOptions.MusicVolume = val;

        var sound = GetNode<SliderOption>("Sound");
        sound.CurrentValue = GameData.GameOptions.EffectVolume;
        sound.ValueChanged += val => GameData.GameOptions.EffectVolume = val;

        var showFps = GetNode<CheckButton>("ShowFps/HBoxContainer/CheckButton");
        showFps.ButtonPressed = GameData.GameOptions.ShowFps;
        showFps.Toggled += (state) => GameData.GameOptions.ShowFps = state;
        
        var reset = GetNode<Button>("ResetButton");
        reset.Visible = ShowReset;
        reset.Pressed += () =>
        {
            GameData.ResetPlayerStats();
            Closed?.Invoke();
        };
    }
}