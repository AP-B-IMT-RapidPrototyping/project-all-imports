using Godot;

public partial class Guard : CharacterBody3D
{
    private NavigationAgent3D _navAgent;
    private AnimationPlayer _animPlayer;
    private float _speed = 3.5f;

    // Exporteer zodat je waypoints kan instellen in de Inspector
    [Export] public NodePath[] WaypointPaths = new NodePath[0];

    private Node3D[] _waypoints;
    private int _currentWaypoint = 0;
    private bool _targetSet = false;

    public override void _Ready()
    {
        _navAgent = GetNodeOrNull<NavigationAgent3D>("NavigationAgent3D");
        _animPlayer = FindChild("Walk") as AnimationPlayer;

        // Laad waypoints
        _waypoints = new Node3D[WaypointPaths.Length];
        for (int i = 0; i < WaypointPaths.Length; i++)
            _waypoints[i] = GetNode<Node3D>(WaypointPaths[i]);

        GD.Print("Waypoints geladen: " + _waypoints.Length);

        Callable.From(ActorSetup).CallDeferred();
    }

    private async void ActorSetup()
    {
        for (int i = 0; i < 10; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        SetNextWaypoint();
    }

    private void SetNextWaypoint()
    {
        if (_waypoints.Length == 0)
        {
            // Geen waypoints ingesteld — gebruik test positie
            _navAgent.TargetPosition = GlobalPosition + new Vector3(10, 0, 0);
        }
        else
        {
            _navAgent.TargetPosition = _waypoints[_currentWaypoint].GlobalPosition;
            GD.Print("Naar waypoint: " + _currentWaypoint);
        }
        _targetSet = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_navAgent == null || _animPlayer == null) return;
        if (!_targetSet) return;

        // Waypoint bereikt → ga naar volgende
        if (GlobalPosition.DistanceTo(_navAgent.TargetPosition) < 1.0f)
        {
            if (_waypoints.Length > 0)
            {
                // Loop door waypoints
                _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Length;
                SetNextWaypoint();
            }
            else
            {
                _animPlayer.Stop();
                return;
            }
        }

        Vector3 nextPos = _navAgent.GetNextPathPosition();
        Vector3 direction = (nextPos - GlobalPosition).Normalized();

        Velocity = direction * _speed;
        MoveAndSlide();

        if (Velocity.Length() > 0.1f)
        {
            if (!_animPlayer.IsPlaying())
                _animPlayer.Play("mixamo_com");

            if (direction.Length() > 0.001f)
                LookAt(GlobalPosition + direction, Vector3.Up);
        }
    }
}