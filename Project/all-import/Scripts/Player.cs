using Godot;

public partial class Player : CharacterBody3D
{
    private enum MovementMode
    {
        Ground,
        Flight
    }

    [Export] public float MouseSensitivity = 0.2f;

    [Export] public float Speed = 5f;
    [Export] public float RunMultiplier = 1.8f;
    [Export] public float JumpForce = 5f;
    [Export] public float Gravity = 13f;

    [Export] public float FlightGravityMultiplier = 0.35f;
    [Export] public float FlightDrag = 20f;
    [Export] public float FlightFlapImpulse = 5f;
    [Export] public float FlightFlapCooldown = 0.2f;
    [Export] public float FlightMaxForwardSpeed = 20f;
    [Export] public float FlightTurnSpeed = 2.5f;
    [Export] public float FlightPitchSpeed = 2.0f;
    [Export] public float FlightMaxPitch = 45f;
    [Export] public float FlightBankAngle = 22f;
    [Export] public float FlightBankSpeed = 5f;

    [Export] public float CameraFollowSpeed = 6f;
    [Export] public float CameraSnapSpeed = 14f;
    [Export] public Vector3 GroundCameraOffset = new Vector3(0f, 2.5f, 6f);
    [Export] public Vector3 FlightCameraOffset = new Vector3(0f, 2f, 8f);
    [Export] public float GroundCameraPitchLimit = 55f;
    [Export] public float FlightCameraPitchLimit = 70f;

    private MovementMode _mode = MovementMode.Ground;
    private float _groundCameraPitch;
    private float _flightCameraYaw;
    private float _flightCameraPitch;
    private float _flapCooldownTimer;
    private bool _justLanded;

