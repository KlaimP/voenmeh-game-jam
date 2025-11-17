using Godot;

/* Уровень 1 */
public partial class LevelScene : LevelsSceneBase
{
	// Указание перехода на уровень
	[Export] public uint numNextLvL = 1;
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
	// Ловушка - Шипы (Префаб)
	[Export] public PackedScene ThornsTrapPrefab { get; set; }
	// Ловушка - Лазер (Префаб)
	[Export] public PackedScene LaserTrapPrefab { get; set; }
	// Зона ящиков (должны быть)
	[Export] public PackedScene BoxTargetZonePrefab { get; set; }
	// Зона финиша робота
	[Export] public PackedScene FinishZonePrefab { get; set; }
	// IDE Робота
	[Export] public BlockEditorUi blockEditorUi { get; set; }
	
	[Export] public TextureRect BackgroundImage { get; set; }

	// Музыка уровня
	[Export] public AudioStream LevelMusic { get; set; }

	// Контейнер для объектов
	private Node2D _objectsContainer;
	
	// Массивы позиций для всех объектов уровня
	// Позиция робота
	private Vector2I _robotPosition = new Vector2I(1, 1);
	// Позиции ящиков
	private Vector2I[] _boxPositions = [ 
	];
	// Позиции стенок
	private Vector2I[] _obstaclePositions = [ 
		new Vector2I(0, 0), 
		new Vector2I(0, 1), 
		new Vector2I(0, 2), 
		new Vector2I(0, 3), 
		new Vector2I(0, 4), 
		new Vector2I(0, 5), 
		new Vector2I(0, 6), 
		new Vector2I(0, 7), 
		new Vector2I(0, 8), 
		new Vector2I(1, 0), 
		new Vector2I(2, 0),
		new Vector2I(2, 1),
		new Vector2I(2, 2),
		new Vector2I(2, 3),
		new Vector2I(2, 4),
		new Vector2I(2, 5),
		new Vector2I(2, 6),
		new Vector2I(2, 7),
		new Vector2I(2, 8),
		new Vector2I(1, 8),
	];
	// Позиции пил
	private Vector2I[] _sawTrapPositions = [ 
	];
	// Нахождение шипов относительно ячейки
	public enum RotationAngle 
	{
		Up = 0,      // 0° - Сверху
		Right = 90,  // 90° - Справа
		Down = 180,  // 180° - Снизу
		Left = 270   // 270° - Слева
	}
	// Позиции шипов и направлений
	private (Vector2I position, RotationAngle rotation)[] _thornsTrapPositions = [ 
	];
	// Конфигурация лазеров: начальная позиция, направление, длина
	private (Vector2I startPos, RotationAngle direction, int length)[] _laserConfigs = [ 
	];
	// Позиции зон ящиков
	private Vector2I[] _boxTargetZonePositions = [ 
	];
	// Позиция зоны завершения уровня
	private Vector2I _finishZonePosition = new Vector2I(1, 7);

	private GlobalSignals globalSignals;


	// Инициализация уровня
	public override void _Ready()
	{
		BackgroundImage.Visible = true;

		globalSignals = GetNode("/root/GlobalSignals") as GlobalSignals;
		globalSignals.Connect("EndGame", new Callable(this, nameof(EndGame)));

		// УСТАНАВЛИВАЕМ МУЗЫКУ УРОВНЯ - ДОБАВЬТЕ ЭТОТ БЛОК
		if (LevelMusic != null && MusicManager.Instance != null)
		{
			MusicManager.Instance.SetMusic(LevelMusic);
		}

		GD.Print("=== ЗАПУСК УРОВНЯ ===");
		// Проверка сетки
		if (LevelGrid == null)
		{
			GD.PrintErr("ОШИБКА: LevelGrid не назначен в инспекторе!");
			return;
		}
		// Проверка префабов
		if (RobotPrefab == null || 
			BoxPrefab == null || 
			ObstaclePrefab == null || 
			SawTrapPrefab == null ||
			ThornsTrapPrefab == null ||
			LaserTrapPrefab == null)
		{
			GD.PrintErr("ОШИБКА: Не все префабы назначены в инспекторе!");
			return;
		}
		// Получение контейнера объектов и инициализация уровня
		_objectsContainer = GetNode<Node2D>("Objects");

		// Инициализация сетки
		LevelGrid.InitializeGrid();
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

		GD.Print("Создание зон...");
		CreateBoxTargetZones(_boxTargetZonePositions);
		CreateFinishZone(_finishZonePosition);

		GD.Print("Создание препятствий...");
		CreateObstacles(_obstaclePositions);
		
		GD.Print("Создание ловушек...");
		CreateSawTraps(_sawTrapPositions);
		CreateThornsTraps(_thornsTrapPositions);
		CreateLaserTraps(_laserConfigs);

		GD.Print("Создание робота...");
		CreateRobot(_robotPosition);
		
		GD.Print("Создание ящиков...");
		CreateBoxes(_boxPositions);
		
		LevelGrid.PrintStateMatrix("ФИНАЛЬНОЕ СОСТОЯНИЕ");
	}

