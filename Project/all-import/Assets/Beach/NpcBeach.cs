using Godot;
using System;

public partial class NpcBeach : CharacterBody3D
{
    public enum State { Walking, PlayerNearby }
    private State _currentState = State.Walking;

    [Export] public Node3D Player;
    [Export] public Sprite3D ExclamationMark;
    [Export] public NavigationAgent3D NavAgent;
    [Export] public Marker3D[] Waypoints;
    [Export] public string WalkAnimationName = "Armature|Walking";
    
    private AnimationPlayer _animPlayer;
    private float _speed = 2.0f;
    private int _currentWaypointIndex = 0;

    public override void _Ready()
    {
    _animPlayer = FindChild("AnimationPlayer") as AnimationPlayer;
    if (ExclamationMark != null) ExclamationMark.Modulate = new Color(1, 1, 1, 0);
    if (_animPlayer != null) _animPlayer.Play(WalkAnimationName);

        GlobalRotation = new Vector3(0, GlobalRotation.Y, 0);
    }

   public override void _PhysicsProcess(double delta)
{
    // Gravity altijd toepassen
    if (!IsOnFloor())
    {
        Velocity += GetGravity() * (float)delta;
    }

    if (Player != null && GlobalPosition.DistanceTo(Player.GlobalPosition) < 10.0f)
    {
        if (_currentState != State.PlayerNearby)
        {
            _currentState = State.PlayerNearby;
            ShowExclamationMark();
        }
    }
    else
    {
        if (_currentState == State.PlayerNearby)
            _currentState = State.Walking;  // ← fix: alleen resetten als het PlayerNearby was
    }

    switch (_currentState)
    {
        case State.Walking:
            WalkPath();  // MoveAndSlide zit al in WalkPath
            break;

        case State.PlayerNearby:
            Velocity = new Vector3(0, Velocity.Y, 0);
            MoveAndSlide();
            break;
    }

    // Rotatie lock (dit is goed)
    Vector3 rot = GlobalRotation;
    rot.X = 0;
    rot.Z = 0;
    GlobalRotation = rot;
}

    private void WalkPath()
    {
        if (Waypoints == null || Waypoints.Length == 0) return;

        Vector3 target = Waypoints[_currentWaypointIndex].GlobalPosition;
        NavAgent.TargetPosition = target;

        if (GlobalPosition.DistanceTo(target) < 1.0f)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % Waypoints.Length;
            return;
        }

        Vector3 nextPoint = NavAgent.GetNextPathPosition();
        Vector3 direction = (nextPoint - GlobalPosition).Normalized();
        direction.Y = 0;
        Velocity = new Vector3(direction.X * _speed, Velocity.Y, direction.Z * _speed);

        if (direction != Vector3.Zero && direction.Length() > 0.1f)
        {
            Vector3 lookTarget = new Vector3(nextPoint.X, GlobalPosition.Y, nextPoint.Z);
            if (GlobalPosition.DistanceTo(lookTarget) > 0.01f)
            {
                LookAt(lookTarget, Vector3.Up);
            }
        }

        MoveAndSlide();
    }

    private async void ShowExclamationMark()
    {
        if (ExclamationMark == null || ExclamationMark.Modulate.A > 0) return;
        ExclamationMark.Modulate = new Color(1, 1, 1, 1);
        await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
        ExclamationMark.Modulate = new Color(1, 1, 1, 0);
    }
}



// using Godot;
// using System;

// public partial class NpcBeach : CharacterBody3D
// {
//     public enum State { Walking, PlayerNearby }
//     private State _currentState = State.Walking;

//     [Export] public Node3D Player;
//     [Export] public Sprite3D ExclamationMark;
//     [Export] public NavigationAgent3D NavAgent;
//     [Export] public Marker3D[] Waypoints;
//     [Export] public string WalkAnimationName = "Armature|Walking";
    
//     private AnimationPlayer _animPlayer;
//     private float _speed = 2.0f;
//     private int _currentWaypointIndex = 0;

//     public override void _Ready()
//     {
//         _animPlayer = GetNode<AnimationPlayer>("Beach-NPC/AnimationPlayer");
//         if (ExclamationMark != null) ExclamationMark.Modulate = new Color(1, 1, 1, 0);
//         _animPlayer.Play(WalkAnimationName);
//     }
// public override void _PhysicsProcess(double delta)


// {
//     // Zwaartekracht
//     if (!IsOnFloor())
//     {
//         Velocity += GetGravity() * (float)delta;
//     }

