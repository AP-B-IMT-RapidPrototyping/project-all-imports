using Godot;
using System;

// De naam van de class MOET exact 'MainMenuController' zijn!
public partial class MainMenuController : Node3D
{
    [Export]
    public string GameScenePath = "res://Levels/TestScene.tscn";

    public override void _Ready()
    {
        // 1. Muis zichtbaar en vrij maken
        Input.MouseMode = Input.MouseModeEnum.Visible;

        // 2. Level camera uitzetten (pas het pad aan naar jouw camera naam)
        var levelCamera = GetNodeOrNull<Camera3D>("TestScene/Camera3D");
        if (levelCamera != null)
            levelCamera.Current = false;

        // 3. Menu camera aanzetten (zorg dat je een Camera3D hebt in je menu scene)
        var menuCamera = GetNodeOrNull<Camera3D>("Camera3D");
        if (menuCamera != null)
            menuCamera.Current = true;

        // 4. Speler disablen (pas het pad aan naar jouw speler naam)
        var player = GetNodeOrNull<CharacterBody3D>("TestScene/Player");
        if (player != null)
        {
            player.Visible = false;
            player.ProcessMode = ProcessModeEnum.Disabled;
        }
    }

    public void OnStartButtonPressed()
    {
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        if (sceneTree != null)
        {
            sceneTree.ChangeSceneToFile(GameScenePath);
        }
    }

    public void OnOptionsButtonPressed()
    {
        GD.Print("Opties geopend via C#!");
    }

    public void OnExitButtonPressed()
    {
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        if (sceneTree != null)
        {
            sceneTree.Quit();
        }
    }
}


// using Godot;
// using System;

// // De naam van de class MOET nu exact 'MainMenuController' zijn!
// public partial class MainMenuController : Node3D
// {
//     [Export]
//     public string GameScenePath = "res://jouw_echte_game_level.tscn";

//     public void OnStartButtonPressed()
//     {
//         var sceneTree = Engine.GetMainLoop() as SceneTree;
//         if (sceneTree != null)
//         {
//             sceneTree.ChangeSceneToFile(GameScenePath);
//         }
//     }

//     public void OnOptionsButtonPressed()
//     {
//         GD.Print("Opties geopend via C#!");
//     }

//     public void OnExitButtonPressed()
//     {
//         var sceneTree = Engine.GetMainLoop() as SceneTree;
//         if (sceneTree != null)
//         {
//             sceneTree.Quit();
//         }
//     }
// }