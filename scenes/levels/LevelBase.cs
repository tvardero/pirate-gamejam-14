using Godot;

public abstract partial class LevelBase : Node2D
{
    private static readonly PackedScene _playerPacked = GD.Load<PackedScene>("res://scenes/entities/Player.tscn");

    public override void _Ready()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer(float playerRotation = 0)
    {
        var spawnPoint = GetNodeOrNull<Node2D>("SpawnPoint");

        var playerInstance = _playerPacked.Instantiate<Player>();
        playerInstance.Position = spawnPoint.Position;
        playerInstance.Rotation = playerRotation;
        playerInstance.Name = "Player";

        AddChild(playerInstance);
    }
}