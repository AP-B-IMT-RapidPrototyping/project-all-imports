using Godot;
using System;

public partial class StealableObject : RigidBody3D
{
	[Export] public string ItemName = "Onbekend Item";
	[Export] public int Value = 10;
	[Export] public bool IsImportant = false;

	public bool IsBeingHeld { get; private set; } = false;

	public override void _Ready()
	{

		CanSleep = false;


		AddToGroup("Pickable");
	}


	public void PickUp()
	{
		IsBeingHeld = true;
		Freeze = true;
		CollisionLayer = 0;
		CollisionMask = 0;
	}


	public void LetGo(Vector3 throwDirection)
	{
		IsBeingHeld = false;
		Freeze = false;
		CollisionLayer = 1;
		CollisionMask = 1;

		if (throwDirection != Vector3.Zero)
		{
			ApplyCentralImpulse(throwDirection);
		}
	}
}