using Godot;

/// <summary>
/// Manages the obstacle-course mini-game:
///   • FinishZone calls AreObjectivesComplete() then OnPlayerWon()
///   • OnPlayerWon() shows MissionComplete label
///   • _Process watches for MissionComplete → freeze 2 seconds → Level-2
///   • NPC catch → Game Over
///   • R key restarts the scene
/// </summary>
public partial class GameManagerLevel1 : Node
{
    private Label _statusLabel;
    private Label _hintLabel;
    private Label _firstObjective;
    private Label _secondObjective;
    private Label _missionComplete;
    private Label _gameOverLabel;

    // ── Game state ────────────────────────────────────────────────
    private bool _gameOver          = false;
    private bool _transitionStarted = false;

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

        // These start hidden
        if (_missionComplete != null) _missionComplete.Visible = false;
        if (_gameOverLabel   != null) _gameOverLabel.Visible   = false;

        ConnectAllNpcs(GetParent() ?? this);
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

        // Watch for MissionComplete becoming visible
        if (!_gameOver && !_transitionStarted && _missionComplete != null && _missionComplete.Visible)
        {
            GD.Print("[GameManager] MissionComplete is visible — starting transition...");
            StartTransition();
        }
    }

    private async void StartTransition()
    {
        _transitionStarted = true;
        _gameOver          = true;

        NeutraliseAllNpcs();

        GD.Print("[GameManager] Freezing for 2 seconds...");
        GetTree().Paused = true;

        await ToSignal(GetTree().CreateTimer(2.0f, true, false, true), SceneTreeTimer.SignalName.Timeout);

        GD.Print("[GameManager] Transitioning to Level-2...");
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Levels/Level-2.tscn");
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

        GD.Print("[GameManager] Player reached the finish zone — showing MissionComplete.");

        // Showing MissionComplete triggers the _Process watcher to start the transition
        if (_missionComplete != null)
            _missionComplete.Visible = true;

        if (_firstObjective  != null) _firstObjective.Visible  = false;
        if (_secondObjective != null) _secondObjective.Visible = false;
    }

    // ── HELPERS ───────────────────────────────────────────────────

    // Called by FinishZone to check if the player is allowed to finish
    public bool AreObjectivesComplete()
    {
        // No checkpoint objectives in this level — finishing the zone is enough
        return true;
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