using Godot;
using System;
using System.Collections.Generic;

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
	[Export] public bool RequireLineOfSightInCone = false;
	[Export] public bool TreatRayNoHitAsClearLos = true;
	[Export] public bool DebugDetection = true;
	[Export] public int DebugLogIntervalMs = 800;

	private NpcState _state = NpcState.Neutral;
	private Node3D _player;

	private Area3D _detectionArea;
	private Timer _alertTimer;
	private readonly Dictionary<string, long> _debugNextLogAtByKey = new();
	private bool _hasCaughtCurrentTarget;

	public string StateName => _state.ToString();

	public override void _Ready()
	{
		_detectionArea = GetNodeOrNull<Area3D>("Detection");
		if (_detectionArea == null)
		{
			GD.PushWarning($"{Name}: Detection Area3D child 'Detection' was not found.");
			DebugLog("ready_detection_missing", "Detection node missing. Fallback group scan only.");
		}
		else
		{
			_detectionArea.Monitoring = true;
			_detectionArea.Monitorable = true;
			_detectionArea.BodyEntered += OnDetectionBodyEntered;
			_detectionArea.BodyExited += OnDetectionBodyExited;
			DebugLog("ready_detection_ok", "Detection node found and monitoring enabled.");
		}

		_alertTimer = GetNodeOrNull<Timer>("AlertTimer");
		if (_alertTimer == null)
		{
			GD.PushWarning($"{Name}: Timer child 'AlertTimer' was not found.");
			DebugLog("ready_timer_missing", "AlertTimer missing. NPC will drop target instantly when LOS is lost.");
		}
		else
		{
			_alertTimer.Timeout += OnAlertTimerTimeout;
			DebugLog("ready_timer_ok", "AlertTimer connected.");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (!IsInstanceValid(_player))
		{
			if (_player != null)
			{
				DebugLog("player_invalid", "Current target became invalid and was cleared.");
			}
			_player = null;
			_hasCaughtCurrentTarget = false;
		}

		if (_player == null)
		{
			TryAcquireVisiblePlayerFromDetection();
			if (_player == null)
			{
				DebugLog("acquire_failed", "No valid player target acquired from Detection cone this frame.");
			}
		}

		if (_state == NpcState.Alert && IsInstanceValid(_player))
		{
			if (!IsInsideDetectionCone(_player))
			{
				DebugLog("cone_lost", $"Target '{_player.Name}' is outside Detection cone, starting alert timer.");
				StartAlertTimer();
				MoveToward(_player.GlobalPosition, Speed, dt);
				return;
			}

			if (RequireLineOfSightInCone && !HasLineOfSight(_player))
			{
				DebugLog("los_lost", $"Lost line of sight to target '{_player.Name}', starting alert timer.");
				StartAlertTimer();
				MoveToward(GlobalPosition, 0.0f, dt);
				return;
			}

			StopAlertTimer();

			Vector3 offset = _player.GlobalPosition - GlobalPosition;
			float distanceToPlayer = new Vector2(offset.X, offset.Z).Length();
			if (distanceToPlayer <= CatchDistance)
			{
				if (!_hasCaughtCurrentTarget)
				{
					GD.Print($"{Name} caught the player, game over.");
					_hasCaughtCurrentTarget = true;
				}
			}
			else
			{
				_hasCaughtCurrentTarget = false;
				MoveToward(_player.GlobalPosition, Speed, dt);
			}
			return;
		}

		MoveToward(GlobalPosition, 0.0f, dt);
	}

	private void OnDetectionBodyEntered(Node3D body)
	{
		DebugLog("body_entered", $"Detection body entered: '{body?.Name ?? "<null>"}'.");
		TrySetAlertTarget(body);
	}

	private void OnDetectionBodyExited(Node3D body)
	{
		DebugLog("body_exited", $"Detection body exited: '{body?.Name ?? "<null>"}'.");
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

		DebugLog("alert_timeout", "Alert timer expired, returning to Neutral.");
		_player = null;
		_hasCaughtCurrentTarget = false;
		ChangeState(NpcState.Neutral);
	}

	private void StartAlertTimer()
	{
		if (_alertTimer == null)
		{
			DebugLog("timer_start_missing", "Cannot start alert timer because AlertTimer is missing.");
			_player = null;
			_hasCaughtCurrentTarget = false;
			ChangeState(NpcState.Neutral);
			return;
		}

		if (_alertTimer.IsStopped())
		{
			DebugLog("timer_start", "Alert timer started.");
			_alertTimer.Start();
		}
	}

	private void StopAlertTimer()
	{
		if (_alertTimer?.IsStopped() == false)
		{
			DebugLog("timer_stop", "Alert timer stopped.");
			_alertTimer.Stop();
		}
	}

	private void TryAcquireVisiblePlayerFromDetection()
	{
		if (_detectionArea == null)
		{
			DebugLog("acquire_detection_missing", "Detection acquisition skipped: Detection node missing.");
			return;
		}

		var overlappingBodies = _detectionArea.GetOverlappingBodies();
		if (overlappingBodies.Count == 0)
		{
			DebugLog("acquire_detection_empty", "Detection acquisition found no overlapping bodies.");
			return;
		}

		foreach (Node3D body in overlappingBodies)
		{
			TrySetAlertTarget(body);
			if (_player == body)
			{
				DebugLog("acquire_detection_success", $"Acquired target from Detection: '{body.Name}'.");
				return;
			}
		}

		DebugLog("acquire_detection_no_valid", "Detection overlaps exist, but no valid visible player target was found.");
	}

	private void TrySetAlertTarget(Node3D body)
	{
		if (body == null)
		{
			DebugLog("set_target_null", "TrySetAlertTarget received null body.");
			return;
		}

		if (!body.IsInGroup(PlayerGroupName))
		{
			DebugLog("set_target_wrong_group", $"Body '{body.Name}' ignored: not in group '{PlayerGroupName}'.");
			return;
		}

		if (RequireLineOfSightInCone && !HasLineOfSight(body))
		{
			DebugLog("set_target_los_fail", $"Body '{body.Name}' failed line-of-sight check.");
			return;
		}

		_player = body;
		StopAlertTimer();
		ChangeState(NpcState.Alert);
		DebugLog("set_target_success", $"Target set to '{body.Name}'.");
	}

	private bool HasLineOfSight(Node3D target)
	{
		Vector3 from = GlobalPosition + Vector3.Up * 1.5f;
		Vector3 to = target.GlobalPosition + Vector3.Up * 1.0f;

		var query = new PhysicsRayQueryParameters3D
		{
			From = from,
			To = to,
			CollisionMask = uint.MaxValue
		};
		query.CollideWithAreas = false;
		query.CollideWithBodies = true;
		query.HitFromInside = true;

		query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
		if (_detectionArea != null)
		{
			query.Exclude.Add(_detectionArea.GetRid());
		}

		var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
		if (hit.Count == 0)
		{
			if (TreatRayNoHitAsClearLos)
			{
				DebugLog("los_no_hit_fallback", $"LOS ray hit nothing toward '{target?.Name ?? "<null>"}', treating as clear LOS fallback.");
				return true;
			}

			DebugLog("los_no_hit", $"LOS failed: ray hit nothing toward '{target?.Name ?? "<null>"}'.");
			return false;
		}

		if (!hit.ContainsKey("collider"))
		{
			DebugLog("los_no_collider", "LOS failed: ray result had no collider key.");
			return false;
		}

		var colliderObject = hit["collider"].AsGodotObject();
		if (colliderObject is not Node colliderNode)
		{
			DebugLog("los_collider_not_node", "LOS failed: collider was not a Node.");
			return false;
		}

		bool hasLine = colliderNode == target || target.IsAncestorOf(colliderNode);
		if (!hasLine)
		{
			DebugLog("los_blocked", $"LOS blocked. Hit '{colliderNode.Name}' instead of '{target.Name}'.");
			return false;
		}

		return true;
	}

	private bool IsInsideDetectionCone(Node3D body)
	{
		if (_detectionArea == null || body == null)
		{
			return false;
		}

		foreach (Node3D overlapped in _detectionArea.GetOverlappingBodies())
		{
			if (overlapped == body)
			{
				return true;
			}
		}

		return false;
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
			float deceleration = Mathf.Max(speed, Speed);
			velocity.X = Mathf.MoveToward(Velocity.X, 0.0f, deceleration * dt);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0.0f, deceleration * dt);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void DebugLog(string key, string message)
	{
		if (!DebugDetection)
		{
			return;
		}

		long now = (long)Time.GetTicksMsec();
		if (_debugNextLogAtByKey.TryGetValue(key, out long nextAllowedAt) && now < nextAllowedAt)
		{
			return;
		}

		_debugNextLogAtByKey[key] = now + Mathf.Max(50, DebugLogIntervalMs);
		GD.Print($"[NPC DEBUG:{Name}] {message}");
	}
}
