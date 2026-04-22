using Godot;

/// <summary>
/// Manages the obstacle-course mini-game:
///   • Listens for "player_caught"  → Game Over
///   • Listens for "player_won"     → You Win!
///   • R key restarts the scene at any time
/// </summary>
public partial class GameManager : Node
{
    // ── Node refs wired up in scene ───────────────────────────────────────────
    [Export] public NodePath StatusLabelPath  = "HUD/StatusLabel";
    [Export] public NodePath HintLabelPath    = "HUD/HintLabel";

    private Label _statusLabel;
    private Label _hintLabel;
    private bool  _gameOver = false;

    public override void _Ready()
    {
        _statusLabel = GetNodeOrNull<Label>(StatusLabelPath);
        _hintLabel   = GetNodeOrNull<Label>(HintLabelPath);

        // Show controls hint
        SetHint("Hold SPACE to fly • WASD to steer • Reach the gold ring!\nESC = unlock mouse   R = restart");

        // Dynamically connect to all NPCs in the level
        var parent = GetParent();
        if (parent != null)
        {
            foreach (Node child in parent.GetChildren())
            {
                if (child is Npc npc)
                {
                    npc.CaughtPlayer += OnPlayerCaught;
                }
            }
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

    // ── Signal receivers (connect from NPC catch / finish zone) ───────────────

    /// Call this when the NPC catches the player.
    public void OnPlayerCaught()
    {
        if (_gameOver) return;
        _gameOver = true;

        if (_statusLabel != null)
        {
            _statusLabel.Text       = "GAME OVER";
            _statusLabel.AddThemeColorOverride("font_color", new Color(1f, 0.2f, 0.2f));
            _statusLabel.Visible    = true;
        }
        SetHint("Press R to try again");
        GD.Print("[GameManager] Player was caught!");

        // Freeze the player so they can't keep flying
        var player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
        if (player != null)
            player.SetPhysicsProcess(false);
    }

    /// Call this when the player reaches the finish area.
    public void OnPlayerWon()
    {
        if (_gameOver) return;
        _gameOver = true;

        if (_statusLabel != null)
        {
            _statusLabel.Text       = "YOU WIN!";
            _statusLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.1f));
            _statusLabel.Visible    = true;
        }
        SetHint("Press R to play again");
        GD.Print("[GameManager] Player won!");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
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
