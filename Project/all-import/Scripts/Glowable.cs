using Godot;
using System;

public partial class Glowable : Node3D
{
    [Export] public float GlowRange = 5.0f;
    [Export] public Color GlowColor = new Color(1.0f, 0.8f, 0.0f); 
    
    private ShaderMaterial glowMaterial;
    private Player player;
    private bool isGlowing = false;

    public override void _Ready()
    {
        player = GetTree().CurrentScene.GetNode<Player>("Player");
        
        MeshInstance3D mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        if (mesh != null && mesh.Mesh.SurfaceGetMaterial(0) is ShaderMaterial sm)
        {
            glowMaterial = sm;
        }
    }

    public override void _Process(double delta)
    {
        if (player == null || glowMaterial == null) return;
        
        float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
        float targetStrength = distance < GlowRange ? 1.0f - (distance / GlowRange) : 0.0f;
        
        float currentStrength = (float)glowMaterial.GetShaderParameter("glow_strength");
        float newStrength = Mathf.Lerp(currentStrength, targetStrength, 5.0f * (float)delta);
        
        glowMaterial.SetShaderParameter("glow_strength", newStrength);
    }
}