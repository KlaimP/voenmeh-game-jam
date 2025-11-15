using Godot;

public partial class LevelScene : Node2D
{
	[Export] public Grid LevelGrid { get; set; }
	[Export] public PackedScene RobotPrefab { get; set; }
	[Export] public PackedScene BoxPrefab { get; set; }
	[Export] public PackedScene ObstaclePrefab { get; set; }

	private Node2D _objectsContainer;
	private Robot _robot;

	public override void _Ready()
	{
		_objectsContainer = GetNode<Node2D>("Objects");
		InitializeLevel();
		
		GD.Print("=== УПРАВЛЕНИЕ РОБОТОМ ===");
		GD.Print("Стрелка ВВЕРХ - Движение вперед");
		GD.Print("Стрелка ВНИЗ - Движение назад"); 
		GD.Print("Стрелка ВЛЕВО - Поворот налево");
		GD.Print("Стрелка ВПРАВО - Поворот направо");
		GD.Print("Page Up - Быстрое движение вперед");
		GD.Print("========================");
	}

	private void InitializeLevel()
	{
		if (LevelGrid == null) 
		{
			GD.PrintErr("LevelGrid не назначен в инспекторе!");
			return;
		}

		CreateRobot();
		CreateBoxes();
		CreateObstacles();
		CreateWalls();
		
		GD.Print("Уровень инициализирован!");
	}

	private void CreateRobot()
	{
		if (RobotPrefab != null)
		{
			_robot = RobotPrefab.Instantiate<Robot>();
			_objectsContainer.AddChild(_robot);
			LevelGrid.AddObjectToGrid(_robot, new Vector2I(2, 2));
			GD.Print("Робот создан в позиции (2, 2)");
		}
		else
		{
			GD.PrintErr("RobotPrefab не назначен!");
		}
	}

	private void CreateBoxes()
	{
		if (BoxPrefab != null)
		{
			// Создаем ящики в разных позициях
			var boxPositions = new Vector2I[] {
				new Vector2I(4, 3),  // Ящик 1
				new Vector2I(5, 4),  // Ящик 2
				new Vector2I(3, 5),  // Ящик 3
				new Vector2I(6, 3)   // Ящик 4
			};
			
			foreach (var pos in boxPositions)
			{
				CreateBoxAtPosition(pos);
			}
			
			GD.Print($"Создано {boxPositions.Length} ящиков");
		}
		else
		{
			GD.PrintErr("BoxPrefab не назначен!");
		}
	}

	private void CreateBoxAtPosition(Vector2I position)
	{
		var box = BoxPrefab.Instantiate<BoxObject>();
		_objectsContainer.AddChild(box);
		
		if (LevelGrid.AddObjectToGrid(box, position))
		{
			GD.Print($"Ящик создан в позиции {position}");
		}
		else
		{
			GD.PrintErr($"Не удалось создать ящик в позиции {position}");
			box.QueueFree(); // Удаляем если не удалось разместить
		}
	}

	private void CreateObstacles()
	{
		if (ObstaclePrefab != null)
		{
			// Создаем отдельные препятствия
			var obstaclePositions = new Vector2I[] {
				new Vector2I(1, 1),  // Препятствие 1
				new Vector2I(6, 2),  // Препятствие 2
				new Vector2I(3, 6),  // Препятствие 3
				new Vector2I(7, 5)   // Препятствие 4
			};
			
			foreach (var pos in obstaclePositions)
			{
				CreateObstacleAtPosition(pos);
			}
			
			GD.Print($"Создано {obstaclePositions.Length} препятствий");
		}
		else
		{
			GD.PrintErr("ObstaclePrefab не назначен!");
		}
	}

	private void CreateObstacleAtPosition(Vector2I position)
	{
		var obstacle = ObstaclePrefab.Instantiate<ObstacleObject>();
		_objectsContainer.AddChild(obstacle);
		
		if (LevelGrid.AddObjectToGrid(obstacle, position))
		{
			GD.Print($"Препятствие создано в позиции {position}");
		}
		else
		{
			GD.PrintErr($"Не удалось создать препятствие в позиции {position}");
			obstacle.QueueFree();
		}
	}

	private void CreateWalls()
	{
		if (ObstaclePrefab != null)
		{
			// Создаем стену внизу
			for (int x = 0; x < LevelGrid.GridWidth; x++)
			{
				var wallPosition = new Vector2I(x, LevelGrid.GridHeight - 1);
				CreateObstacleAtPosition(wallPosition);
			}
			
			// Создаем стену справа
			for (int y = 0; y < LevelGrid.GridHeight - 1; y++)
			{
				var wallPosition = new Vector2I(LevelGrid.GridWidth - 1, y);
				CreateObstacleAtPosition(wallPosition);
			}
			
			GD.Print("Стены созданы по границам уровня");
		}
	}
}
