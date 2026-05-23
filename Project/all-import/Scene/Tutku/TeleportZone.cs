using Godot;

public partial class TeleportZone : Area3D
{
    // Sleep in de editor de Marker3D (je spawn punt) naar dit vakje
    [Export] public Marker3D DoelLocatie;

    public override void _Ready()
    {
        // Verbind het signaal via code als je het niet via de editor wilt doen
        BodyEntered += OnBodyEntered;
    }

private void OnBodyEntered(Node3D body)
{
    GD.Print("Iets raakte de schoorsteen: " + body.Name); // Dit komt in je Output venster
    
    if (body.IsInGroup("Player"))
    {
        GD.Print("De speler is gedetecteerd!");
        if (DoelLocatie != null)
        {
            body.GlobalPosition = DoelLocatie.GlobalPosition;
        }
    }
}
}