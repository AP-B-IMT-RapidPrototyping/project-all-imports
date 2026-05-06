using Godot;

/// <summary>
/// Manages the obstacle-course mini-game:
///   • Score via checkpoints
///   • NPC catch → Game Over
///   • Finish → You Win
///   • R key restarts the scene
/// </summary>
public partial class GameManager : Node
{
    // ── UI references ─────────────────────────────────────────────
    [Export] public NodePath StatusLabelPath = "HUD/StatusLabel";
    [Export] public NodePath HintLabelPath = "HUD/HintLabel";
    [Export] public NodePath ScoreLabelPath = "HUD/ScoreLabel";

    private Label _statusLabel;
    private Label _hintLabel;
    private Label _scoreLabel;

    // ── Game state ────────────────────────────────────────────────
    private bool _gameOver = false;
    private bool _gameStarted = false;
    private int _score = 0;

    public override void _Ready()
    {
        _statusLabel = GetNodeOrNull<Label>(StatusLabelPath);
        _hintLabel = GetNodeOrNull<Label>(HintLabelPath);
        _scoreLabel = GetNodeOrNull<Label>(ScoreLabelPath);

        if (_scoreLabel == null)
            GD.Print("GameManager: ScoreLabel not assigned and not found.");

        // Initialize score text
        if (_scoreLabel != null)
            _scoreLabel.Text = $"Score: {_score}";

        // Connect NPCs
        var root = GetParent() ?? this;
        ConnectAllNpcs(root);

        // Connect checkpoints
        ConnectCheckpoints();
    }

    // ── CHECKPOINT SYSTEM ─────────────────────────────────────────
    private void ConnectCheckpoints()
    {
        var checkpoints = GetTree().GetNodesInGroup("checkpoint");

        GD.Print($"GameManager found {checkpoints.Count} checkpoints");

        foreach (Node node in checkpoints)
        {
            Area3D area = null;

            // If node itself is Area3D
            if (node is Area3D a)
            {
                area = a;
            }
            else
            {
                // Search upward for Area3D parent
                Node parent = node;

                while (parent != null && parent is not Area3D)
                {
                    parent = parent.GetParent();
                }

                if (parent is Area3D foundArea)
                    area = foundArea;
            }

            if (area != null)
            {
                // Prevent duplicate connections (safely disconnect first)
                try
                {
                    area.BodyEntered -= OnCheckpointBodyEntered;
                }
                catch
                {
                    // Connection didn't exist, that's fine
                }
                
                area.BodyEntered += OnCheckpointBodyEntered;

                GD.Print($"GameManager: connected to checkpoint '{area.Name}'");
            }
            else
            {
                GD.Print($"GameManager: '{node.Name}' has no Area3D parent.");
            }
        }

        // Enable scoring after everything is connected
        _gameStarted = true;
    }

    private void OnCheckpointBodyEntered(Node body)
    {
        GD.Print($"Entered checkpoint with: {body.Name}");

        // Walk up the node tree looking for the player group
        Node current = body;

        while (current != null)
        {
            if (current.IsInGroup("player"))
            {
                GD.Print("Player detected!");
                AddPoint();
                return;
            }

            current = current.GetParent();
        }

        GD.Print("Not player, ignored.");
    }

    private void AddPoint()
    {
        if (!_gameStarted)
            return;

        _score++;

        if (_scoreLabel != null)
        {
            _scoreLabel.Text = $"Score: {_score}";
        }
        else
        {
            GD.Print("ScoreLabel missing!");
        }

        GD.Print($"Added point, total {_score}");
    }

    // ── NPC CONNECTION ────────────────────────────────────────────
    private void ConnectAllNpcs(Node node)
    {
        if (node == null)
            return;

        foreach (Node child in node.GetChildren())
        {
            if (child is Npc npc)
            {
                npc.CaughtPlayer += OnPlayerCaught;
            }

            // Recurse into children
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
        {
            Restart();
        }
    }

    // ── GAME EVENTS ───────────────────────────────────────────────
    public void OnPlayerCaught()
    {
        if (_gameOver)
            return;

        _gameOver = true;

        if (_statusLabel != null)
        {
            _statusLabel.Text = "GAME OVER";
            _statusLabel.AddThemeColorOverride(
                "font_color",
                new Color(1f, 0.2f, 0.2f)
            );

            _statusLabel.Visible = true;
        }

        SetHint("Press R to try again");

        GD.Print("[GameManager] Player was caught!");

        // Freeze player
        var player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;

        if (player != null)
        {
            player.SetPhysicsProcess(false);
        }
    }

    public void OnPlayerWon()
    {
        if (_gameOver)
            return;

        _gameOver = true;

        if (_statusLabel != null)
        {
            _statusLabel.Text = "YOU WIN!";
            _statusLabel.AddThemeColorOverride(
                "font_color",
                new Color(1f, 0.85f, 0.1f)
            );

            _statusLabel.Visible = true;
        }

        SetHint($"Final Score: {_score} | Press R to play again");

        GD.Print("[GameManager] Player won!");
    }

    // ── HELPERS ───────────────────────────────────────────────────
    private void SetHint(string text)
    {
        if (_hintLabel != null)
        {
            _hintLabel.Text = text;
        }
    }

    private void Restart()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        GetTree().ReloadCurrentScene();
    }
}

