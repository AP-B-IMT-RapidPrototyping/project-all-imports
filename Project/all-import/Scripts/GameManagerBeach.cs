using Godot;

/// <summary>
/// Manages the beach mini-game:
///   • Tracks the two sand-castles (group "sand_castle").
///   • Progress bar fills as castles are destroyed.
///   • Mission complete when both are gone.
///   • Player entering water (WaterArea) or being caught by an NPC = game over.
/// </summary>
public partial class GameManagerBeach : Node
{
    private Label _statusLabel;
    private Label _hintLabel;
    private Label _firstObjective;
    private Label _secondObjective;
    private Label _missionComplete;
    private Label _gameOverLabel;
    private ProgressBar _objectiveProgress;

    private int _totalCastles;
    private bool _missionDone = false;
    private bool _gameOver    = false;

    public override void _Ready()
    {
        _statusLabel       = GetTree().Root.FindChild("StatusLabel",            true, false) as Label;
        _hintLabel         = GetTree().Root.FindChild("HintLabel",              true, false) as Label;
        _firstObjective    = GetTree().Root.FindChild("FirstObjective",         true, false) as Label;
        _secondObjective   = GetTree().Root.FindChild("SecondObjective",        true, false) as Label;
        _missionComplete   = GetTree().Root.FindChild("MissionComplete",        true, false) as Label;
        _gameOverLabel     = GetTree().Root.FindChild("GameOver",               true, false) as Label;
        _objectiveProgress = GetTree().Root.FindChild("FirstObjectiveProgress", true, false) as ProgressBar;

        if (_missionComplete != null) _missionComplete.Visible = false;
        if (_gameOverLabel   != null) _gameOverLabel.Visible   = false;

        if (_firstObjective != null)
        {
            _firstObjective.Text    = "-Destroy the 2 sand castles";
            _firstObjective.Visible = true;
        }
        if (_secondObjective != null) _secondObjective.Visible = false;

        _totalCastles = GetTree().GetNodesInGroup("sand_castle").Count;
        if (_totalCastles <= 0) _totalCastles = 2;

        if (_objectiveProgress != null)
        {
            _objectiveProgress.MinValue        = 0;
            _objectiveProgress.MaxValue        = 100;
            _objectiveProgress.Step            = 1;
            _objectiveProgress.ShowPercentage  = false;
            _objectiveProgress.Value           = 0;
            _objectiveProgress.Visible         = true;
        }

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

        if (_missionDone || _gameOver) return;

        int remaining = GetTree().GetNodesInGroup("sand_castle").Count;
        int destroyed = Mathf.Max(_totalCastles - remaining, 0);

        if (_objectiveProgress != null && _totalCastles > 0)
            _objectiveProgress.Value = 100f * destroyed / _totalCastles;

        if (remaining <= 0)
            CompleteMission();
    }

    private async void CompleteMission()
    {
        _missionDone = true;

        if (_missionComplete != null) _missionComplete.Visible = true;
        if (_firstObjective  != null) _firstObjective.Visible  = false;
        if (_objectiveProgress != null) _objectiveProgress.Value = 100;

        NeutraliseAllNpcs();

        GD.Print("[GameManagerBeach] Mission complete — both castles destroyed.");

        await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
        GetTree().ChangeSceneToFile("res://Levels/LevelMuseum_Tutku.tscn");
    }

    public void OnPlayerDrowned()
    {
        if (_gameOver || _missionDone) return;
        _gameOver = true;

        if (_gameOverLabel != null) _gameOverLabel.Visible = true;
        SetHint("Press R to try again");

        var player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
        if (player != null) player.SetPhysicsProcess(false);

        GD.Print("[GameManagerBeach] Player drowned!");
    }

    public void OnPlayerCaught()
    {
        if (_gameOver || _missionDone) return;
        _gameOver = true;

        if (_gameOverLabel != null) _gameOverLabel.Visible = true;
        SetHint("Press R to try again");

        var player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
        if (player != null) player.SetPhysicsProcess(false);

        GD.Print("[GameManagerBeach] Player was caught!");
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
                npc.SetStateExternal(Npc.NpcState.Neutral);
            NeutraliseNpcsRecursive(child);
        }
    }

    private void SetHint(string text)
    {
        if (_hintLabel != null) _hintLabel.Text = text;
    }

    private void Restart()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        GetTree().ReloadCurrentScene();
    }
}
