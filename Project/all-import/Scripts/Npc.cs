using Godot;

public partial class Npc : CharacterBody3D
{
	// ── Signals ───────────────────────────────────────────────────────────────
	[Signal] public delegate void CaughtPlayerEventHandler();

	private enum NpcState { Neutral, Alert }

	// ── Inspector exports ────────────────────────────────────────────────────
	[Export] public float Speed        = 2.0f;
	[Export] public float CatchDistance = 1.5f;
	[Export] public bool  DebugLos     = true;

	// ── Runtime state ────────────────────────────────────────────────────────
	private NpcState _state  = NpcState.Neutral;
	private Node3D   _target;
	private bool     _hasCaught;

	private Area3D _detectionArea;
	private Area3D _catchArea;
	private Timer  _alertTimer;

	// ────────────────────────────────────────────────────────────────────────
	public override void _Ready()
	{
		_detectionArea = GetNodeOrNull<Area3D>("Detection");
		if (_detectionArea == null)
			GD.PushWarning($"{Name}: 'Detection' Area3D child not found.");

		_catchArea = GetNodeOrNull<Area3D>("Catch");
		if (_catchArea != null)
			_catchArea.BodyEntered += OnCatchBodyEntered;

		_alertTimer = GetNodeOrNull<Timer>("AlertTimer");
		if (_alertTimer != null)
			_alertTimer.Timeout += OnAlertTimerTimeout;
		else
			GD.PushWarning($"{Name}: 'AlertTimer' child not found.");
	}

	private void OnCatchBodyEntered(Node3D body)
	{
		if (_state == NpcState.Alert && !_hasCaught && body.IsInGroup("player"))
		{
			GD.Print($"{Name} caught the player in 'Catch' area!");
			_hasCaught = true;
			EmitSignal(SignalName.CaughtPlayer);
		}
	}

	// ────────────────────────────────────────────────────────────────────────
	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// Invalidate stale target
		if (_target != null && !IsInstanceValid(_target))
		{
			_target    = null;
			_hasCaught = false;
		}

