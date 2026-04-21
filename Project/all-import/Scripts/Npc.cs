using Godot;
using System;

public partial class Npc : CharacterBody3D
{
	private enum NpcState
	{
		Neutral,
		Alert
	}

	private const string PlayerGroupName = "player";

	[Export] public float Speed = 2.0f;
	[Export] public float CatchDistance = 1.5f;

	private NpcState _state = NpcState.Neutral;
	private Node3D _player;

	private Area3D _detectionArea;
	private Timer _alertTimer;

	public string StateName => _state.ToString();

	public override void _Ready()
	{
		_detectionArea = GetNodeOrNull<Area3D>("Detection");
		if (_detectionArea == null)
		{
			GD.PushWarning($"{Name}: Detection Area3D child 'Detection' was not found.");
			return;
		}

		_alertTimer = GetNodeOrNull<Timer>("AlertTimer");
		if (_alertTimer == null)
		{
			GD.PushWarning($"{Name}: Timer child 'AlertTimer' was not found.");
		}
		else
		{
			_alertTimer.Timeout += OnAlertTimerTimeout;
		}

		_detectionArea.BodyEntered += OnDetectionBodyEntered;
		_detectionArea.BodyExited += OnDetectionBodyExited;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (_state == NpcState.Alert && IsInstanceValid(_player))
		{
			//If player is within catch distance, catch player(debug msg)
			Vector3 offset = _player.GlobalPosition - GlobalPosition;
			float distanceToPlayer = new Vector2(offset.X, offset.Z).Length();
			if (distanceToPlayer <= CatchDistance)
			{
				//debug msg placeholder
				GD.Print($"{Name} caught the player, game over.");

			}
			else
			{
				MoveToward(_player.GlobalPosition, Speed, dt);

			}
			return;
		}

		MoveToward(GlobalPosition, 0.0f, dt);
	}

	private void OnDetectionBodyEntered(Node3D body)
	{
		if (!body.IsInGroup(PlayerGroupName))
		{
			return;
		}

		if (HasLineOfSight(body))
		{
			_player = body;
			StopAlertTimer();
			ChangeState(NpcState.Alert);
		}
	}

	private void OnDetectionBodyExited(Node3D body)
	{
		if (_player == body)
		{
			StartAlertTimer();
		}
	}

	private void OnAlertTimerTimeout()
	{
		if (_state != NpcState.Alert)
		{
			return;
		}

		_player = null;
		ChangeState(NpcState.Neutral);
	}

	private void StartAlertTimer()
	{
		if (_alertTimer == null)
		{
			_player = null;
			ChangeState(NpcState.Neutral);
			return;
		}

		_alertTimer.Start();
	}

	private void StopAlertTimer()
	{
		if (_alertTimer?.IsStopped() == false)
		{
			_alertTimer.Stop();
		}
	}

	private bool HasLineOfSight(Node3D target)
	{
		Vector3 from = GlobalPosition + Vector3.Up * 1.5f;
		Vector3 to = target.GlobalPosition + Vector3.Up * 1.0f;

		var query = new PhysicsRayQueryParameters3D
		{
			From = from,
			To = to
		};

		var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
		if (hit.Count == 0)
		{
			return false;
		}

		return hit.ContainsKey("collider") && hit["collider"].AsGodotObject() == target;
	}


	private void ChangeState(NpcState newState)
	{
		if (_state != newState)
		{
			_state = newState;
			GD.Print($"{Name} is now {_state}");
		}
	}


	private void MoveToward(Vector3 target, float speed, float dt)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * dt;
		}

		Vector3 offset = target - GlobalPosition;
		Vector3 direction = new Vector3(offset.X, 0.0f, offset.Z).Normalized();
		float stopDistance = _state == NpcState.Alert ? CatchDistance : 0.1f;
		bool shouldMove = new Vector2(offset.X, offset.Z).Length() > stopDistance;

		if (shouldMove && direction != Vector3.Zero)
		{
			LookAt(GlobalPosition + direction, Vector3.Up, true);
		}

		if (shouldMove && direction != Vector3.Zero)
		{
			velocity.X = direction.X * speed;
			velocity.Z = direction.Z * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0.0f, speed * dt);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0.0f, speed * dt);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
