using Godot;

/* Уровень 1 */
public partial class LevelScene : Node2D
{
	// Сетка 
	[Export] public Grid LevelGrid { get; set; }
	// Робот (Префаб)
	[Export] public PackedScene RobotPrefab { get; set; }
	// Ящик (Префаб)
	[Export] public PackedScene BoxPrefab { get; set; }
	// Стенка (Префаб)
	[Export] public PackedScene ObstaclePrefab { get; set; }
	// Ловушка - Пила (Префаб)
	[Export] public PackedScene SawTrapPrefab { get; set; }
	// IDE Робота
	[Export] public BlockEditorUi blockEditorUi { get; set; }

	// Контейнер для объектов
    private Node2D _objectsContainer;
    
    // Массивы позиций для всех объектов уровня
    private Vector2I _robotPosition = new Vector2I(2, 2);
    private Vector2I[] _boxPositions = [ 
        new Vector2I(4, 3), 
        new Vector2I(5, 4), 
        new Vector2I(3, 5) 
    ];
    private Vector2I[] _obstaclePositions = [ 
        new Vector2I(1, 1), 
        new Vector2I(6, 2), 
        new Vector2I(3, 6) 
	];
    private Vector2I[] _sawTrapPositions = [ 
        new Vector2I(2, 3), 
        new Vector2I(6, 6), 
        new Vector2I(4, 6) 
    ];



	// Инициализация уровня
	public override void _Ready()
	{
		GD.Print("=== ЗАПУСК УРОВНЯ ===");
		
		if (LevelGrid == null)
		{
			GD.PrintErr("ОШИБКА: LevelGrid не назначен в инспекторе!");
			return;
		}

		if (RobotPrefab == null || BoxPrefab == null || ObstaclePrefab == null || SawTrapPrefab == null)
		{
			GD.PrintErr("ОШИБКА: Не все префабы назначены в инспекторе!");
			return;
		}

		_objectsContainer = GetNode<Node2D>("Objects");
		InitializeLevel();
		
		GD.Print("=== УПРАВЛЕНИЕ ===");
		GD.Print("Стрелка ВВЕРХ - Движение вперед");
		GD.Print("Стрелка ВНИЗ - Движение назад");
		GD.Print("Стрелка ВЛЕВО - Поворот налево");
		GD.Print("Стрелка ВПРАВО - Поворот направо");
		GD.Print("Page Up - Показать матрицу состояния");
		GD.Print("==================");
	}
	// Основная функция инициализации/перезапуска уровня
    private void InitializeLevel()
    {
        ClearLevel(); // Очищаем перед созданием
        
        GD.Print("=== НАЧАЛО ИНИЦИАЛИЗАЦИИ УРОВНЯ ===");
        
        GD.Print("Создание робота...");
        CreateRobot(_robotPosition);
        
        GD.Print("Создание ящиков...");
        CreateBoxes(_boxPositions);
        
        GD.Print("Создание препятствий...");
        CreateObstacles(_obstaclePositions);
        
        GD.Print("Создание ловушек...");
        CreateSawTraps(_sawTrapPositions);
        
        LevelGrid.PrintStateMatrix("ФИНАЛЬНОЕ СОСТОЯНИЕ");
    }

	// Очистка уровня для перезапуска
    private void ClearLevel()
    {
        GD.Print("Очистка уровня...");
        LevelGrid.ClearGrid();
        blockEditorUi.Robot = null;
        GD.Print("Уровень очищен");
    }

	// Функция перезапуска уровня (можно вызвать из кнопки)
    public void RestartLevel()
    {
        GD.Print("=== ПЕРЕЗАПУСК УРОВНЯ ===");
        InitializeLevel();
    }

	// Создание робота
    private void CreateRobot(Vector2I position)
    {
        var robot = RobotPrefab.Instantiate<Robot>();
        _objectsContainer.AddChild(robot);

        blockEditorUi.Robot = robot;

        CallDeferred(nameof(DeferredAddRobot), robot, position);
    }
	private void DeferredAddRobot(Robot robot, Vector2I position)
    {
        if (LevelGrid.AddObjectToGrid(robot, position))
        {
            GD.Print($"Робот создан в позиции {position}");
            robot.UpdateWorldPositionImmediately();
        }
        else
        {
            GD.PrintErr($"Не удалось создать робота в {position}!");
        }
    }

	/* ------------ Функции добавления объектов сцены ------------ */
	// Создание ящиков
	private void CreateBoxes(Vector2I[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            var box = BoxPrefab.Instantiate<BoxObject>();
            _objectsContainer.AddChild(box);
            
            CallDeferred(nameof(DeferredAddBox), box, positions[i], i + 1);
        }
    }
	private void DeferredAddBox(BoxObject box, Vector2I position, int boxNumber)
    {
        if (LevelGrid.AddObjectToGrid(box, position))
        {
            GD.Print($"Ящик {boxNumber} создан в позиции {position}");
            box.UpdateWorldPositionImmediately();
        }
        else
        {
            GD.PrintErr($"Не удалось создать ящик {boxNumber} в {position}");
        }
    }

	// Создание препятствий (стенки)
	private void CreateObstacles(Vector2I[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            var obstacle = ObstaclePrefab.Instantiate<ObstacleObject>();
            _objectsContainer.AddChild(obstacle);
            
            CallDeferred(nameof(DeferredAddObstacle), obstacle, positions[i], i + 1);
        }
    }
	private void DeferredAddObstacle(ObstacleObject obstacle, Vector2I position, int obstacleNumber)
    {
        if (LevelGrid.AddObjectToGrid(obstacle, position))
        {
            GD.Print($"Препятствие {obstacleNumber} создано в позиции {position}");
            obstacle.UpdateWorldPositionImmediately();
        }
        else
        {
            GD.PrintErr($"Не удалось создать препятствие {obstacleNumber} в {position}");
        }
    }

	// Создание ловушек-пил
	private void CreateSawTraps(Vector2I[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            var sawTrap = SawTrapPrefab.Instantiate<SawTrap>();
            _objectsContainer.AddChild(sawTrap);
            
            CallDeferred(nameof(DeferredAddSawTrap), sawTrap, positions[i], i + 1);
        }
    }
    private void DeferredAddSawTrap(SawTrap sawTrap, Vector2I position, int trapNumber)
    {
        if (LevelGrid.AddObjectToGrid(sawTrap, position))
        {
            GD.Print($"Пила-ловушка {trapNumber} создана в позиции {position}");
            sawTrap.UpdateWorldPositionImmediately();
        }
        else
        {
            GD.PrintErr($"Не удалось создать пилу-ловушку {trapNumber} в {position}");
        }
    }
}
