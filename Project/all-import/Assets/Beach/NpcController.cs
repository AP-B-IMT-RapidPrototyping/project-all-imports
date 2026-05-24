using Godot;
using System;

public partial class NpcController : CharacterBody3D
{
	
	
    public enum State { WalkingToSea, WalkingBack, RunningAway }
    private State _currentState = State.WalkingToSea;
    private float _timer = 0.0f;
    private float _speed = 2.0f;

    [Export] public Node3D Player; // Sleep je speler hiernaartoe in de Inspector
    [Export] public Sprite3D ExclamationMark; // Sleep je Sprite3D hiernaartoe

    private AnimationTree _animTree;

    public override void _Ready()
    {
        _animTree = GetNode<AnimationTree>("AnimationTree");
        ExclamationMark.Modulate = new Color(1, 1, 1, 0); // Onzichtbaar maken
    }

    public override void _PhysicsProcess(double delta)
    {
        // 1. Detectie: afstand tot de speler (Bird)
        if (Player != null && GlobalPosition.DistanceTo(Player.GlobalPosition) < 5.0f)
        {
            _currentState = State.RunningAway;
            ShowExclamationMark();
        }

        // 2. State Machine
        switch (_currentState)
        {
            case State.WalkingToSea:
                Velocity = new Vector3(0, 0, 1) * _speed;
                _animTree.Set("parameters/playback", "Walk");
                _timer += (float)delta;
                if (_timer > 5.0f) { _currentState = State.WalkingBack; _timer = 0; }
                break;

            case State.WalkingBack:
                Velocity = new Vector3(0, 0, -1) * _speed;
                _animTree.Set("parameters/playback", "Walk");
                _timer += (float)delta;
                if (_timer > 5.0f) { _currentState = State.WalkingToSea; _timer = 0; }
                break;

            case State.RunningAway:
                Vector3 runDir = (GlobalPosition - Player.GlobalPosition).Normalized();
                Velocity = runDir * 5.0f; // Sneller
                _animTree.Set("parameters/playback", "Run");
                break;
        }

        MoveAndSlide();
    }

    private async void ShowExclamationMark()
    {
        ExclamationMark.Modulate = new Color(1, 1, 1, 1); // Zichtbaar
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
        ExclamationMark.Modulate = new Color(1, 1, 1, 0); // Weer weg
    }
}