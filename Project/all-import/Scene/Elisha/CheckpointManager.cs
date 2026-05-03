using Godot;
using System;

public partial class CheckpointManager : Node
{
    public override void _Ready()
    {
        // Get all nodes in the "checkpoint" group
        var checkpoints = GetTree().GetNodesInGroup("checkpoint");

        foreach (Node node in checkpoints)
        {
            if (node is Area3D checkpoint)
            {
                checkpoint.BodyEntered += body => OnCheckpointEntered(body, checkpoint);
            }
        }
    }

    private void OnCheckpointEntered(Node body, Area3D checkpoint)
    {
        // Check if the body is your player
        if (body is CharacterBody3D player && body.Name == "player")
        {
            GD.Print("got through ring 1");
        }
    }
}