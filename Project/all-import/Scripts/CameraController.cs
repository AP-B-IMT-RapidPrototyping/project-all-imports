using Godot;

public partial class CameraController : Camera3D
{
	[Export] private PlayerController _target;
	[Export] private Vector3 _offset = new Vector3(0, 3, -6);
	[Export] private float _smoothSpeed = 5f;

	public override void _PhysicsProcess(double delta)
	{
		// Transform the offset by the target's rotation so it stays behind the target
		Vector3 rotatedOffset = _target.GlobalTransform.Basis * _offset;
		Vector3 desiredPosition = _target.GlobalPosition + rotatedOffset;

		// Smooth follow met lerp - beweeg geleidelijk naar de gewenste positie
		GlobalPosition = GlobalPosition.Lerp(desiredPosition, _smoothSpeed * (float)delta);

		// Kijk altijd naar de target
		LookAt(_target.GlobalPosition, Vector3.Up);
	}
}