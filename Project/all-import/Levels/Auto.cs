using Godot;
using System;

public partial class Auto : Node3D
{
    private SpotLight3D leftLight;
    private SpotLight3D rightLight;

    private bool isBlinking = false;

    public override void _Ready()
    {
        leftLight = GetNode<SpotLight3D>("SpotLight3D");
        rightLight = GetNode<SpotLight3D>("SpotLight3D2");
    }

    private async void OnDamageAreaBodyEntered(Node3D body)
    {
        if (isBlinking)
            return;

        isBlinking = true;

        for (int i = 0; i < 6; i++)
        {
            leftLight.Visible = false;
            rightLight.Visible = false;

            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

            leftLight.Visible = true;
            rightLight.Visible = true;

            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
        }

        isBlinking = false;
    }
}