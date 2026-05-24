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
		public Material OriginalOverride;
		public float Current = 1f;
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
		float step = FadeSpeed * deltaF;

		var toRemove = new List<MeshInstance3D>();
		foreach (var kv in _entries)
		{
			Entry e = kv.Value;
			if (e.SeenThisFrame)
			{
				// Fade *out* smoothly toward OccludedFade while occluding.
				e.Current = Mathf.MoveToward(e.Current, OccludedFade, step);
				e.Mat.SetShaderParameter("fade", e.Current);
			}
			else
			{
				// No longer occluding — snap-restore. Fading back *in* would
				// leave the mesh visibly stuck in flat-grey override territory
				// (fade > ~0.94 means the dither matrix discards nothing).
				toRemove.Add(kv.Key);
			}
		}

		foreach (var mesh in toRemove)
		{
			if (_entries.TryGetValue(mesh, out Entry e) && IsInstanceValid(mesh))
				mesh.MaterialOverride = e.OriginalOverride;
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
		// Find the smallest ancestor of the collider whose subtree contains a
		// mesh without crossing into another collider's territory. Three common
		// model layouts to handle:
		//   1) Collider has mesh inside its own subtree.
		//   2) Mesh is the collider's parent (e.g. Box2424/StaticBody3D).
		//   3) Mesh is a sibling of the collider under a shared model root.
		// We refuse to walk past a parent that has *other* collider siblings —
		// that's the boundary between unrelated level objects (e.g. the bare
		// StaticBody3D ground at the level root with houses next to it).
		Node cur = collider;
		while (cur != null)
		{
			int before = _seenCount;
			CollectMeshes(cur, collider);
			if (_seenCount > before) return;

			Node parent = cur.GetParent();
			if (parent == null) return;
			if (HasOtherColliderSibling(parent, cur)) return;
			cur = parent;
		}
	}

	private int _seenCount;

	private void CollectMeshes(Node node, CollisionObject3D owningCollider)
	{
		if (node is MeshInstance3D mesh)
		{
			if (!_entries.TryGetValue(mesh, out Entry e))
			{
				e = new Entry
				{
					Mat = (ShaderMaterial)DitherTemplate.Duplicate(),
					OriginalOverride = mesh.MaterialOverride,
					Current = 1f,
				};
				e.Mat.SetShaderParameter("fade", 1f);
				mesh.MaterialOverride = e.Mat;
				_entries[mesh] = e;
			}
			if (!e.SeenThisFrame)
			{
				e.SeenThisFrame = true;
				_seenCount++;
			}
		}

		// Recurse, but stop at any *other* CollisionObject3D — that's a foreign
		// model and its meshes shouldn't ride along with ours.
		foreach (Node child in node.GetChildren())
		{
			if (child is CollisionObject3D && child != owningCollider) continue;
			CollectMeshes(child, owningCollider);
		}
	}

	// True iff `parent` has any direct child that is — or contains — a
	// CollisionObject3D other than `self`. Used to detect when walking up
	// would expose us to unrelated colliders' meshes.
	private static bool HasOtherColliderSibling(Node parent, Node self)
	{
		foreach (Node child in parent.GetChildren())
		{
			if (child == self) continue;
			if (HasColliderInSubtree(child)) return true;
		}
		return false;
	}

	private static bool HasColliderInSubtree(Node node)
	{
		if (node is CollisionObject3D) return true;
		foreach (Node child in node.GetChildren())
			if (HasColliderInSubtree(child)) return true;
		return false;
	}
}
