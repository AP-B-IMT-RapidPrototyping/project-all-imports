using Godot;
using System;

public partial class CarMover : Node3D
{
    public Vector3 StartPoint;
    public Vector3 EndPoint;
    public float Speed = 10.0f;

    private float _distanceToTarget;
    private float _distanceTraveled;

    public override void _Ready()
    {
        GlobalPosition = StartPoint;

        // Face the endpoint
        if (StartPoint.DistanceTo(EndPoint) > 0.01f)
        {
            LookAt(EndPoint, Vector3.Up);
        }

        _distanceToTarget = StartPoint.DistanceTo(EndPoint);
        _distanceTraveled = 0;
    }

    public override void _Process(double delta)
    {
        float moveDistance = Speed * (float)delta;
        _distanceTraveled += moveDistance;

        // Despawn when target is reached
        if (_distanceTraveled >= _distanceToTarget)
        {
            QueueFree();
            return;
        }

        GlobalPosition = GlobalPosition.MoveToward(EndPoint, moveDistance);
    }
}
