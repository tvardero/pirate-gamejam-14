using System;
using Godot;

public abstract partial class LevelBase : Node2D
{
    private static readonly PackedScene _playerPacked = GD.Load<PackedScene>("res://scenes/entities/Player.tscn");

    [Export] private bool SpawnPlayer { get; set; }

    [Export] private int SpawnPointIndex { get; set; }

    public override void _Ready()
    {
        if (SpawnPlayer) SpawnPlayerAt(SpawnPointIndex);
    }

    public void SpawnPlayerAt(int playerSpawnPointIndex, float playerRotation = 0)
    {
        var spawnPoint = GetNodeOrNull<Node2D>($"PlayerSpawn_{playerSpawnPointIndex}");
        if (spawnPoint == null)
            throw new ArgumentException($"Player spawn point #{playerSpawnPointIndex} was not found",
                nameof(playerSpawnPointIndex));

        var playerInstance = _playerPacked.Instantiate<Player>();
        playerInstance.Position = spawnPoint.Position;
        playerInstance.Rotation = playerRotation;
        playerInstance.Name = "Player";

        AddChild(playerInstance);
    }

    public void TransitToScene(LevelBase nextScene, int playerSpawnPointIndex)
    {
        var player = GetNodeOrNull<Player>("Player");
        var rotation = player?.Rotation ?? 0;
        nextScene.SpawnPlayerAt(playerSpawnPointIndex, rotation);
    }
}