    private Node3D _cameraPivot;
    private Camera3D _camera;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _cameraPivot = GetNode<Node3D>("CameraPivot");
        _camera = GetNode<Camera3D>("CameraPivot/Camera3D");
        _camera.MakeCurrent();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseMotion motion)
        {
            return;
        }

        if (_mode == MovementMode.Ground)
        {
            RotateY(Mathf.DegToRad(-motion.Relative.X * MouseSensitivity));

            _groundCameraPitch -= motion.Relative.Y * MouseSensitivity;
            _groundCameraPitch = Mathf.Clamp(_groundCameraPitch, -GroundCameraPitchLimit, GroundCameraPitchLimit);
        }
        else
        {
            // In flight, mouse controls full camera orbit (both axes).
            _flightCameraYaw -= motion.Relative.X * MouseSensitivity;
            _flightCameraPitch -= motion.Relative.Y * MouseSensitivity;
            _flightCameraPitch = Mathf.Clamp(_flightCameraPitch, -FlightCameraPitchLimit, FlightCameraPitchLimit);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float deltaF = (float)delta;
        bool wasOnFloor = IsOnFloor();
        Vector3 velocity = Velocity;

        if (_flapCooldownTimer > 0f)
        {
            _flapCooldownTimer -= deltaF;
        }

        if (_mode == MovementMode.Ground)
        {
            velocity = ProcessGroundMovement(velocity, deltaF, wasOnFloor);
        }
        else
        {
            velocity = ProcessFlightMovement(velocity, deltaF);
        }

        Velocity = velocity;
        MoveAndSlide();

        bool isOnFloor = IsOnFloor();
        if (isOnFloor && _mode != MovementMode.Ground)
        {
            EnterGroundMode();
        }
        else if (!isOnFloor && _mode != MovementMode.Flight)
        {
            EnterFlightMode();
        }

        UpdateCamera(deltaF);
    }

    private Vector3 ProcessGroundMovement(Vector3 velocity, float delta, bool wasOnFloor)
    {
        float currentSpeed = Speed;
        if (Input.IsActionPressed("run"))
        {
            currentSpeed *= RunMultiplier;
        }

        if (!wasOnFloor)
        {
            velocity.Y -= Gravity * delta;
        }
        else if (velocity.Y < 0f)
        {
            velocity.Y = 0f;
        }

        Vector2 inputDir = Input.GetVector("move_left", "move_right", "forward_up", "backwards_down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0f, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * currentSpeed;
            velocity.Z = direction.Z * currentSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0f, currentSpeed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0f, currentSpeed);
        }

        if (Input.IsActionJustPressed("jump") && wasOnFloor)
        {
            velocity.Y = JumpForce;
        }

        return velocity;
    }

    private Vector3 ProcessFlightMovement(Vector3 velocity, float delta)
    {
        float turnInput = Input.GetAxis("move_left", "move_right");
        // Reversed: W (forward_up) now pitches DOWN, S (backwards_down) pitches UP
        float pitchInput = -Input.GetAxis("forward_up", "backwards_down");

        float yawDelta = turnInput * FlightTurnSpeed * delta;

        float targetPitch = Mathf.Clamp(
            Rotation.X - (pitchInput * FlightPitchSpeed * delta),
            Mathf.DegToRad(-FlightMaxPitch),
            Mathf.DegToRad(FlightMaxPitch));
        float targetYaw = Rotation.Y - yawDelta;
        float targetBank = Mathf.DegToRad(-turnInput * FlightBankAngle);
        float targetBankAngle = Mathf.Lerp(Rotation.Z, targetBank, Mathf.Clamp(FlightBankSpeed * delta, 0f, 1f));

        Rotation = new Vector3(targetPitch, targetYaw, targetBankAngle);

        // Shift camera yaw along with A/D so it stays behind the character.
        // W/S pitch does NOT shift the camera.
        _flightCameraYaw += Mathf.RadToDeg(yawDelta);

        // --- Bird-like momentum: redirect ALL velocity along the 3D forward ---
        // The bird always flies where it faces.
        float speed = velocity.Length();
        Vector3 forward = -Transform.Basis.Z; // character's current 3D forward
        velocity = forward * speed;

        // Apply drag in local space
        Vector3 localVelocity = Transform.Basis.Inverse() * velocity;
        localVelocity.X = Mathf.MoveToward(localVelocity.X, 0f, FlightDrag * delta);
        localVelocity.Y = Mathf.MoveToward(localVelocity.Y, 0f, FlightDrag * delta);
        localVelocity.Z = Mathf.Clamp(localVelocity.Z, -FlightMaxForwardSpeed, FlightMaxForwardSpeed);

        // Flap pulse (one-shot impulse with cooldown)
        if (Input.IsActionJustPressed("fly") && _flapCooldownTimer <= 0f)
        {
            localVelocity.Z -= FlightFlapImpulse;
            // Clamp after impulse
            localVelocity.Z = Mathf.Max(localVelocity.Z, -FlightMaxForwardSpeed);
            _flapCooldownTimer = FlightFlapCooldown;
        }

        velocity = Transform.Basis * localVelocity;
        velocity.Y -= Gravity * FlightGravityMultiplier * delta;

        return velocity;
    }

    private void UpdateCamera(float delta)
    {
        if (_camera == null || _cameraPivot == null)
        {
            return;
        }

        // Use a fast snap speed right after landing, otherwise normal follow speed
        float blendSpeed = _justLanded ? CameraSnapSpeed : CameraFollowSpeed;
        float cameraBlend = Mathf.Clamp(blendSpeed * delta, 0f, 1f);
        Vector3 targetOffset = _mode == MovementMode.Ground ? GroundCameraOffset : FlightCameraOffset;
        _camera.Position = _camera.Position.Lerp(targetOffset, cameraBlend);

        if (_mode == MovementMode.Ground)
        {
            // Smoothly interpolate the pivot back to behind the PC
            Vector3 currentRot = _cameraPivot.RotationDegrees;
            Vector3 targetRot = new Vector3(_groundCameraPitch, 0f, 0f);
            _cameraPivot.RotationDegrees = currentRot.Lerp(targetRot, cameraBlend);

            // Once close enough, clear the just-landed flag
            if (_justLanded && currentRot.DistanceTo(targetRot) < 1f)
            {
                _justLanded = false;
            }
        }
        else
        {
// Camera is independent from WS pitch, but sticks to player AD yaw
            _cameraPivot.GlobalRotationDegrees = new Vector3(_flightCameraPitch, GlobalRotationDegrees.Y + _flightCameraYaw, 0f);        }

        _camera.LookAt(GlobalPosition + Vector3.Up * 1.5f, Vector3.Up);
    }

    private void EnterGroundMode()
    {
        _mode = MovementMode.Ground;
        Rotation = new Vector3(0f, Rotation.Y, 0f);
        // Snap camera yaw immediately; pitch will be smoothed in UpdateCamera
        _flightCameraYaw = 0f;
        _flightCameraPitch = 0f;
        _justLanded = true;
    }

    private void EnterFlightMode()
    {
        _mode = MovementMode.Flight;
        _flightCameraYaw = 0f;
        _flightCameraPitch = Mathf.Clamp(_groundCameraPitch, -FlightCameraPitchLimit, FlightCameraPitchLimit);
        _flapCooldownTimer = 0f;
    }
}