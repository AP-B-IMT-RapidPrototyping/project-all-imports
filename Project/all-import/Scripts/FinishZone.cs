using Godot;

/// <summary>
/// Placed at the end of the obstacle course.
/// When the player enters this Area3D, it checks if objectives are complete.
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
        if (!body.IsInGroup("player"))
            return;

        var gm = GetNodeOrNull<GameManager>(GameManagerPath);

        if (gm == null)
            return;

        // Only allow winning after both objectives are complete
        if (gm.AreObjectivesComplete())
        {
            gm.OnPlayerWon();
        }
        else
        {
            GD.Print("Objectives not complete yet!");
        }
    }
}