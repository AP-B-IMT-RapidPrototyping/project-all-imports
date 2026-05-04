using Godot;
using System;

public partial class HighlightTarget : Node3D
{
    [Export] public string TargetName = "Doel";
    [Export] public Color HighlightColor = new Color(1.0f, 0.2f, 0.2f); // Rood
    [Export] public bool ShowIcon = true;
    
    private Sprite3D icon;
    private StealableObject stealable;
    private Glowable glowable;
    
    public override void _Ready()
    {
        stealable = GetParent<StealableObject>();
        glowable = GetParent<Glowable>();
        
        if (glowable != null && stealable != null && stealable.IsImportant)
        {
            glowable.GlowColor = HighlightColor;
        }
        
        if (ShowIcon)
        {
            icon = new Sprite3D();
            icon.Position = new Vector3(0, 2.0f, 0); 
            icon.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled; 
            icon.Modulate = HighlightColor;
            
            Label3D label = new Label3D();
            label.Text = "★ " + TargetName;
            label.Position = new Vector3(0, 0.5f, 0);
            label.Modulate = HighlightColor;
            label.FontSize = 32;
            icon.AddChild(label);
            
            AddChild(icon);
        }
    }
    
    public override void _Process(double delta)
    {
        if (stealable != null && stealable.IsBeingHeld && icon != null)
        {
            icon.Visible = false; 
        }
    }
}