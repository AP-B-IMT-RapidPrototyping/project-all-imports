using Godot;

public partial class LevelController : CanvasLayer
{
    private CanvasLayer _pauseMenu;
    private bool _isPaused = false;

// public override void _Ready()
// {
//     _pauseMenu = GetNode<CanvasLayer>("PauseMenu"); // ← exact zelfde naam als node!
//     _pauseMenu.Visible = false;
// }
    public override void _Ready()
    {
        _pauseMenu = GetTree().Root.FindChild("PauseMenu", true, false) as CanvasLayer;
        _pauseMenu.Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            GD.Print("ESC gedrukt!"); // debug
            TogglePause();
        }
    }

	private void TogglePause()
	{
		_isPaused = !_isPaused;
		_pauseMenu.Visible = _isPaused;  // false = verborgen
		GetTree().Paused = _isPaused;
		Input.MouseMode = _isPaused
			? Input.MouseModeEnum.Visible
			: Input.MouseModeEnum.Captured;
	}

	public void OnResumeButtonPressed()
	{
		TogglePause(); // ← dit verbergt het menu
	}

    public void OnBackToMenuButtonPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scene/Tutku/main_menu.tscn");
    }
}