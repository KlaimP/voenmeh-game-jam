using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		var playButton = GetNode<Button>("VBoxContainer/PlayButton");
		var exitButton = GetNode<Button>("VBoxContainer/ExitButton");

		playButton.Pressed += OnPlayButtonPressed;
		exitButton.Pressed += OnExitButtonPressed;
	}

	private void OnPlayButtonPressed()
	{
		// Загружаем сцену выбора уровня
		GetTree().ChangeSceneToFile("res://Assets/Scripts/MainMenu/LevelSelection.tscn");
	}

	private void OnExitButtonPressed()
	{
		GetTree().Quit();
	}
}