//     if (Player != null && GlobalPosition.DistanceTo(Player.GlobalPosition) < 10.0f)
//     {
//         if (_currentState != State.PlayerNearby)
//         {
//             _currentState = State.PlayerNearby;
//             ShowExclamationMark();
//         }
//     }
//     else
//     {
//         _currentState = State.Walking;
//     }

//     switch (_currentState)
//     {
//         case State.Walking:
//             WalkPath();
//             break;

//         case State.PlayerNearby:
//             Velocity = new Vector3(0, Velocity.Y, 0); // Y behouden voor zwaartekracht
//             MoveAndSlide();
//             break;
//     }

//     // Alleen Y rotatie vastzetten
//     Vector3 rot = GlobalRotation;
//     rot.X = 0;
//     rot.Z = 0;
//     GlobalRotation = rot;
// }
    // public override void _PhysicsProcess(double delta)
    // {
    //     if (!IsOnFloor())
    //     {
    //         Velocity += GetGravity() * (float)delta;
    //     }
    //     if (Player != null && GlobalPosition.DistanceTo(Player.GlobalPosition) < 10.0f)
    //     {
    //         if (_currentState != State.PlayerNearby)
    //         {
    //             _currentState = State.PlayerNearby;
    //             ShowExclamationMark();
    //         }
    //     }
    //     else
    //     {
    //         _currentState = State.Walking;
    //     }

    //     switch (_currentState)
    //     {
    //         case State.Walking:
    //             WalkPath();
    //             break;

    //         case State.PlayerNearby:
    //             Velocity = Vector3.Zero;
    //             MoveAndSlide();
    //             break;
    //     }

    //     GlobalRotation = new Vector3(GlobalRotation.X, GlobalRotation.Y, 0);
    // }

//     private void WalkPath()
//     {
//         if (Waypoints == null || Waypoints.Length == 0) return;

//         Vector3 target = Waypoints[_currentWaypointIndex].GlobalPosition;
//         NavAgent.TargetPosition = target;

//         if (GlobalPosition.DistanceTo(target) < 1.0f)
//         {
//             _currentWaypointIndex = (_currentWaypointIndex + 1) % Waypoints.Length;
//             return;
//         }

//         Vector3 nextPoint = NavAgent.GetNextPathPosition();
//         Vector3 direction = (nextPoint - GlobalPosition).Normalized();
//         direction.Y = 0;
//         Velocity = direction * _speed;

//     Velocity = new Vector3(direction.X * _speed, Velocity.Y, direction.Z * _speed);

//         if (direction != Vector3.Zero)
//         {
//             LookAt(new Vector3(nextPoint.X, GlobalPosition.Y, nextPoint.Z), Vector3.Up);
//         }

//         MoveAndSlide();
//     }

//     private async void ShowExclamationMark()
//     {
//         if (ExclamationMark == null || ExclamationMark.Modulate.A > 0) return;
//         ExclamationMark.Modulate = new Color(1, 1, 1, 1);
//         await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
//         ExclamationMark.Modulate = new Color(1, 1, 1, 0);
//     }
// }


// using Godot;
// using System;

// public partial class NpcBeach : CharacterBody3D
// {
//     public enum State { WalkingToSea, WalkingBack, RunningAway }
//     private State _currentState = State.WalkingToSea;
    
//     [Export] public Node3D Player;
//     [Export] public Sprite3D ExclamationMark;
//     [Export] public Marker3D SeaMarker;
//     [Export] public Marker3D StartMarker;
//     [Export] public NavigationAgent3D NavAgent;
//     private AnimationTree _animTree;
//     private float _speed = 2.0f;

//     public override void _Ready()
//     {
//         _animTree = GetNode<AnimationTree>("AnimationTree");
//         if (ExclamationMark != null) ExclamationMark.Modulate = new Color(1, 1, 1, 0);
//     }

//     public override void _PhysicsProcess(double delta)
//     {
//         // 1. Detectie: Vogel in de buurt?
//         if (Player != null && GlobalPosition.DistanceTo(Player.GlobalPosition) < 5.0f)
//         {
//             _currentState = State.RunningAway;
//             ShowExclamationMark();
//         }

//         // 2. Logica
//         switch (_currentState)
//         {
//             case State.WalkingToSea:
//                 MoveTowards(SeaMarker.GlobalPosition);
//                 if (GlobalPosition.DistanceTo(SeaMarker.GlobalPosition) < 1.0f) _currentState = State.WalkingBack;
//                 break;

