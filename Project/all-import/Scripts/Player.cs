using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public float MouseSensitivity = 0.2f;
    private float rotationX = 0f;

    // MOVEMENT
    [Export] public float Speed = 5f;
    [Export] public float RunMultiplier = 1.8f;
    [Export] public float JumpForce = 5f;
    [Export] public float Gravity = 13f;

    // FLYING 
    private float pressTime = 0f;
    [Export] public float HoldThreshold = 0.25f;
    [Export] public float FlyForce = 10f;
    [Export] public float FlightHeight = 5f;
    [Export] public float FlyForwardSpeed = 8f;

    private bool isFlying = false;
    private float flightStartY;

    private Node3D cameraPivot;
    private Camera3D camera;

    public override void _Ready()
    { 
        Input.MouseMode = Input.MouseModeEnum.Captured;
        cameraPivot = GetNode<Node3D>("CameraPivot");
        camera = GetNode<Camera3D>("CameraPivot/Camera3D");
    }

    public override void _Input(InputEvent @event)
    { 
        if (@event is InputEventMouseMotion motion)
        {
            RotateY(Mathf.DegToRad(-motion.Relative.X * MouseSensitivity));

            rotationX -= motion.Relative.Y * MouseSensitivity;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            cameraPivot.RotationDegrees = new Vector3(rotationX, 0, 0);
        }
    }

    public override void _Process(double delta)
    {
        Vector3 velocity = Velocity;

        // RUN INPUT
        float currentSpeed = Speed;
        if (Input.IsActionPressed("run"))
        {
            currentSpeed *= RunMultiplier;
        }

        // GRAVITY (disabled while flying)
        if (!IsOnFloor() && !isFlying)
        {
            velocity.Y -= Gravity * (float)delta;
        }

        // NORMAL MOVEMENT (only when not flying)
        if (!isFlying)
        {
            Vector2 inputDir = Input.GetVector("move_left", "move_right", "forward_up", "backwards_down");
            Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

            if (direction != Vector3.Zero)
            {
                velocity.X = direction.X * currentSpeed;
                velocity.Z = direction.Z * currentSpeed;
            }
            else
            {
                velocity.X = Mathf.MoveToward(velocity.X, 0, currentSpeed);
                velocity.Z = Mathf.MoveToward(velocity.Z, 0, currentSpeed);
            }
        }

        // HOLD SPACE = START FLIGHT
        if (Input.IsActionPressed("jump"))
        {
            pressTime += (float)delta;

            if (pressTime >= HoldThreshold && !isFlying)
            {
                isFlying = true;
                flightStartY = GlobalTransform.Origin.Y;
            }
        }

        // RELEASE SPACE
        if (Input.IsActionJustReleased("jump"))
        {
            // TAP → JUMP
            if (pressTime < HoldThreshold && IsOnFloor())
            {
                velocity.Y = JumpForce;
            }

            pressTime = 0f;
        }

        // PRESS SPACE AGAIN = STOP FLIGHT
        if (isFlying && Input.IsActionJustPressed("jump"))
        {
            isFlying = false;
        }

        // FLIGHT LOGIC
        if (isFlying)
        {
            float targetY = flightStartY + FlightHeight;

            // ASCEND UNTIL HEIGHT
            if (GlobalTransform.Origin.Y < targetY)
            {
                velocity.Y = FlyForce;
            }
            else
            {
                velocity.Y = 0;

                // AUTO FORWARD
                Vector3 forward = -Transform.Basis.Z;
                velocity.X = forward.X * FlyForwardSpeed;
                velocity.Z = forward.Z * FlyForwardSpeed;

                // LEFT / RIGHT CONTROL
                float steer = Input.GetAxis("move_left", "move_right");
                Vector3 right = Transform.Basis.X;
                velocity += right * steer * currentSpeed;
            }
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}