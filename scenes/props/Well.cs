using Godot;
using System;
using Gamejam.code;

public partial class Well : StaticBody2D, IInteractable
{
    public Area2D InteractivityHitbox { get; private set; } = null!;
    
    public override void _Ready()
    {
        InteractivityHitbox = GetNode<Area2D>("Area2D");
    }

    public void Interact(Node initiator)
    {
        if (GameData.LevelData != null) GameData.LevelData.BucketAmmo = LevelData.MaxBucketAmmo;
    }
}