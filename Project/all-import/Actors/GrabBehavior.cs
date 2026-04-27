using Godot;
using System;
using System.Collections.Generic;

public partial class GrabBehavior : Node
{
    [Export] public string GrabAction = "grab"; 

    
    private Area3D grabArea;
    private Marker3D holdPosition;
    private Player player; 
    private RigidBody3D heldObject = null;
    private List<RigidBody3D> objectsInRange = new List<RigidBody3D>();

    public override void _Ready()
    {
        
        player = GetParent<Player>();

        
        grabArea = GetNode<Area3D>("../GrabArea");
        holdPosition = GetNode<Marker3D>("../HoldPosition");

        
        grabArea.BodyEntered += OnGrabAreaBodyEntered;
        grabArea.BodyExited += OnGrabAreaBodyExited;

        GD.Print("GrabBehavior is klaar! Druk op SPACE om te grijpen/loslaten.");
    }

    public override void _Process(double delta)
    {
        // Check voor input
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
        if (objectsInRange.Count == 0)
        {
            GD.Print("Niks in de buurt om op te pakken.");
            return;
        }

       
        RigidBody3D target = objectsInRange[0];

        
        if (IsInstanceValid(target))
        {
            heldObject = target;

            
            heldObject.Freeze = true;
            
            heldObject.CollisionLayer = 0;
            heldObject.CollisionMask = 0;

            
            heldObject.Reparent(holdPosition);
            heldObject.Position = Vector3.Zero;
            heldObject.Rotation = Vector3.Zero;

            GD.Print($"Opgepakt: {heldObject.Name}");
        }
        else
        {
            objectsInRange.Remove(target);
        }
    }

    private void TryRelease()
    {
        if (heldObject == null) return;

        
        heldObject.Reparent(GetTree().CurrentScene);

        
        heldObject.CollisionLayer = 1;
        heldObject.CollisionMask = 1;
        heldObject.Freeze = false;

        
        Vector3 throwDir = -player.Transform.Basis.Z * 5.0f;
        heldObject.ApplyCentralImpulse(throwDir);

        GD.Print($"Losgelaten: {heldObject.Name}");
        heldObject = null;
    }

    private void OnGrabAreaBodyEntered(Node3D body)
    {
        
        if (body.IsInGroup("Pickable") && body is RigidBody3D rb)
        {
            if (!objectsInRange.Contains(rb))
            {
                objectsInRange.Add(rb);
                GD.Print($"Kan oppakken: {body.Name}");
            }
        }
    }

    private void OnGrabAreaBodyExited(Node3D body)
    {
        if (body is RigidBody3D rb && objectsInRange.Contains(rb))
        {
            objectsInRange.Remove(rb);
            GD.Print($"Niet meer in bereik: {body.Name}");
        }
    }
}