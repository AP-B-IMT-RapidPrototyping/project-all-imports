using Godot;
using System;
using System.Collections.Generic;

public partial class GrabBehavior : Node
{
    [Export] public string GrabAction = "grab";

    private bool diagnoseGedaan = false;




    private Area3D grabArea;
    private Marker3D holdPosition;
    private PlayerMovement player;
    private RigidBody3D heldObject = null;
    private List<RigidBody3D> objectsInRange = new List<RigidBody3D>();

    public override void _Ready()
    {
        player = GetParent<PlayerMovement>();

        if (player != null)
        {
            grabArea = player.GetNodeOrNull<Area3D>("GrabArea");
            holdPosition = player.GetNodeOrNull<Marker3D>("HoldPosition");

            if (grabArea != null)
            {
                grabArea.BodyEntered += OnGrabAreaBodyEntered;
                grabArea.BodyExited += OnGrabAreaBodyExited;
            }
        }
    }

    public override void _Process(double delta)
    {
        // Check voor grab input
        if (Input.IsActionJustPressed(GrabAction))
        {
            if (heldObject == null)
            {
                TryGrab();
            }
            else
            {
                TryRelease();
            }
        }



    }

    private void TryGrab()
    {
        if (objectsInRange.Count == 0) return;

        // Pick the first in-range object that has a clear line of sight.
        // If an obstacle (wall, glass cover, ...) is between the player and the
        // object, it can't be picked up.
        RigidBody3D target = null;
        foreach (RigidBody3D candidate in objectsInRange)
        {
            if (IsInstanceValid(candidate) && HasLineOfSight(candidate))
            {
                target = candidate;
                break;
            }
        }

        if (target == null)
        {
            GD.Print("Geen grijpbaar object met vrij zicht (obstakel in de weg).");
            return;
        }

        if (IsInstanceValid(target))
        {
            StealableObject stealable = target as StealableObject;
            heldObject = target;

            GD.Print($"Target voor reparent: {target.Name}");
            GD.Print($"Target GlobalPosition voor: {target.GlobalPosition}");
            GD.Print($"holdPosition is: {(holdPosition != null ? holdPosition.Name : "NULL")}");

            if (holdPosition != null)
            {
                GD.Print($"holdPosition GlobalPosition: {holdPosition.GlobalPosition}");

                if (stealable != null)
                    stealable.PickUp();
                else
                {
                    target.Freeze = true;
                    target.CollisionLayer = 0;
                    target.CollisionMask = 0;
                }

                target.Reparent(holdPosition);
                target.Position = Vector3.Zero;
                target.Rotation = Vector3.Zero;

                GD.Print($"Target nieuwe GlobalPosition: {target.GlobalPosition}");
            }
            else
            {
                GD.PrintErr("KAN NIET OPPAKKEN: holdPosition is null!");
            }
        }

    }

    private void TryRelease()
    {
        if (heldObject == null) return;

        StealableObject stealable = heldObject as StealableObject;

        // Sla globale positie op vóór reparent
        Vector3 globalPos = heldObject.GlobalPosition;

        // Terug naar de wereld
        heldObject.Reparent(GetTree().CurrentScene);

        // Zet de globale positie terug
        heldObject.GlobalPosition = globalPos;

        // Gooi richting
        Vector3 throwDir = -player.Transform.Basis.Z * 3.0f;
        throwDir.Y = 2.0f; // Beetje omhoog gooien
        throwDir += player.Velocity; // Inherit player velocity

        if (stealable != null)
        {
            stealable.LetGo(throwDir);
        }
        else
        {
            heldObject.Freeze = false;
            heldObject.CollisionLayer = 1;
            heldObject.CollisionMask = 1;
            heldObject.ApplyCentralImpulse(throwDir);
        }

        GD.Print($"Losgelaten: {heldObject.Name}");
        heldObject = null;
    }

    // Casts a ray from the player to the target. If it hits anything before
    // reaching the target, there's an obstacle in the way and the object
    // shouldn't be grabbable.
    private bool HasLineOfSight(RigidBody3D target)
    {
        if (player == null) return true;

        var spaceState = player.GetWorld3D().DirectSpaceState;

        var query = PhysicsRayQueryParameters3D.Create(
            player.GlobalPosition,
            target.GlobalPosition);

        query.CollideWithAreas = false;
        query.CollideWithBodies = true;
        query.Exclude = new Godot.Collections.Array<Rid>
        {
            player.GetRid(),
            target.GetRid()
        };

        var result = spaceState.IntersectRay(query);

        // Empty result → nothing between player and target → clear sight.
        return result.Count == 0;
    }

    private void OnGrabAreaBodyEntered(Node3D body)
    {
        GD.Print($"Body entered: {body.Name}, Is in Pickable: {body.IsInGroup("Pickable")}");

        if (body.IsInGroup("Pickable") && body is RigidBody3D rb)
        {
            if (!objectsInRange.Contains(rb))
            {
                objectsInRange.Add(rb);
                GD.Print($"Toegevoegd aan lijst: {body.Name}. Aantal: {objectsInRange.Count}");
            }
        }
    }

    private void OnGrabAreaBodyExited(Node3D body)
    {
        GD.Print($"Body exited: {body.Name}");

        if (body is RigidBody3D rb && objectsInRange.Contains(rb))
        {
            objectsInRange.Remove(rb);
            GD.Print($"Verwijderd uit lijst: {body.Name}. Aantal: {objectsInRange.Count}");
        }
    }

    private void Diagnose()
    {
        GD.Print("=== DIAGNOSE START ===");

        // Wat is de parent?
        var parent = GetParent();
        if (parent != null)
        {
            GD.Print($"Mijn parent: {parent.Name} (type: {parent.GetType().Name})");

            // Lijst alle kinderen van de parent op
            GD.Print($"Kinderen van {parent.Name}:");
            foreach (Node child in parent.GetChildren())
            {
                GD.Print($"  - {child.Name} (type: {child.GetType().Name})");
            }
        }
        else
        {
            GD.PrintErr("IK HEB GEEN PARENT!");
        }

        GD.Print("=== DIAGNOSE EINDE ===");
    }
}