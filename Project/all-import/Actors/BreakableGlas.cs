using Godot;

public partial class BreakableGlas : Node3D
{
    [Export] public float MinImpactSpeed = 3.0f;
    private bool _destroyed = false;
    private Area3D _sensor;

    public override void _Ready()
    {
        _sensor = FindArea3D(this);
        if (_sensor != null)
            _sensor.BodyEntered += OnBodyEntered;
    }

    private Area3D FindArea3D(Node node)
    {
        for (int i = 0; i < node.GetChildCount(); i++)
        {
            var child = node.GetChild(i);
            if (child is Area3D area)
                return area;
            var found = FindArea3D(child);
            if (found != null)
                return found;
        }
        return null;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (_destroyed) return;
        if (!body.IsInGroup("Pickable")) return;

        if (body is RigidBody3D rb)
        {
            float speed = rb.LinearVelocity.Length();
            if (speed < MinImpactSpeed) return;
        }
        else
        {
            return;
        }

        _destroyed = true;

        for (int i = GetChildCount() - 1; i >= 0; i--)
        {
            GetChild(i).QueueFree();
        }
    }
}
