using Godot;

public partial class PlayerController : CharacterBody3D
{
    [Export] private float _forwardSpeed = 10.0f;
    [Export] private float _turnSpeed = 2.0f;
    [Export] private float _verticalSpeed = 5.0f;
    [Export] private float _boostMultiplier = 1.2f;
    [Export] private float _maxHeight = 4f;
    [Export] private float _bankAngle = 30f;
    [Export] private float _bankSpeed = 5f;
    [Export] private float _boostTransitionSpeed = 3f;

    [Export] private Camera3D _camera;
    [Export] private GpuParticles3D _speedLines;

    private const float BaseFov = 75.0f;
    private const float BoostFov = 90.0f;

    private float _currentSpeed;

    public override void _PhysicsProcess(double delta)
    {
        Vector3 movement = Vector3.Zero;
        float turnInput = 0f;

        // Bepaal target snelheid: boost of normaal
        float targetSpeed = _forwardSpeed;
        if (Input.IsActionPressed("boost"))
            targetSpeed *= _boostMultiplier;

        // Smooth transitie naar target speed
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _boostTransitionSpeed * (float)delta);

        // Auto-forward movement met smooth boost
        movement += Transform.Basis.Z * _currentSpeed;

        // Links/Rechts - draai het schip EN kantel het
        if (Input.IsActionPressed("move_left"))
        {
            Rotation = new Vector3(Rotation.X, Rotation.Y + _turnSpeed * (float)delta, Rotation.Z);
            turnInput = -1f;
        }
        else if (Input.IsActionPressed("move_right"))
        {
            Rotation = new Vector3(Rotation.X, Rotation.Y - _turnSpeed * (float)delta, Rotation.Z);
            turnInput = 1f;
        }

        // Smooth banking
        float targetBankAngle = turnInput * Mathf.DegToRad(_bankAngle);
        float newBankAngle = Mathf.Lerp(Rotation.Z, targetBankAngle, _bankSpeed * (float)delta);

        Rotation = new Vector3(Rotation.X, Rotation.Y, newBankAngle);

        // Omhoog/Omlaag (Y-as)
        if (Input.IsActionPressed("move_up"))
            movement.Y += _verticalSpeed;
        if (Input.IsActionPressed("move_down"))
            movement.Y -= _verticalSpeed;

        // Pas de Velocity aan en beweeg met physics
        Velocity = movement;
        MoveAndSlide();

        // Beperk hoogte
        float clampedY = Mathf.Clamp(Position.Y, 0.5f, _maxHeight);
        Position = new Vector3(Position.X, clampedY, Position.Z);
    }

    public override void _Process(double delta)
    {
        UpdateCameraFov();
    }

    private void UpdateCameraFov()
    {
        if (_camera == null)
            return;

        // Check of speler aan het boosten is
        bool isBoosting = Input.IsActionPressed("boost");

        // Bepaal target FOV: boost of normaal
        float targetFov = isBoosting ? BoostFov : BaseFov;

        // Smooth interpolatie naar target FOV
        _camera.Fov = Mathf.Lerp(_camera.Fov, targetFov, 0.1f);

        // Update speed lines
        _speedLines.Emitting = isBoosting;
    }
}