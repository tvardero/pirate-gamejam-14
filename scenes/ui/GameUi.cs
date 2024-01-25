using Godot;

public partial class GameUi : Control
{
    private Label _fps = null!;

    public override void _Ready()
    {
        _fps = GetNode<Label>("FPS");
        _fps.Visible = GameData.GameOptions.ShowFps;
        GameData.GameOptions.Updated += () => _fps.Visible = GameData.GameOptions.ShowFps;
    }

    public override void _Process(double delta)
    {
        if (GameData.GameOptions.ShowFps) _fps.Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }
}