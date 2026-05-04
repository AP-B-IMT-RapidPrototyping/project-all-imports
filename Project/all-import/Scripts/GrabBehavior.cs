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

        if (objectsInRange.Count == 0) return;

        foreach (RigidBody3D obj in objectsInRange)
        {
            if (IsInstanceValid(obj) && obj is StealableObject stealable && !stealable.IsBeingHeld)
            {
                heldObject = obj;
                stealable.PickUp();

                heldObject.Reparent(holdPosition);
                heldObject.Position = Vector3.Zero;
                heldObject.Rotation = Vector3.Zero;

                GD.Print($"Opgepakt: {stealable.ItemName} (waarde: {stealable.Value})");
                return;
            }
        }
       
    }

    private void TryRelease()
    {
        if (heldObject == null) return;

        StealableObject stealable = heldObject as StealableObject;

        heldObject.Reparent(GetTree().CurrentScene);

        Vector3 throwDir = -player.Transform.Basis.Z * 5.0f;
        stealable.LetGo(throwDir);

        GD.Print($"Losgelaten: {stealable.ItemName}");
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