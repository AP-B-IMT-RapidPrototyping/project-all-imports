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
    [Export] public float FlightDrag = 3.5f;
    [Export] public float FlightFlapAcceleration = 18f;
    [Export] public float FlightMaxForwardSpeed = 20f;
    [Export] public float FlightTurnSpeed = 2.5f;
    [Export] public float FlightPitchSpeed = 2.0f;
    [Export] public float FlightMaxPitch = 45f;
    [Export] public float FlightBankAngle = 22f;
    [Export] public float FlightBankSpeed = 5f;

    [Export] public float CameraFollowSpeed = 6f;
    [Export] public Vector3 GroundCameraOffset = new Vector3(0f, 2.5f, 6f);
    [Export] public Vector3 FlightCameraOffset = new Vector3(0f, 2f, 8f);
    [Export] public float GroundCameraPitchLimit = 55f;
    [Export] public float FlightCameraPitchLimit = 70f;

    private MovementMode _mode = MovementMode.Ground;
    private float _groundCameraPitch;
    private float _flightCameraYaw;
    private float _flightCameraPitch;

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
        float pitchInput = Input.GetAxis("forward_up", "backwards_down");

        float targetPitch = Mathf.Clamp(Rotation.X - (pitchInput * FlightPitchSpeed * delta), Mathf.DegToRad(-FlightMaxPitch), Mathf.DegToRad(FlightMaxPitch));
        float targetYaw = Rotation.Y - (turnInput * FlightTurnSpeed * delta);
        float targetBank = Mathf.DegToRad(-turnInput * FlightBankAngle);
        float targetBankAngle = Mathf.Lerp(Rotation.Z, targetBank, Mathf.Clamp(FlightBankSpeed * delta, 0f, 1f));

        Rotation = new Vector3(targetPitch, targetYaw, targetBankAngle);

        Vector3 localVelocity = Transform.Basis.Inverse() * velocity;
        localVelocity.X = Mathf.MoveToward(localVelocity.X, 0f, FlightDrag * delta);
        localVelocity.Y = Mathf.MoveToward(localVelocity.Y, 0f, FlightDrag * delta);
        localVelocity.Z = Mathf.Clamp(localVelocity.Z, -FlightMaxForwardSpeed, FlightMaxForwardSpeed);

        if (Input.IsActionPressed("fly"))
        {
            localVelocity.Z = Mathf.Max(localVelocity.Z - (FlightFlapAcceleration * delta), -FlightMaxForwardSpeed);
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

        float cameraBlend = Mathf.Clamp(CameraFollowSpeed * delta, 0f, 1f);
        Vector3 targetOffset = _mode == MovementMode.Ground ? GroundCameraOffset : FlightCameraOffset;
        _camera.Position = _camera.Position.Lerp(targetOffset, cameraBlend);

        if (_mode == MovementMode.Ground)
        {
            _cameraPivot.RotationDegrees = new Vector3(_groundCameraPitch, 0f, 0f);
        }
        else
        {
            _cameraPivot.RotationDegrees = new Vector3(_flightCameraPitch, _flightCameraYaw, 0f);
        }

        _camera.LookAt(GlobalPosition + Vector3.Up * 1.5f, Vector3.Up);
    }

    private void EnterGroundMode()
    {
        _mode = MovementMode.Ground;
        Rotation = new Vector3(0f, Rotation.Y, 0f);
        _flightCameraYaw = 0f;
        _flightCameraPitch = 0f;
    }

    private void EnterFlightMode()
    {
        _mode = MovementMode.Flight;
        _flightCameraYaw = 0f;
        _flightCameraPitch = Mathf.Clamp(_groundCameraPitch, -FlightCameraPitchLimit, FlightCameraPitchLimit);
    }
}