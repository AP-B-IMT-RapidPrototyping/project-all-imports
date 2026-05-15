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



        }
    }

    public override void _Process(double delta)
    {
        // TIJDELIJKE CHEAT: Scan ALLE RigidBody3D objecten in de scene
        objectsInRange.Clear();
        var allNodes = GetTree().CurrentScene.GetChildren();
        foreach (var node in allNodes)
        {
            if (node is RigidBody3D rb && rb.IsInGroup("Pickable"))
            {
                objectsInRange.Add(rb);
            }
        }

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

        RigidBody3D target = objectsInRange[0];

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
        Vector3 throwDir = -player.Transform.Basis.Z * 8.0f;
        throwDir.Y = 2.0f; // Beetje omhoog gooien

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