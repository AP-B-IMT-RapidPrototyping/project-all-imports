using Godot;
using System;

public partial class Draaideur : Node3D
{
    // Snelheid van het draaien in radialen per seconde
    [Export]
    public float DraaiSnelheid { get; set; } = 1.0f; 

    // Schakelknop om het draaien aan/uit te zetten
    [Export]
    public bool IsDraaiend { get; set; } = true;

    public override void _Process(double delta)
    {
        if (IsDraaiend)
        {
            // Roteer rond de Y-as (omhoog/omlaag as)
            // We vermenigvuldigen met delta voor een constante snelheid, ongeacht de framerate
            RotateY(DraaiSnelheid * (float)delta);
        }
    }
}