//             case State.WalkingBack:
//                 MoveTowards(StartMarker.GlobalPosition);
//                 if (GlobalPosition.DistanceTo(StartMarker.GlobalPosition) < 1.0f) _currentState = State.WalkingToSea;
//                 break;

//             case State.RunningAway:
//                 Vector3 runDir = (GlobalPosition - Player.GlobalPosition).Normalized();
//                 runDir.Y = 0;
//                 Velocity = runDir * 5.0f;
//                 _animTree.Set("parameters/playback", "Run");
//                 MoveAndSlide();
//                 break;
//         }

//         // DIT HOUDT HEM RECHTOP:
//         GlobalRotation = new Vector3(0, GlobalRotation.Y, 0);
//     }

//     private void MoveTowards(Vector3 target)
//     {
//     // Vertel de agent waar hij heen moet
//         NavAgent.TargetPosition = target;
        
//         // Bereken het volgende punt op de navigatie-mesh
//         Vector3 nextPathPoint = NavAgent.GetNextPathPosition();
//         Vector3 direction = (nextPathPoint - GlobalPosition).Normalized();
        
//         // Zorg dat hij alleen horizontaal beweegt (Y=0)
//         direction.Y = 0; 
//         Velocity = direction * _speed;
        
//         // Draai naar het doel
//         if (direction != Vector3.Zero)
//         {
//             LookAt(new Vector3(nextPathPoint.X, GlobalPosition.Y, nextPathPoint.Z), Vector3.Up);
//         }
                
//         _animTree.Set("parameters/playback", "Walk");
//         MoveAndSlide();
//     }

//     private async void ShowExclamationMark()
//     {
//         if (ExclamationMark == null || ExclamationMark.Modulate.A > 0) return;
//         ExclamationMark.Modulate = new Color(1, 1, 1, 1);
//         await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
//         ExclamationMark.Modulate = new Color(1, 1, 1, 0);
//     }
// }

// // public partial class NpcBeach : CharacterBody3D
// // {
	
	
// //     public enum State { WalkingToSea, WalkingBack, RunningAway }
// //     private State _currentState = State.WalkingToSea;
// //     private float _timer = 0.0f;
// //     private float _speed = 2.0f;

// //     [Export] public Node3D Player; // Sleep je speler hiernaartoe in de Inspector
// //     [Export] public Sprite3D ExclamationMark; // Sleep je Sprite3D hiernaartoe

// //     private AnimationTree _animTree;

// //     public override void _Ready()
// //     {
// // _animTree = GetNode<AnimationTree>("AnimationTree");
// //     ExclamationMark.Modulate = new Color(1, 1, 1, 0);
    
// //     if (Player == null) 
// //     {
// //         GD.PrintErr("FOUT: De speler is niet gekoppeld in de Inspector!");
// //     }
// //     }

// //     public override void _PhysicsProcess(double delta)
// //     {
// //         // 1. Detectie: afstand tot de speler (Bird)
// //         if (Player != null && GlobalPosition.DistanceTo(Player.GlobalPosition) < 50.0f)
// //         {
// //             _currentState = State.RunningAway;
// //             ShowExclamationMark();
// //         }

// //         // 2. State Machine
// //         switch (_currentState)
// //         {
// //             case State.WalkingToSea:
// //                 Velocity = new Vector3(0, 0, 1) * _speed;
// //                 _animTree.Set("parameters/playback", "Walk");
// //                 _timer += (float)delta;
// //                 if (_timer > 5.0f) { _currentState = State.WalkingBack; _timer = 0; }
// //                 break;

// //             case State.WalkingBack:
// //                 Velocity = new Vector3(0, 0, -1) * _speed;
// //                 _animTree.Set("parameters/playback", "Walk");
// //                 _timer += (float)delta;
// //                 if (_timer > 5.0f) { _currentState = State.WalkingToSea; _timer = 0; }
// //                 break;

// //             case State.RunningAway:
// //                 Vector3 runDir = (GlobalPosition - Player.GlobalPosition).Normalized();
// //                 Velocity = runDir * 5.0f; // Sneller
// //                 _animTree.Set("parameters/playback", "Run");
// //                 break;
// //         }

// //         MoveAndSlide();
// //     }

// //     private async void ShowExclamationMark()
// //     {
// //         ExclamationMark.Modulate = new Color(1, 1, 1, 1); // Zichtbaar
// //         await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
// //         ExclamationMark.Modulate = new Color(1, 1, 1, 0); // Weer weg
// //     }
// // }