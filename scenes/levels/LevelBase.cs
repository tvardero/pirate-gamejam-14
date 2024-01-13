using System;
using Godot;

public abstract partial class LevelBase : Node2D
{
    [Export]
    private PackedScene PlayerScene { get; set; } = null!;
    
    [Export]
    private bool SpawnPlayerAtGameStart { get; set; }
    
    [Export]
    private int SpawnPointAtGameStart { get; set; }
    
    [Export]
    private float PlayerRotationAtGameStartDegrees { get; set; }

    public override void _Ready()
    {
        ArgumentNullException.ThrowIfNull(PlayerScene); // <-- dont forget to set player scene in the editor
        
        if (SpawnPlayerAtGameStart) SpawnPlayerAt(SpawnPointAtGameStart, MathF.PI * PlayerRotationAtGameStartDegrees / 180);
    }

    protected void SpawnPlayerAt(int playerSpawnPointIndex, float playerRotation)
    {
        var spawnPoint = GetNodeOrNull<Node2D>($"PlayerSpawn_{playerSpawnPointIndex}");
        if (spawnPoint == null) throw new ArgumentException($"Player spawn point #{playerSpawnPointIndex} was not found", nameof(playerSpawnPointIndex));

        var playerInstance = PlayerScene.Instantiate<Player>();
        playerInstance.Position = spawnPoint.Position;
        playerInstance.Rotation = playerRotation;
        
        AddChild(playerInstance);
    }

    protected void TransitionToScene(LevelBase nextScene, int playerSpawnPointIndex)
    {
        var playerRotation = PlayerData.PlayerInstance?.Rotation ?? 0;
        nextScene.SpawnPlayerAt(playerSpawnPointIndex, playerRotation);
    }
}
