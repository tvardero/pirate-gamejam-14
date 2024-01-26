using System.Linq;
using Godot;

public partial class Game : Node
{
    private static int _counter = 0;
    private PauseMenu _pauseMenu = null!;

    public override void _Ready()
    {
        _pauseMenu = GetNode<PauseMenu>("UI/PauseMenu");
        _pauseMenu.Closed += () => TogglePauseMenu(false);
    }

    public override void _UnhandledInput(InputEvent input)
    {
        if (!input.IsActionPressed("ui_cancel")) return;

        TogglePauseMenu(!_pauseMenu.Visible);
        GetTree().Root.SetInputAsHandled();
    }

    public void ReplaceLevelWith(Node2D newLevel)
    {
        GetCurrentLevel()?.QueueFree();
        newLevel.Name = "Level" + _counter++;
        AddChild(newLevel);
        MoveChild(newLevel, 0);
    }

    private void TogglePauseMenu(bool paused)
    {
        GetTree().Paused = paused;

        var gameUi = GetNode<Control>("UI/GameUi");
        gameUi.Visible = !paused;

        GetCurrentLevel()?.SetProcessUnhandledInput(!paused);

        var pauseMenu = GetNode<PauseMenu>("UI/PauseMenu");
        pauseMenu.Visible = paused;
        pauseMenu.SetProcessUnhandledInput(paused);
        if (paused) pauseMenu.GrabFocusOnOpen();
    }

    private Node? GetCurrentLevel()
    {
        return GetChildren()
            .OfType<Node2D>()
            .FirstOrDefault(n => n.Name.ToString().StartsWith("Level"));
    }
}