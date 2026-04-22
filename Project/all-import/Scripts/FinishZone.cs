using Godot;

/// <summary>
/// Placed at the end of the obstacle course.
/// When the player enters this Area3D, it fires "player_won" on the GameManager.
/// </summary>
public partial class FinishZone : Area3D
{
    [Export] public NodePath GameManagerPath = "../GameManager";

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (!body.IsInGroup("player")) return;

        var gm = GetNodeOrNull<GameManager>(GameManagerPath);
        gm?.OnPlayerWon();
    }
}
