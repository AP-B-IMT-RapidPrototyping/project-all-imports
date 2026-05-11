using Godot;

public partial class PlayerMovement : CharacterBody3D
{
	[Export] public float MouseSensitivity = 0.2f;

	[Export] public float Speed = 5.0f;
	[Export] public float JumpVelocity = 5.0f;
	[Export] public float AirJumpVelocity = 4.0f;
	[Export] public float JumpCooldown = 0.2f;

	[Export] public float GlideFallSpeed = .2f;
	[Export] public float GlideLerpSpeed = 5.0f;
	[Export] public float GlideTurnSpeed = 2.5f;
	[Export] public float GlideBankAngle = 22f;
	[Export] public float GlideBankSpeed = 5f;

	[Export] public float TurnSpeed = 12f;

	[Export] public float CameraPitchLimit = 70f;
	[Export] public Vector3 CameraOffset = new Vector3(0f, 2f, 8f);
	[Export] public float CameraFollowSpeed = 5f;

	private float _cameraYawDeg;
	private float _cameraPitchDeg;
	private float _jumpCooldownTimer;

	private Node3D _cameraPivot;
	private Camera3D _camera;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_cameraPivot = GetNode<Node3D>("CameraPivot");
		_camera = GetNode<Camera3D>("CameraPivot/Camera3D");
		_camera.MakeCurrent();
		_cameraYawDeg = Mathf.RadToDeg(Rotation.Y);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseMotion motion)
		{
			return;
		}

		_cameraYawDeg -= motion.Relative.X * MouseSensitivity;
		_cameraPitchDeg -= motion.Relative.Y * MouseSensitivity;
		_cameraPitchDeg = Mathf.Clamp(_cameraPitchDeg, -CameraPitchLimit, CameraPitchLimit);
	}

	public override void _PhysicsProcess(double delta)
	{
		float deltaF = (float)delta;
		Vector3 velocity = Velocity;

		if (_jumpCooldownTimer > 0f)
		{
			_jumpCooldownTimer -= deltaF;
		}

		bool onFloor = IsOnFloor();

		if (!onFloor)
		{
			velocity += GetGravity() * deltaF;
		}
		else if (velocity.Y < 0f)
		{
			velocity.Y = 0f;
		}

		bool jumpHeld = Input.IsActionPressed("jump");
		bool jumpPressed = Input.IsActionJustPressed("jump");

		if (jumpPressed && _jumpCooldownTimer <= 0f)
		{
			velocity.Y += onFloor ? JumpVelocity : AirJumpVelocity;
			_jumpCooldownTimer = JumpCooldown;
		}

		bool gliding = jumpHeld && !onFloor && velocity.Y < 0f;

		if (gliding)
		{
			velocity.Y = Mathf.Lerp(velocity.Y, -GlideFallSpeed,
				Mathf.Clamp(GlideLerpSpeed * deltaF, 0f, 1f));
			velocity = ProcessGlideHorizontal(velocity, deltaF);
		}
		else
		{
			velocity = ProcessGroundedHorizontal(velocity, deltaF);
		}

		Velocity = velocity;
		MoveAndSlide();

		UpdateCamera(deltaF);
	}

	private Vector3 ProcessGroundedHorizontal(Vector3 velocity, float delta)
	{
		float bankBlend = Mathf.Clamp(GlideBankSpeed * delta, 0f, 1f);
		float clearedBank = Mathf.Lerp(Rotation.Z, 0f, bankBlend);

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "forward_up", "backwards_down");

		float camYawRad = Mathf.DegToRad(_cameraYawDeg);
		Vector3 camRight = new Vector3(Mathf.Cos(camYawRad), 0f, -Mathf.Sin(camYawRad));
		Vector3 camBack = new Vector3(Mathf.Sin(camYawRad), 0f, Mathf.Cos(camYawRad));
		Vector3 direction = (camRight * inputDir.X + camBack * inputDir.Y).Normalized();

		float newYaw = Rotation.Y;
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;

			float targetYaw = Mathf.Atan2(-direction.X, -direction.Z);
			newYaw = Mathf.LerpAngle(Rotation.Y, targetYaw, Mathf.Clamp(TurnSpeed * delta, 0f, 1f));
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0f, Speed);
			velocity.Z = Mathf.MoveToward(velocity.Z, 0f, Speed);
		}

		Rotation = new Vector3(0f, newYaw, clearedBank);
		return velocity;
	}

	private Vector3 ProcessGlideHorizontal(Vector3 velocity, float delta)
	{
		float turnInput = Input.GetAxis("move_left", "move_right");
		float yawDelta = turnInput * GlideTurnSpeed * delta;

		float targetYaw = Rotation.Y - yawDelta;
		float targetBank = Mathf.DegToRad(-turnInput * GlideBankAngle);
		float bankBlend = Mathf.Clamp(GlideBankSpeed * delta, 0f, 1f);
		float newBank = Mathf.Lerp(Rotation.Z, targetBank, bankBlend);

		Rotation = new Vector3(0f, targetYaw, newBank);

		Vector2 horiz = new Vector2(velocity.X, velocity.Z);
		float horizSpeed = horiz.Length();
		Vector3 forward = -Transform.Basis.Z;
		Vector2 forwardHoriz = new Vector2(forward.X, forward.Z);
		if (forwardHoriz.LengthSquared() > 0.0001f)
		{
			forwardHoriz = forwardHoriz.Normalized();
			velocity.X = forwardHoriz.X * horizSpeed;
			velocity.Z = forwardHoriz.Y * horizSpeed;
		}

		return velocity;
	}

	private void UpdateCamera(float delta)
	{
		if (_camera == null || _cameraPivot == null)
		{
			return;
		}

		float blend = Mathf.Clamp(CameraFollowSpeed * delta, 0f, 1f);
		_camera.Position = _camera.Position.Lerp(CameraOffset, blend);
		_cameraPivot.GlobalRotationDegrees = new Vector3(_cameraPitchDeg, _cameraYawDeg, 0f);
		_camera.LookAt(GlobalPosition + Vector3.Up * 1.5f, Vector3.Up);
	}
}
