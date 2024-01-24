using Godot;
using System.Threading.Tasks;

public partial class SplashGameLoader : Control
{
    private TextureRect _splashImage = null!;
    
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
        OpenMenu();
    }
    
    private void OpenMenu()
    {
        var menuPacked = GD.Load<PackedScene>("res://scenes/ui/MainMenu.tscn");
        GetTree().ChangeSceneToPacked(menuPacked);
    }
}
