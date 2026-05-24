using Godot;

/// <summary>
/// Manages the tutoring mini-game:
///   • Progress bar updates distance from Streetlight9 to MissionArea.
///   • MissionComplete visible when MissionArea is reached.
/// </summary>
public partial class GameManagerTutorial : Node
{
    private Label _statusLabel;
    private Label _hintLabel;
    private Label _firstObjective;
    private Label _secondObjective;
    private Label _missionComplete;
    private Label _gameOverLabel;
    private ProgressBar _objectiveProgress;

    private Node3D _player;
    private Area3D _missionArea;
    private Node3D _streetlight;

    private float _startZ;
    private float _endZ;
    private float _totalDistance;

    private bool _gameOver = false;

    public override void _Ready()
    {
        _statusLabel = GetTree().Root.FindChild("StatusLabel", true, false) as Label;
        _hintLabel = GetTree().Root.FindChild("HintLabel", true, false) as Label;
        _firstObjective = GetTree().Root.FindChild("FirstObjective", true, false) as Label;
        _secondObjective = GetTree().Root.FindChild("SecondObjective", true, false) as Label;
        _missionComplete = GetTree().Root.FindChild("MissionComplete", true, false) as Label;
        _gameOverLabel = GetTree().Root.FindChild("GameOver", true, false) as Label;

        _objectiveProgress = GetTree().Root.FindChild("FirstObjectiveProgress", true, false) as ProgressBar;

        _player = GetTree().GetFirstNodeInGroup("player") as Node3D;

        Node missionAreaNode = GetTree().Root.FindChild("MissionArea", true, false);
        if (missionAreaNode == null)
        {
            // fallback to finding the area named Area3D if MissionArea hasn't been explicitly named yet
            missionAreaNode = GetTree().Root.FindChild("Area3D", true, false);
        }

        if (missionAreaNode is Area3D area)
        {
            _missionArea = area;
        }

        _streetlight = GetTree().Root.FindChild("Streetlight9", true, false) as Node3D;

        if (_missionComplete != null) _missionComplete.Visible = false;
        if (_gameOverLabel != null) _gameOverLabel.Visible = false;

        if (_objectiveProgress != null)
        {
            _objectiveProgress.MinValue = 0;
            _objectiveProgress.MaxValue = 100;
            _objectiveProgress.Value = 0;
        }

        if (_missionArea != null && _streetlight != null)
        {
            _startZ = _streetlight.GlobalPosition.Z;
            _endZ = _missionArea.GlobalPosition.Z;
            _totalDistance = Mathf.Abs(_endZ - _startZ);

            if (!_missionArea.IsConnected(Area3D.SignalName.BodyEntered, Callable.From<Node3D>(OnMissionAreaEntered)))
            {
                _missionArea.BodyEntered += OnMissionAreaEntered;
            }
        }
        else
        {
            GD.PrintErr("[GameManagerTutorial] Could not configure progress because MissionArea or Streetlight9 was null.");
        }

        ConnectAllNpcs(GetParent() ?? this);
    }

    private void OnMissionAreaEntered(Node3D body)
    {
        Node current = body;
        while (current != null)
        {
            if (current.IsInGroup("player"))
            {
                CompleteMission();
                return;
            }
            current = current.GetParent();
        }
    }

    private async void CompleteMission()
    {
        if (_gameOver) return;
        _gameOver = true;

        GD.Print("[GameManagerTutorial] Mission Area Reached!");

        if (_missionComplete != null)
            _missionComplete.Visible = true;

        if (_firstObjective != null)
            _firstObjective.Visible = false;

        if (_objectiveProgress != null)
            _objectiveProgress.Value = 100;

        NeutraliseAllNpcs();

        await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
        GetTree().ChangeSceneToFile("res://Levels/Level1.tscn");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
        }

        if (Input.IsKeyPressed(Key.R))
            Restart();

        if (!_gameOver && _player != null && _totalDistance > 0)
        {
            float currentDist = Mathf.Abs(_endZ - _player.GlobalPosition.Z);
            float progress = 100f * (1.0f - (currentDist / _totalDistance));
            if (_objectiveProgress != null)
            {
                _objectiveProgress.Value = Mathf.Clamp(progress, 0, 100);
            }
        }
    }

    private void NeutraliseAllNpcs()
    {
        var root = GetParent() ?? this;
        NeutraliseNpcsRecursive(root);
    }

    private void NeutraliseNpcsRecursive(Node node)
    {
        if (node == null) return;
        foreach (Node child in node.GetChildren())
        {
            if (child is Npc npc)
            {
                npc.SetStateExternal(Npc.NpcState.Neutral);
            }
            NeutraliseNpcsRecursive(child);
        }
    }

    private void ConnectAllNpcs(Node node)
    {
        if (node == null) return;
        foreach (Node child in node.GetChildren())
        {
            if (child is Npc npc)
                npc.CaughtPlayer += OnPlayerCaught;
            ConnectAllNpcs(child);
        }
    }

    public void OnPlayerCaught()
    {
        if (_gameOver) return;
        _gameOver = true;

        if (_gameOverLabel != null)
            _gameOverLabel.Visible = true;

        SetHint("Press R to try again");
        GD.Print("[GameManagerTutorial] Player was caught!");

        var player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
        if (player != null)
            player.SetPhysicsProcess(false);
    }

    private void SetHint(string text)
    {
        if (_hintLabel != null)
            _hintLabel.Text = text;
    }

    private void Restart()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        GetTree().ReloadCurrentScene();
    }
}
