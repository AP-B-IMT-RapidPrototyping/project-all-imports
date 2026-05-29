using Godot;

public partial class  BreakableGlas : Area3D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is StealableObject)
        {
            GetParent<MeshInstance3D>().QueueFree();
        }
    }
}
