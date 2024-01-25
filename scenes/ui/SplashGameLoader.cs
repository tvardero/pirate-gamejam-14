using Godot;
using System.Threading.Tasks;

public partial class SplashGameLoader : Control
{
    private TextureRect _splashImage = null!;

    [Export]
    public PackedScene SceneToOpenOnLoad { get; set; } = GD.Load<PackedScene>("res://scenes/ui/MainMenu.tscn");
    
    public override void _Ready()
    {
        _splashImage = GetNode<TextureRect>("TextureRect");
        _ = LoadData();
    }

    public override void _Process(double delta)
    {
        _splashImage.Rotation += (float)delta * Mathf.Pi / 4;
    }

    private async Task LoadData()
    {
        await GameData.LoadAsync();
        GetTree().ChangeSceneToPacked(SceneToOpenOnLoad);
    }
}
