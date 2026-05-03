using Godot;

public partial class UI : Control
{
    [Export] private Label _scoreLabel;
    private int _score = 0;
    private bool _gameStarted = false;

    public override void _Ready()
    {
        // Fallback: if the label wasn't assigned in the inspector, try to find a child named "ScoreLabel"
        if (_scoreLabel == null)
        {
            _scoreLabel = GetNodeOrNull<Label>("ScoreLabel");
            if (_scoreLabel == null)
                GD.Print("UI: _scoreLabel not assigned and ScoreLabel child not found.");
        }

        // Vind alle checkpoints in de "checkpoint" group
        var checkpoints = GetTree().GetNodesInGroup("checkpoint");
        GD.Print($"UI found {checkpoints.Count} checkpoints");
        foreach (Node node in checkpoints)
        {
            // The group may be set on a child (eg CollisionShape3D) or the Area3D itself.
            Area3D? area = null;
            if (node is Area3D a) area = a;
            else
            {
                Node? parent = node;
                while (parent != null && !(parent is Area3D))
                    parent = parent.GetParent();
                if (parent is Area3D p) area = p;
            }

            if (area != null)
            {
                area.BodyEntered += OnCheckpointBodyEntered;
                GD.Print($"UI: connected to checkpoint area '{area.Name}'");
            }
            else
            {
                GD.Print($"UI: group member '{node.Name}' has no Area3D parent to connect to.");
            }
        }

        // Enable scoring after everything is loaded
        _gameStarted = true;
    }

    private void OnCheckpointBodyEntered(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        AddPoint();
    }

    public void AddPoint()
    {
        if (!_gameStarted) return;
        _score++;
        if (_scoreLabel != null)
            _scoreLabel.Text = $"Score: {_score}";
        GD.Print($"added point, total {_score}");
    }
}