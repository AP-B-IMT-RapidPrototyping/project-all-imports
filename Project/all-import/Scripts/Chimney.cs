using Godot;

public partial class Chimney : Area3D
{
	public override void _Ready()
	{
		// Connect the body_entered signal to the OnBodyEntered method
		Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
	}

	private void OnBodyEntered(Node body)
	{
		// Check if the body is in the "Pickable" group
		if (body.IsInGroup("Pickable"))
		{
			// Remove the object from the scene tree
			body.QueueFree();
		}
	}
}
