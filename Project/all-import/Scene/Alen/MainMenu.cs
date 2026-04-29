using Godot;
using System;

public partial class MainMenu : Control
{
    private Button startButton;
    private Button settingsButton;
    private Button quitButton;

    public override void _Ready()
    {
        startButton = GetNode<Button>("VBoxContainer/StartButton");
        settingsButton = GetNode<Button>("VBoxContainer/SettingsButton");
        quitButton = GetNode<Button>("VBoxContainer/QuitButton");

        startButton.Pressed += OnStartPressed;
        settingsButton.Pressed += OnSettingsPressed;
        quitButton.Pressed += OnQuitPressed;

        
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void OnStartPressed()
    {
        GD.Print("Start Game ingedrukt!");
    }

    private void OnSettingsPressed()
    {
        GD.Print("Settings ingedrukt!");
    }

    private void OnQuitPressed()
    {
        GD.Print("Quit ingedrukt!");
        GetTree().Quit();
    }
}
