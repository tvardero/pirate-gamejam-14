using Godot;
using System;

public partial class Well : StaticBody2D, IInteractable
{
    public Area2D InteractivityHitbox { get; private set; } = null!;
    
    public string[] AvailableActionMethods => new string[] { InputActionNames.PrimaryAction };

    public override void _Ready()
    {
        InteractivityHitbox = GetNode<Area2D>("Area2D");
    }

    public void Interact(Node initiator, string interactionMethod)
    {
        GD.Print("Refill bucket");
        throw new NotImplementedException();
    }
}