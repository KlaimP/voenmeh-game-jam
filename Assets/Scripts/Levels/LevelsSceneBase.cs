using Godot;

public partial class LevelsSceneBase : Node
{
	// Защищенные поля для путей (доступны в наследниках)
	protected string MainMenuPath = "res://Assets/Scripts/MainMenu/MainMenu.tscn";
	protected string LevelsFolderPath = "res://Assets/Scripts/Levels/";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Виртуальная функция перезапуска уровня (будет переопределена в дочерних классах)
	public virtual void RestartLevel()
	{
		GD.Print("Базовый метод перезапуска уровня");
	}

}
