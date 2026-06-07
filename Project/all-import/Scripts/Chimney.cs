using Godot;


public partial class Chimney : Area3D
{
	[Export] private AudioStreamPlayer _thrashDesposeSound;
	private int _deletedItems = 0;
	private int _targetItems = 3;

	public override void _Ready()
	{
		// Connect the body_entered signal to the OnBodyEntered method
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		// Check if the body is in the "Pickable" group
		if (body.IsInGroup("Pickable"))
		{
			_thrashDesposeSound.Play();
			// Remove the object from the scene tree
			body.QueueFree();
			_deletedItems++;

			ProgressBar progressBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("UI/FirstObjective/FirstObjectiveProgress");
			if (progressBar != null)
			{
				progressBar.Value = _deletedItems;
			}

			if (_deletedItems >= _targetItems)
			{
				Label completeLabel = GetTree().CurrentScene.GetNodeOrNull<Label>("UI/MissionComplete");
				if (completeLabel != null)
				{
					completeLabel.Visible = true;
				}
			}
		}
	}
}
