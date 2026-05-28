using Godot;

/// <summary>
/// Pickable sand-castle. Behaves like a StealableObject but is also tracked by
/// GameManagerBeach via the "sand_castle" group, and destroys both itself and
/// another sand-castle on a high-speed impact.
/// </summary>
public partial class SandCastle : StealableObject
{
    [Export] public float MinImpactSpeed = 10.0f;

    private bool _destroyed = false;

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("sand_castle");

        ContactMonitor = true;
        MaxContactsReported = 4;
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (_destroyed) return;
        if (body is not SandCastle other) return;
        if (other._destroyed) return;

        float relSpeed = (LinearVelocity - other.LinearVelocity).Length();
        if (relSpeed < MinImpactSpeed) return;

        _destroyed = true;
        other._destroyed = true;
        other.QueueFree();
        QueueFree();
    }
}
