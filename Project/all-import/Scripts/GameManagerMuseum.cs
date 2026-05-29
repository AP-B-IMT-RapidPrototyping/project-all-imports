using Godot;

/// <summary>
/// Manages the museum heist level:
///   • Objective 1: break the glass cover on the diamond pedestal using
///     weighty objects (BreakableGlas frees the Cover's children when hit).
///   • Objective 2: steal the diamond and carry it to the MissionCompletePoint.
///       progress = dist(DiamondPedestal, MissionCompletePoint)
///                / dist(Diamond,         MissionCompletePoint)
///   • Level is cleared when the diamond is within 2 m of the MissionCompletePoint.
///   • On success the level transitions to the main menu (like Level1/Tutorial).
///   • NPC catch → Game Over. R key restarts the scene.
/// </summary>
public partial class GameManagerMuseum : Node
{
    private Label _hintLabel;
    private Label _firstObjective;
    private Label _secondObjective;
    private Label _missionComplete;
    private Label _gameOverLabel;
    private ProgressBar _firstProgress;
    private ProgressBar _secondProgress;

    private Node3D _diamond;
    private Node3D _pedestal;
    private Node3D _missionPoint;
    private Node _cover;

    private float _pedestalToPointDist;

    private bool _coverBroken = false;
    private bool _missionDone = false;
    private bool _gameOver    = false;

    private const float ClearDistance = 2.0f;

    public override void _Ready()
    {
        _hintLabel       = GetTree().Root.FindChild("HintLabel",              true, false) as Label;
        _firstObjective  = GetTree().Root.FindChild("FirstObjective",         true, false) as Label;
        _secondObjective = GetTree().Root.FindChild("SecondObjective",        true, false) as Label;
        _missionComplete = GetTree().Root.FindChild("MissionComplete",        true, false) as Label;
        _gameOverLabel   = GetTree().Root.FindChild("GameOver",               true, false) as Label;
        _firstProgress   = GetTree().Root.FindChild("FirstObjectiveProgress", true, false) as ProgressBar;
        _secondProgress  = GetTree().Root.FindChild("SecondObjectiveProgress",true, false) as ProgressBar;

        _diamond      = GetTree().Root.FindChild("Diamond",              true, false) as Node3D;
        _pedestal     = GetTree().Root.FindChild("DiamondPedestal",      true, false) as Node3D;
        _missionPoint = GetTree().Root.FindChild("MissionCompletePoint", true, false) as Node3D;
        _cover        = GetTree().Root.FindChild("Cover",                true, false);

        if (_missionComplete != null) _missionComplete.Visible = false;
        if (_gameOverLabel   != null) _gameOverLabel.Visible   = false;

        if (_firstObjective != null)
        {
            _firstObjective.Text    = "-Break the cover using weighty stuff";
            _firstObjective.Visible = true;
        }
        if (_secondObjective != null)
        {
            _secondObjective.Text    = "-Steal the diamond and leave";
            _secondObjective.Visible = true;
        }

        SetupProgress(_firstProgress);
        SetupProgress(_secondProgress);

        if (_pedestal != null && _missionPoint != null)
            _pedestalToPointDist = _pedestal.GlobalPosition.DistanceTo(_missionPoint.GlobalPosition);

        ConnectAllNpcs(GetParent() ?? this);
    }

    private void SetupProgress(ProgressBar bar)
    {
        if (bar == null) return;
        bar.MinValue       = 0;
        bar.MaxValue       = 100;
        bar.Step           = 1;
        bar.ShowPercentage = false;
        bar.Value          = 0;
        bar.Visible        = true;
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

        // ── Objective 1: break the glass cover ───────────────────────
        if (!_coverBroken && IsCoverBroken())
        {
            _coverBroken = true;
            if (_firstProgress != null) _firstProgress.Value = 100;
            GD.Print("[GameManagerMuseum] Glass cover broken!");
        }

        // ── Objective 2: steal the diamond and leave ─────────────────
        if (_diamond != null && _missionPoint != null && _pedestalToPointDist > 0)
        {
            float diamondDist = _diamond.GlobalPosition.DistanceTo(_missionPoint.GlobalPosition);

            if (_secondProgress != null && diamondDist > 0.0001f)
            {
                float progress = 100f * (_pedestalToPointDist / diamondDist);
                _secondProgress.Value = Mathf.Clamp(progress, 0, 100);
            }

            if (_coverBroken && diamondDist < ClearDistance)
                CompleteMission();
        }
    }

    // The Cover node keeps the BreakableGlas script; BreakableGlas frees all of
    // its children once it's hit hard enough, so "no children left" = broken.
    private bool IsCoverBroken()
    {
        if (_cover == null || !IsInstanceValid(_cover)) return true;
        return _cover.GetChildCount() == 0;
    }

    private async void CompleteMission()
    {
        if (_missionDone) return;
        _missionDone = true;

        GD.Print("[GameManagerMuseum] Diamond delivered — mission complete!");

        if (_missionComplete != null) _missionComplete.Visible = true;
        if (_firstObjective  != null) _firstObjective.Visible  = false;
        if (_secondObjective != null) _secondObjective.Visible = false;
        if (_firstProgress   != null) _firstProgress.Value     = 100;
        if (_secondProgress  != null) _secondProgress.Value    = 100;

        NeutraliseAllNpcs();

        await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
        GetTree().ChangeSceneToFile("res://Scene/Tutku/main_menu.tscn");
    }

    public void OnPlayerCaught()
    {
        if (_gameOver || _missionDone) return;
        _gameOver = true;

        if (_gameOverLabel != null) _gameOverLabel.Visible = true;
        SetHint("Press R to try again");

        var player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
        if (player != null) player.SetPhysicsProcess(false);

        GD.Print("[GameManagerMuseum] Player was caught!");
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
