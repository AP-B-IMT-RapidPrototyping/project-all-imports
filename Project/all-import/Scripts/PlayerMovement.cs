// TODO: Stamina
// public float
// min=0.f, max=1.f (percentage, easier to draw bar in ui later)
// Pressing space (jump/flap) consume stamina
// Gliding reduce/pause stamina regen
// 
//
// Goal: 
// 	Player should maintain their level of stamina if they maintain their height,
// i.e. moderate flapping when gliding, height doesn't change;
// pulling up drain stamina quickly.
// 	Limit player reachable height not with invis wall ceiling
// but by limiting stamina.





using Godot;

public partial class PlayerMovement : CharacterBody3D
{
	[Export] public float MouseSensitivity = 0.2f;

	[Export] public float Speed = 5.0f;
	[Export] public float GlideSpeed = 8.0f;
	[Export] public float JumpVelocity = 4.0f;
	[Export] public float AirJumpVelocity = 2.0f;
	[Export] public float JumpCooldown = 0.2f;
	[Export] public float GlideFallSpeed = .3f;
	[Export] public float GlideLerpSpeed = 5.0f;
	[Export] public float GlideTurnSpeed = 2.5f;
	[Export] public float GlideBankAngle = 22f;
	[Export] public float GlideBankSpeed = 5f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float GlideGravityMultiplier = 0.3f;

	[Export] public float TurnSpeed = 4.5f;

	[Export] public float CameraPitchLimit = 70f;
	[Export] public Vector3 CameraOffset = new Vector3(0f, 2f, 8f);
	[Export] public float CameraFollowSpeed = 5f;

	[Export] public float VisualTiltMaxAngleDeg = 50f;
	[Export] public float VisualTiltVelocityRange = 6f;
	[Export] public float VisualTiltLerpSpeed = 8f;

	private float _cameraYawDeg;
	private float _cameraPitchDeg;
	private float _jumpCooldownTimer;

	private Node3D _cameraPivot;
	private Camera3D _camera;
	private Node3D _visualTilt;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_cameraPivot = GetNode<Node3D>("CameraPivot");
		_camera = GetNode<Camera3D>("CameraPivot/Camera3D");
		_camera.MakeCurrent();
		_cameraYawDeg = Mathf.RadToDeg(Rotation.Y);
		_visualTilt = GetNodeOrNull<Node3D>("VisualTilt");
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
		bool jumpHeld = Input.IsActionPressed("jump");
		bool jumpPressed = Input.IsActionJustPressed("jump");
		bool gliding = jumpHeld && !onFloor;

		if (!onFloor)
		{
			float gravityMul = gliding ? GlideGravityMultiplier : 1f;
			velocity += GetGravity() * deltaF * gravityMul;
		}
		else if (velocity.Y < 0f)
		{
			velocity.Y = 0f;
		}

		if (jumpPressed && _jumpCooldownTimer <= 0f)
		{
			velocity.Y += onFloor ? JumpVelocity : AirJumpVelocity;
			_jumpCooldownTimer = JumpCooldown;
		}

		if (gliding)
		{
			if (velocity.Y < -GlideFallSpeed)
			{
				velocity.Y = Mathf.Lerp(velocity.Y, -GlideFallSpeed,
					Mathf.Clamp(GlideLerpSpeed * deltaF, 0f, 1f));
			}
			velocity = ProcessGlideHorizontal(velocity, deltaF);
		}
		else
		{
			velocity = ProcessGroundedHorizontal(velocity, deltaF, onFloor);
		}

		Velocity = velocity;
		MoveAndSlide();

		UpdateCamera(deltaF);
		UpdateVisualTilt(deltaF, velocity);
	}

	private Vector3 ProcessGroundedHorizontal(Vector3 velocity, float delta, bool onFloor)
	{
		float bankBlend = Mathf.Clamp(GlideBankSpeed * delta, 0f, 1f);
		float clearedBank = Mathf.Lerp(Rotation.Z, 0f, bankBlend);

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "forward_up", "backwards_down");

		float referenceYawRad = onFloor ? Mathf.DegToRad(_cameraYawDeg) : Rotation.Y;
		Vector3 refRight = new Vector3(Mathf.Cos(referenceYawRad), 0f, -Mathf.Sin(referenceYawRad));
		Vector3 refBack = new Vector3(Mathf.Sin(referenceYawRad), 0f, Mathf.Cos(referenceYawRad));
		Vector3 direction = (refRight * inputDir.X + refBack * inputDir.Y).Normalized();

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
		horizSpeed = Mathf.Lerp(horizSpeed, GlideSpeed, Mathf.Clamp(GlideLerpSpeed * delta, 0f, 1f));

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

	private void UpdateVisualTilt(float delta, Vector3 velocity)
	{
		if (_visualTilt == null)
		{
			return;
		}

		float normalizedY = Mathf.Clamp(velocity.Y / VisualTiltVelocityRange, -1f, 1f);
		float targetPitch = Mathf.DegToRad(normalizedY * VisualTiltMaxAngleDeg);
		float blend = Mathf.Clamp(VisualTiltLerpSpeed * delta, 0f, 1f);
		Vector3 current = _visualTilt.Rotation;
		float newPitch = Mathf.Lerp(current.X, targetPitch, blend);
		_visualTilt.Rotation = new Vector3(newPitch, 0f, 0f);
	}
}