		switch (_state)
		{
			case NpcState.Neutral:
				NeutralTick(dt);
				break;

			case NpcState.Alert:
				AlertTick(dt);
				break;
		}
	}

	// ── State ticks ──────────────────────────────────────────────────────────
	private void NeutralTick(float dt)
	{
		_target = FindVisiblePlayer();
		if (_target != null)
		{
			EnterAlert();
		}
		else
		{
			Decelerate(dt);
		}
	}

	private void AlertTick(float dt)
	{
		if (_target == null)
		{
			// No target at all — wait for timer
			Decelerate(dt);
			return;
		}

		bool inCone = IsBodyInCone(_target);
		bool hasLos = HasLineOfSight(_target);

		if (inCone && hasLos)
		{
			// Clear sight: stop grace timer and chase
			_alertTimer?.Stop();
			ChaseTarget(dt);
		}
		else
		{
			// Lost sight: start grace timer and keep chasing last known position
			StartAlertTimer();
			ChaseTarget(dt);
		}
	}

	// ── Helpers ──────────────────────────────────────────────────────────────

	/// Scan the detection cone for the first player body we have LOS to.
	private Node3D FindVisiblePlayer()
	{
		if (_detectionArea == null) return null;

		foreach (Node3D body in _detectionArea.GetOverlappingBodies())
		{
			if (!body.IsInGroup("player")) continue;
			if (HasLineOfSight(body)) return body;
		}
		return null;
	}

	/// True if <paramref name="body"/> is currently inside the detection cone.
	private bool IsBodyInCone(Node3D body)
	{
		if (_detectionArea == null || body == null) return false;
		foreach (Node3D b in _detectionArea.GetOverlappingBodies())
			if (b == body) return true;
		return false;
	}

	/// <summary>
	/// Casts a ray from the NPC's eye level toward the target's torso.
	/// Returns true when the path is clear:
	///   • ray hits nothing  → no obstacle, clear path
	///   • ray hits target   → player spotted
	///   • ray hits something else → blocked
	/// </summary>
	private bool HasLineOfSight(Node3D target)
	{
		if (!IsInstanceValid(target)) return false;

		// NPC eye ≈ 1.4 m, player torso ≈ 0.8 m (avoids clipping floor)
		Vector3 from = GlobalPosition + Vector3.Up * 1.4f;
		Vector3 to   = target.GlobalPosition + Vector3.Up * 0.8f;

		var query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithAreas   = false;
		query.CollideWithBodies  = true;
		query.HitFromInside      = true;
		// Only exclude this NPC's own physics body so we never self-block
		query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

		var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);

		// ── No hit ───────────────────────────────────────────────────────────
		// Nothing blocked the ray → the path between the two points is open.
		if (hit.Count == 0)
		{
			Log($"LOS clear (open path) → '{target.Name}'");
			return true;
		}

		// ── Something was hit ────────────────────────────────────────────────
		if (hit.TryGetValue("collider", out var cv) && cv.AsGodotObject() is Node hitNode)
		{
			// The ray hit the player body directly, or a child of it
			if (hitNode == target || target.IsAncestorOf(hitNode) || hitNode.IsInGroup("player"))
			{
				Log($"LOS clear (hit player) → '{target.Name}'");
				return true;
			}

			Log($"LOS blocked by '{hitNode.Name}' → '{target.Name}'");
			return false;
		}

		Log($"LOS blocked (unknown collider) → '{target.Name}'");
		return false;
	}

	// ── State transitions ─────────────────────────────────────────────────────
	private void EnterAlert()
	{
		_hasCaught = false;
		_alertTimer?.Stop();
		ChangeState(NpcState.Alert);
	}

	private void OnAlertTimerTimeout()
	{
		GD.Print($"[NPC:{Name}] Alert timer expired → Neutral");
		_target    = null;
		_hasCaught = false;
		ChangeState(NpcState.Neutral);
	}

	private void StartAlertTimer()
	{
		if (_alertTimer == null)
		{
			// No timer node — drop target immediately
			_target    = null;
			_hasCaught = false;
			ChangeState(NpcState.Neutral);
			return;
		}

		if (_alertTimer.IsStopped())
		{
			Log("Alert timer started.");
			_alertTimer.Start();
		}
	}

	private void ChangeState(NpcState next)
	{
		if (_state == next) return;
		_state = next;
		GD.Print($"{Name} → {_state}");
	}

	// ── Movement ──────────────────────────────────────────────────────────────
	private void ChaseTarget(float dt)
	{
		Vector3 offset = _target.GlobalPosition - GlobalPosition;
		float   dist2d = new System.Numerics.Vector2(offset.X, offset.Z).Length();

		if (dist2d <= CatchDistance)
		{
			// Simulate jumping to catch the bird
			if (IsOnFloor())
			{
				Vector3 vel = Velocity;
				vel.Y = 6.0f; // Jump impulse
				Velocity = vel;
			}
			ApplyMove(_target.GlobalPosition, Speed, dt);
		}
		else
		{
			ApplyMove(_target.GlobalPosition, Speed, dt);
		}
	}

	private void Decelerate(float dt) => ApplyMove(GlobalPosition, 0f, dt);

	private void ApplyMove(Vector3 targetPos, float speed, float dt)
	{
		Vector3 vel = Velocity;

		if (!IsOnFloor())
			vel += GetGravity() * dt;

		Vector3 offset = targetPos - GlobalPosition;
		Vector3 dir    = new Vector3(offset.X, 0f, offset.Z).Normalized();
		float   dist2d = new System.Numerics.Vector2(offset.X, offset.Z).Length();

		if (speed > 0f && dist2d > 0.05f && dir != Vector3.Zero)
		{
			LookAt(GlobalPosition + dir, Vector3.Up, true);
			vel.X = dir.X * speed;
			vel.Z = dir.Z * speed;
		}
		else
		{
			float decel = Speed > 0f ? Speed : 4f;
			vel.X = Mathf.MoveToward(vel.X, 0f, decel * dt * 8f);
			vel.Z = Mathf.MoveToward(vel.Z, 0f, decel * dt * 8f);
		}

		Velocity = vel;
		MoveAndSlide();
	}

	// ── Debug ─────────────────────────────────────────────────────────────────
	private void Log(string msg)
	{
		if (DebugLos) GD.Print($"[NPC:{Name}] {msg}");
	}
}
