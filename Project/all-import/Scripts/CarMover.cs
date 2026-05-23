using Godot;
using System;

public partial class CarMover : Area3D
{
    public Vector3 StartPoint;
    public Vector3 EndPoint;
    public float Speed = 10.0f;

    private float _distanceToTarget;
    private float _distanceTraveled;

    public override void _Ready()
    {
        GlobalPosition = StartPoint;

        // Add a CollisionShape3D for the car
        CollisionShape3D col = new CollisionShape3D();
        BoxShape3D shape = new BoxShape3D();
        shape.Size = new Vector3(4.0f, 4.0f, 6.0f); // approx car bounds
        col.Shape = shape;
        col.Position = new Vector3(0, 1.5f, 0);
        AddChild(col);

        // Detect player
        BodyEntered += OnBodyEntered;

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

        if (_distanceTraveled >= _distanceToTarget)
        {
            QueueFree();
            return;
        }

        GlobalPosition = GlobalPosition.MoveToward(EndPoint, moveDistance);
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body.IsInGroup("player"))
        {
            GD.Print("Car hit the player!");

            // Call OnPlayerCaught on the appropriate GameManager
            // Try Level 1
            var gmLevel1 = GetTree().Root.FindChild("GameManager", true, false) as GameManagerLevel1;
            if (gmLevel1 == null)
                gmLevel1 = GetTree().Root.FindChild("GameManagerTestlvl", true, false) as GameManagerLevel1;

            if (gmLevel1 != null)
            {
                gmLevel1.OnPlayerCaught();
                return;
            }

            // Try Tutorial Level
            var gmTut = GetTree().Root.FindChild("GameManagerTestlvl", true, false) as GameManagerTutorial;
            if (gmTut != null)
            {
                gmTut.OnPlayerCaught();
                return;
            }
        }
    }
}
