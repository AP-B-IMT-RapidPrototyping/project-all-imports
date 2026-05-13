using Godot;

/// <summary>
/// Manages the obstacle-course mini-game:
///   • Objectives via checkpoints (first pass → FirstObjective disappears,
///     second pass → SecondObjective disappears)
///   • Both objectives done → MissionComplete becomes visible, NPCs go Neutral
///   • NPC catch → Game Over
///   • Finish → You Win
///   • R key restarts the scene
/// </summary>
public partial class GameManager : Node
{
    private Label _statusLabel;
    private Label _hintLabel;
    private Label _firstObjective;
    private Label _secondObjective;
    private Label _missionComplete;
    private Label _gameOverLabel;

    // ── Game state ────────────────────────────────────────────────
    private bool _gameOver    = false;
    private bool _gameStarted = false;
    private bool _firstDone   = false;
    private bool _secondDone  = false;

    public override void _Ready()
    {
        _statusLabel     = GetTree().Root.FindChild("StatusLabel",     true, false) as Label;
        _hintLabel       = GetTree().Root.FindChild("HintLabel",       true, false) as Label;
        _firstObjective  = GetTree().Root.FindChild("FirstObjective",  true, false) as Label;
        _secondObjective = GetTree().Root.FindChild("SecondObjective", true, false) as Label;
        _missionComplete = GetTree().Root.FindChild("MissionComplete", true, false) as Label;
        _gameOverLabel   = GetTree().Root.FindChild("GameOver",        true, false) as Label;

        // Debug: confirm every label was found
        GD.Print($"[GameManager] StatusLabel:     {(_statusLabel     != null ? "OK" : "NOT FOUND")}");
        GD.Print($"[GameManager] HintLabel:       {(_hintLabel       != null ? "OK" : "NOT FOUND")}");
        GD.Print($"[GameManager] FirstObjective:  {(_firstObjective  != null ? "OK" : "NOT FOUND")}");
        GD.Print($"[GameManager] SecondObjective: {(_secondObjective != null ? "OK" : "NOT FOUND")}");
        GD.Print($"[GameManager] MissionComplete: {(_missionComplete != null ? "OK" : "NOT FOUND")}");
        GD.Print($"[GameManager] GameOver:        {(_gameOverLabel   != null ? "OK" : "NOT FOUND")}");

        // Both objective labels start visible
        if (_firstObjective  != null) _firstObjective.Visible  = true;
        if (_secondObjective != null) _secondObjective.Visible = true;

        // These start hidden
        if (_missionComplete != null) _missionComplete.Visible = false;
        if (_gameOverLabel   != null) _gameOverLabel.Visible   = false;

        ConnectAllNpcs(GetParent() ?? this);
        ConnectCheckpoints();
    }

    // ── CHECKPOINT SYSTEM ─────────────────────────────────────────
    private void ConnectCheckpoints()
    {
        var checkpoints = GetTree().GetNodesInGroup("checkpoint");
        GD.Print($"[GameManager] Found {checkpoints.Count} checkpoints");

        foreach (Node node in checkpoints)
        {
            Area3D area = null;

            if (node is Area3D a)
            {
                area = a;
            }
            else
            {
                Node parent = node;
                while (parent != null && parent is not Area3D)
                    parent = parent.GetParent();

                if (parent is Area3D foundArea)
                    area = foundArea;
            }

            if (area != null)
            {
                if (!area.IsConnected(Area3D.SignalName.BodyEntered,
                        Callable.From<Node3D>(OnCheckpointBodyEntered)))
                {
                    area.BodyEntered += OnCheckpointBodyEntered;
                }

                GD.Print($"[GameManager] Connected checkpoint '{area.Name}'");
            }
            else
            {
                GD.Print($"[GameManager] '{node.Name}' has no Area3D parent — skipped.");
            }
        }

        _gameStarted = true;
    }

    private void OnCheckpointBodyEntered(Node3D body)
    {
        Node current = body;
        while (current != null)
        {
            if (current.IsInGroup("player"))
            {
                RegisterCheckpointHit();
                return;
            }
            current = current.GetParent();
        }
    }

    private void RegisterCheckpointHit()
    {
        if (!_gameStarted) return;

        // Complete FirstObjective if not yet done
        if (!_firstDone)
        {
            _firstDone = true;

            if (_firstObjective != null)
                _firstObjective.Visible = false;

            GD.Print("[GameManager] FirstObjective completed.");
            CheckMissionComplete();
            return;
        }

        // Complete SecondObjective if first is done but second isn't
        if (_firstDone && !_secondDone)
        {
            _secondDone = true;

            if (_secondObjective != null)
                _secondObjective.Visible = false;

            GD.Print("[GameManager] SecondObjective completed.");
            CheckMissionComplete();
            return;
        }
    }

    private void CheckMissionComplete()
    {
        if (!_firstDone || !_secondDone) return;

        GD.Print("[GameManager] Both objectives complete — showing MissionComplete.");

        if (_missionComplete != null)
        {
            _missionComplete.Visible = true;
            GD.Print("[GameManager] MissionComplete is now visible.");
        }
        else
        {
            GD.PrintErr("[GameManager] MissionComplete label is NULL — check node name spelling in the scene tree!");
        }

        // Set all NPCs to Neutral so they stop chasing the player
        NeutraliseAllNpcs();
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
                GD.Print($"[GameManager] NPC '{npc.Name}' set to Neutral.");
            }

            NeutraliseNpcsRecursive(child);
        }
    }

    // ── NPC CONNECTION ────────────────────────────────────────────
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
    }

    // ── GAME EVENTS ───────────────────────────────────────────────
    public void OnPlayerCaught()
    {
        if (_gameOver) return;
        _gameOver = true;

        if (_gameOverLabel != null)
            _gameOverLabel.Visible = true;

        SetHint("Press R to try again");
        GD.Print("[GameManager] Player was caught!");

        var player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
        if (player != null)
            player.SetPhysicsProcess(false);
    }

    public void OnPlayerWon()
    {
        if (_gameOver) return;
        _gameOver = true;

        if (_statusLabel != null)
        {
            _statusLabel.Text = "YOU WIN!";
            _statusLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.1f));
            _statusLabel.Visible = true;
        }

        SetHint("Press R to play again");
        GD.Print("[GameManager] Player won!");
    }

    // ── HELPERS ───────────────────────────────────────────────────
    public bool AreObjectivesComplete()
    {
        return _firstDone && _secondDone;
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