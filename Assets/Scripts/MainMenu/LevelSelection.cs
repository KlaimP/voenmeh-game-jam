using Godot;

public partial class LevelSelection : Control
{
	[Export]
	public Button Level1Button { get; set; }
	
	[Export]
	public Button Level2Button { get; set; }
	
	[Export]
	public Button Level3Button { get; set; }
	
	[Export]
	public Button Level4Button { get; set; }
	
	[Export]
	public Button Level5Button { get; set; }
	
	[Export]
	public Button BackButton { get; set; }

	public override void _Ready()
	{
		// Подключаем сигналы кнопок уровней
		Level1Button.Pressed += () => OnLevelButtonPressed(1);
		Level2Button.Pressed += () => OnLevelButtonPressed(2);
		Level3Button.Pressed += () => OnLevelButtonPressed(3);
		Level4Button.Pressed += () => OnLevelButtonPressed(4);
		Level5Button.Pressed += () => OnLevelButtonPressed(5);
		
		BackButton.Pressed += OnBackButtonPressed;
	}

	private void OnLevelButtonPressed(int levelNumber)
	{
		GD.Print($"Загружается уровень {levelNumber}");
		
		// Здесь загружаем соответствующий уровень
		// Замените пути на ваши реальные сцены уровней
		switch (levelNumber)
		{
			case 1:
				GetTree().ChangeSceneToFile("res://Assets/Scripts/Levels/LvL1.tscn");
				break;
			case 2:
				GetTree().ChangeSceneToFile("res://Assets/Scripts/Levels/LvL1.tscn");
				break;
			case 3:
				GetTree().ChangeSceneToFile("res://Assets/Scripts/Levels/LvL1.tscn");
				break;
			case 4:
				GetTree().ChangeSceneToFile("res://Assets/Scripts/Levels/LvL1.tscn");
				break;
			case 5:
				GetTree().ChangeSceneToFile("res://Assets/Scripts/Levels/LvL1.tscn");
				break;
		}
	}

	private void OnBackButtonPressed()
	{
		// Возвращаемся в главное меню
		GetTree().ChangeSceneToFile("res://Assets/Scripts/MainMenu/MainMenu.tscn");
	}
}
