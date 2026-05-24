using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class CameraOcclusion : Camera3D
{
	[Export] public NodePath PlayerPath;

	// ShaderMaterial template (DitherFade.tres). A unique copy is duplicated
	// per occluded MeshInstance3D so each can fade independently.
	[Export] public ShaderMaterial DitherTemplate;

	// 1.0 = fully visible, 0.0 = invisible. When occluding the player, the
	// object's fade is driven toward this value.
	[Export(PropertyHint.Range, "0.0,1.0,0.01")] public float OccludedFade = 0.25f;

	[Export] public float FadeSpeed = 8f;

	// Physics collision mask used to detect blockers. Default = layer 1.
	[Export(PropertyHint.Layers3DPhysics)] public uint OcclusionMask = 1;

	// Hard cap on rays per frame so we don't melt if something weird happens.
	[Export] public int MaxRaySteps = 8;

	// Eye-height offset on the player so we aim at the head, not the feet.
	[Export] public Vector3 PlayerAimOffset = new Vector3(0f, 1.0f, 0f);

	private Node3D _player;

	private class Entry
	{
		public ShaderMaterial Mat;
		public float Current = 1f;
		public float Target = 1f;
		public bool SeenThisFrame;
	}

	private readonly System.Collections.Generic.Dictionary<MeshInstance3D, Entry> _entries = new();

	public override void _Ready()
	{
		if (PlayerPath != null && !PlayerPath.IsEmpty)
			_player = GetNodeOrNull<Node3D>(PlayerPath);

		// Fall back to walking up the tree to find the player CharacterBody3D.
		if (_player == null)
		{
			Node n = GetParent();
			while (n != null && _player == null)
			{
				if (n is CharacterBody3D) _player = (Node3D)n;
				n = n.GetParent();
			}
		}

		if (_player == null)
			GD.PushWarning("CameraOcclusion: player not found; set PlayerPath.");
		if (DitherTemplate == null)
			GD.PushWarning("CameraOcclusion: DitherTemplate not assigned.");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player == null || DitherTemplate == null) return;

		foreach (var e in _entries.Values) e.SeenThisFrame = false;

		FindBlockers();

		float deltaF = (float)delta;
		float blend = Mathf.Clamp(FadeSpeed * deltaF, 0f, 1f);

		var toRemove = new List<MeshInstance3D>();
		foreach (var kv in _entries)
		{
			Entry e = kv.Value;
			e.Target = e.SeenThisFrame ? OccludedFade : 1f;
			e.Current = Mathf.Lerp(e.Current, e.Target, blend);
			e.Mat.SetShaderParameter("fade", e.Current);

			// Once fully restored and no longer occluding, drop the override
			// so the original material renders normally again.
			if (!e.SeenThisFrame && e.Current > 0.995f)
				toRemove.Add(kv.Key);
		}

		foreach (var mesh in toRemove)
		{
			if (IsInstanceValid(mesh))
				mesh.MaterialOverride = null;
			_entries.Remove(mesh);
		}
	}

	private void FindBlockers()
	{
		PhysicsDirectSpaceState3D space = GetWorld3D().DirectSpaceState;

		Vector3 from = GlobalPosition;
		Vector3 to = _player.GlobalPosition + PlayerAimOffset;

		var exclude = new Array<Rid>();
		Rid playerRid = default;
		if (_player is CollisionObject3D playerCol)
		{
			playerRid = playerCol.GetRid();
			exclude.Add(playerRid);
		}

		for (int step = 0; step < MaxRaySteps; step++)
		{
			var query = PhysicsRayQueryParameters3D.Create(from, to, OcclusionMask, exclude);
			query.CollideWithBodies = true;
			query.CollideWithAreas = false;

			Dictionary hit = space.IntersectRay(query);
			if (hit.Count == 0) break;

			CollisionObject3D collider = hit["collider"].As<CollisionObject3D>();
			if (collider == null) break;
			if (collider.GetRid() == playerRid) break;

			MarkOccluded(collider);
			exclude.Add(collider.GetRid());
		}
	}

	private void MarkOccluded(CollisionObject3D collider)
	{
		// Walk up to the closest sensible "model root" — the collider's parent
		// usually holds both the StaticBody and the MeshInstance3Ds.
		Node root = collider.GetParent() ?? collider;
		CollectMeshes(root, collider);
	}

	private void CollectMeshes(Node node, CollisionObject3D owningCollider)
	{
		if (node is MeshInstance3D mesh && IsRenderedBy(mesh, owningCollider))
		{
			if (!_entries.TryGetValue(mesh, out Entry e))
			{
				e = new Entry
				{
					Mat = (ShaderMaterial)DitherTemplate.Duplicate(),
					Current = 1f,
				};
				e.Mat.SetShaderParameter("fade", 1f);
				mesh.MaterialOverride = e.Mat;
				_entries[mesh] = e;
			}
			e.SeenThisFrame = true;
		}

		// Recurse into Node3D children but stop at other CollisionObject3D
		// boundaries so we don't bleed into adjacent unrelated objects.
		foreach (Node child in node.GetChildren())
		{
			if (child is CollisionObject3D && child != owningCollider) continue;
			CollectMeshes(child, owningCollider);
		}
	}

	private static bool IsRenderedBy(MeshInstance3D mesh, CollisionObject3D collider)
	{
		// Heuristic: mesh and collider should share an ancestor within 3 hops.
		// Cheap guard against unrelated meshes parented elsewhere in the tree.
		Node a = mesh;
		for (int i = 0; i < 4 && a != null; i++)
		{
			Node b = collider;
			for (int j = 0; j < 4 && b != null; j++)
			{
				if (a == b) return true;
				b = b.GetParent();
			}
			a = a.GetParent();
		}
		return false;
	}
}
