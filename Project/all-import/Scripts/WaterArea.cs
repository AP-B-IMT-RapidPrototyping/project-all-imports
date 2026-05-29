using Godot;

/// <summary>
/// Beach water trigger:
///   • Sand-castles (Pickable group) thrown in are deleted, mirroring Level1's chimney.
///   • Player entering triggers game over via GameManagerBeach.
/// </summary>
public partial class WaterArea : Area3D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body.IsInGroup("player"))
        {
            var gm = GetTree().Root.FindChild("GameManagerBeach", true, false) as GameManagerBeach;
            gm?.OnPlayerDrowned();
            return;
        }

        if (body.IsInGroup("Pickable"))
        {
            body.QueueFree();
        }
    }
}