	private void EndGame()
	{
		OnLevelCompletionCheck();
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
	public override void RestartLevel()
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
	// Создание целевых зон для ящиков
	private void CreateBoxTargetZones(Vector2I[] positions)
	{
		for (int i = 0; i < positions.Length; i++)
		{
			var zone = BoxTargetZonePrefab.Instantiate<BoxTargetZone>();
			_objectsContainer.AddChild(zone);
			CallDeferred(nameof(DeferredAddBoxTargetZone), zone, positions[i], i + 1);
		}
	}
	private void DeferredAddBoxTargetZone(BoxTargetZone zone, Vector2I position, int zoneNumber)
	{
		if (LevelGrid.AddObjectToGrid(zone, position))
		{
			GD.Print($"Целевая зона для ящиков {zoneNumber} создана в позиции {position}");
			zone.UpdateWorldPositionImmediately();
		}
		else
		{
			GD.PrintErr($"Не удалось создать целевую зону для ящиков {zoneNumber} в {position}");
		}
	}

	// Создание финишной зоны
	private void CreateFinishZone(Vector2I position)
	{
		var zone = FinishZonePrefab.Instantiate<FinishZone>();
		_objectsContainer.AddChild(zone);
		CallDeferred(nameof(DeferredAddFinishZone), zone, position);
	}
	private void DeferredAddFinishZone(FinishZone zone, Vector2I position)
	{
		if (LevelGrid.AddObjectToGrid(zone, position))
		{
			GD.Print($"Финишная зона создана в позиции {position}");
			zone.UpdateWorldPositionImmediately();
		}
		else
		{
			GD.PrintErr($"Не удалось создать финишную зону в {position}");
		}
	}
	
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

	// Создание ловушек-шипов
	private void CreateThornsTraps((Vector2I position, RotationAngle rotation)[] positions)
	{
		for (int i = 0; i < positions.Length; i++)
		{
			var thornsTrap = ThornsTrapPrefab.Instantiate<ThornsTrap>();
			_objectsContainer.AddChild(thornsTrap);
			
			thornsTrap.Rotation = Mathf.DegToRad((float)positions[i].rotation);
			
			CallDeferred(nameof(DeferredAddThornsTrap), thornsTrap, positions[i].position, i + 1);
		}
	}
	private void DeferredAddThornsTrap(ThornsTrap thornsTrap, Vector2I position, int trapNumber)
	{
		if (LevelGrid.AddObjectToGrid(thornsTrap, position))
		{
			GD.Print($"Шипы-ловушка {trapNumber} создана в позиции {position} с поворотом {Mathf.RadToDeg(thornsTrap.Rotation)}°");
		}
		else
		{
			GD.PrintErr($"Не удалось создать шипы-ловушку {trapNumber} в {position}");
		}
	}

	private void CreateLaserTraps((Vector2I startPos, RotationAngle direction, int length)[] configs)
	{
		for (int i = 0; i < configs.Length; i++)
		{
			var config = configs[i];
			GD.Print($"Создание лазера {i + 1}: позиция={config.startPos}, направление={config.direction}, длина={config.length}");
			
			for (int j = 0; j < config.length; j++)
			{
				Vector2I laserPos = config.startPos + GetDirectionVector(config.direction) * j;
				
				var laserTrap = LaserTrapPrefab.Instantiate<LaserTrap>();
				_objectsContainer.AddChild(laserTrap);
				
				// Устанавливаем свойства
				if (j == 0)
				{
					laserTrap.SegmentType = LaserTrap.LaserSegmentType.Start;
					laserTrap.Rotation = Mathf.DegToRad((float)config.direction);
				}
				else if (j == config.length - 1)
				{
					laserTrap.SegmentType = LaserTrap.LaserSegmentType.End;
					// Противоположное направление для конечного фрагмента
					laserTrap.Rotation = Mathf.DegToRad((float)GetOppositeDirection(config.direction));
				}
				else
				{
					laserTrap.SegmentType = LaserTrap.LaserSegmentType.Beam;
					laserTrap.Rotation = Mathf.DegToRad((float)config.direction);
				}

				// ВЫЗЫВАЕМ ОБНОВЛЕНИЕ ТЕКСТУРЫ
				laserTrap.UpdateLaserTexture();

				CallDeferred(nameof(DeferredAddLaserTrap), laserTrap, laserPos, i + 1, j + 1);
			}
		}
	}

	// Получение противоположного направления
	private RotationAngle GetOppositeDirection(RotationAngle direction)
	{
		return direction switch
		{
			RotationAngle.Up => RotationAngle.Down,
			RotationAngle.Right => RotationAngle.Left,
			RotationAngle.Down => RotationAngle.Up,
			RotationAngle.Left => RotationAngle.Right,
			_ => RotationAngle.Up
		};
	}

	private void DeferredAddLaserTrap(LaserTrap laserTrap, Vector2I position, int laserNumber, int segmentNumber)
	{
		if (LevelGrid.AddObjectToGrid(laserTrap, position))
		{
			GD.Print($"✓ Лазер {laserNumber} сегмент {segmentNumber} создан в позиции {position}");
		}
		else
		{
			GD.PrintErr($"✗ Не удалось создать лазер {laserNumber} сегмент {segmentNumber} в {position}");
			laserTrap.QueueFree();
		}
	}

	private Vector2I GetDirectionVector(RotationAngle direction)
	{
		return direction switch
		{
			RotationAngle.Up => new Vector2I(0, -1),
			RotationAngle.Right => new Vector2I(1, 0),
			RotationAngle.Down => new Vector2I(0, 1),
			RotationAngle.Left => new Vector2I(-1, 0),
			_ => new Vector2I(1, 0)
		};
	}

	/* ПРОВЕРКА УРОВНЯ */
	// Функция для вызова из кнопки или по завершении команд (ОСНОВНАЯ)
	public void OnLevelCompletionCheck()
	{
		if (CheckLevelCompletion())
		{
			GD.Print("🎉 УРОВЕНЬ ПРОЙДЕН! 🎉");
			
			LoadNextLevel();

			// Здесь можно добавить:
			// - Показать сообщение о победе
			// - Воспроизвести звук
			// - Загрузить следующий уровень
			// - Показать кнопку продолжения
		}
		else
		{
			GD.Print("💪 Продолжайте выполнение команд...");
			RestartLevel();
		}
	}

	// Загрузка следующего уровня
	public void LoadNextLevel()
	{
		// Если номер следующего уровня равен 0 - возврат в главное меню
		if (numNextLvL == 0)
		{
			GD.Print($"Возврат в главное меню: {MainMenuPath}");
			
			var menuSceneResource = ResourceLoader.Load<PackedScene>(MainMenuPath);
			if (menuSceneResource != null)
			{
				// Удаляем текущую сцену перед загрузкой новой
				GetTree().CurrentScene.QueueFree();
				globalSignals.EndGame -= EndGame;
				GetTree().ChangeSceneToPacked(menuSceneResource);
			}
			else
			{
				GD.PrintErr($"Не удалось загрузить главное меню: {MainMenuPath}");
				RestartLevel();
			}
			return;
		}

		// Формируем путь к следующему уровню
		string nextLevel = $"{LevelsFolderPath}Level{numNextLvL}/LvL{numNextLvL}.tscn";
		
		GD.Print($"Переход к следующему уровню: {nextLevel}");
		
		// Загружаем следующую сцену
		var nextScene = ResourceLoader.Load<PackedScene>(nextLevel);
		if (nextScene != null)
		{
			// Удаляем текущую сцену перед загрузкой новой
			GetTree().CurrentScene.QueueFree();
			globalSignals.EndGame -= EndGame;
			GetTree().ChangeSceneToPacked(nextScene);
		}
		else
		{
			GD.PrintErr($"Не удалось загрузить сцену: {nextLevel}");
			// Если следующий уровень не найден - возврат в главное меню
			var menuSceneResource = ResourceLoader.Load<PackedScene>(MainMenuPath);
			if (menuSceneResource != null)
			{
				GetTree().CurrentScene.QueueFree();
				globalSignals.EndGame -= EndGame;
				GetTree().ChangeSceneToPacked(menuSceneResource);
			}
			else
			{
				RestartLevel();
			}
		}
	}
	
	// Функция проверки завершения уровня
	public bool CheckLevelCompletion()
	{
		bool allBoxesOnTarget = CheckAllBoxesOnTarget();
		bool robotOnFinish = CheckRobotOnFinish();
		
		bool levelCompleted = allBoxesOnTarget && robotOnFinish;
		
		GD.Print("=== ПРОВЕРКА ЗАВЕРШЕНИЯ УРОВНЯ ===");
		GD.Print($"Все ящики на целевых зонах: {allBoxesOnTarget}");
		GD.Print($"Робот на финишной зоне: {robotOnFinish}");
		GD.Print($"Уровень завершен: {levelCompleted}");
		GD.Print("=================================");
		
		return levelCompleted;
	}

	// Проверка что все ящики на целевых зонах
	private bool CheckAllBoxesOnTarget()
	{
		// Если целевых зон нет, то проверка пройдена
		if (BoxTargetZone.AllBoxTargetZones.Count == 0) return true;
			
		// Проверяем каждую целевую зону
		foreach (var targetZone in BoxTargetZone.AllBoxTargetZones)
		{
			if (!targetZone.HasBox)
			{
				GD.Print($"❌ Целевая зона в {targetZone.GridPosition} не занята ящиком");
				return false;
			}
		}
		
		GD.Print($"✅ Все {BoxTargetZone.AllBoxTargetZones.Count} целевых зон заняты ящиками");
		return true;
	}

	// Проверка что робот на финишной зоне
	private bool CheckRobotOnFinish()
	{
		// Если финишных зон нет, то проверка пройдена
		if (FinishZone.AllFinishZones.Count == 0) return true;
			
		// Проверяем каждую финишную зону
		foreach (var finishZone in FinishZone.AllFinishZones)
		{
			if (finishZone.HasRobot)
			{
				GD.Print($"✅ Робот на финишной зоне в {finishZone.GridPosition}");
				return true;
			}
		}
		
		GD.Print($"❌ Робот не на финишной зоне");
		return false;
	}

	
}
