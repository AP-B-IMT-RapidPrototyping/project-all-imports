using Godot;
using System;

public partial class CarSpawner : Node3D
{
    [Export] public PackedScene CarPrefab;
    [Export] public float MinSpawnTime = 2.0f;
    [Export] public float MaxSpawnTime = 5.0f;
    [Export] public float CarSpeed = 10.0f;

    private Node3D _startPoint;
    private Node3D _endPoint;
    private Timer _spawnTimer;

    public override void _Ready()
    {
        _startPoint = GetNodeOrNull<Node3D>("StartPoint");
        _endPoint = GetNodeOrNull<Node3D>("EndPoint");

        if (_startPoint == null || _endPoint == null)
        {
            GD.PrintErr("CarSpawner needs StartPoint and EndPoint child nodes!");
            return;
        }

        _spawnTimer = new Timer();
        AddChild(_spawnTimer);
        _spawnTimer.Timeout += OnSpawnTimerTimeout;

        StartNextTimer();
    }

    private void StartNextTimer()
    {
        double nextWaitTime = GD.RandRange(MinSpawnTime, MaxSpawnTime);
        _spawnTimer.WaitTime = nextWaitTime;
        _spawnTimer.Start();
    }

    private void OnSpawnTimerTimeout()
    {
        SpawnCar();
        StartNextTimer();
    }

    private void SpawnCar()
    {
        if (CarPrefab == null)
        {
            GD.PrintErr("CarSpawner: No CarPrefab assigned!");
            return;
        }

        // Setup the mover element
        CarMover carMover = new CarMover();
        carMover.StartPoint = _startPoint.GlobalPosition;
        carMover.EndPoint = _endPoint.GlobalPosition;
        carMover.Speed = CarSpeed;

        // Instantiate visual element and attach
        Node3D carVisual = CarPrefab.Instantiate<Node3D>();
        carMover.AddChild(carVisual);

        // Add to main scene
        AddChild(carMover);
    }
}
