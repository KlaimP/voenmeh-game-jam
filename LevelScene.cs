using Godot;

/* Уровень 1 */
public partial class LevelScene : Node2D
{
    // Сетка 
    [Export] public Grid LevelGrid { get; set; }
    // Робот (Префаб)
    [Export] public PackedScene RobotPrefab { get; set; }
    [Export] public PackedScene BoxPrefab { get; set; }
    [Export] public PackedScene ObstaclePrefab { get; set; }

    // Инициализация уровня
    public override void _Ready()
    {
        GD.Print("=== ЗАПУСК УРОВНЯ ===");
        
        if (LevelGrid == null)
        {
            GD.PrintErr("ОШИБКА: LevelGrid не назначен в инспекторе!");
            return;
        }

        if (RobotPrefab == null || BoxPrefab == null || ObstaclePrefab == null)
        {
            GD.PrintErr("ОШИБКА: Не все префабы назначены в инспекторе!");
            return;
        }

        InitializeLevel();
        
        GD.Print("=== УПРАВЛЕНИЕ ===");
        GD.Print("Стрелка ВВЕРХ - Движение вперед");
        GD.Print("Стрелка ВНИЗ - Движение назад");
        GD.Print("Стрелка ВЛЕВО - Поворот налево");
        GD.Print("Стрелка ВПРАВО - Поворот направо");
        GD.Print("Page Up - Показать матрицу состояния");
        GD.Print("==================");
    }
    private void InitializeLevel()
    {
        var objectsContainer = GetNode<Node2D>("Objects");
        
        GD.Print("=== НАЧАЛО ИНИЦИАЛИЗАЦИИ УРОВНЯ ===");
        LevelGrid.PrintStateMatrix("До создания объектов");
        
        /*
        GD.Print("Создание стен...");
        CreateWalls(objectsContainer);
        LevelGrid.PrintStateMatrix("После создания стен");
        
        GD.Print("Создание препятствий...");
        CreateObstacles(objectsContainer);
        LevelGrid.PrintStateMatrix("После создания препятствий");*/
        
        GD.Print("Создание ящиков...");
        CreateBoxes(objectsContainer);
        LevelGrid.PrintStateMatrix("После создания ящиков");
        
        GD.Print("Создание робота...");
        CreateRobot(objectsContainer);
        LevelGrid.PrintStateMatrix("После создания робота");
        
        GD.Print("=== УРОВЕНЬ СОЗДАН ===");
        LevelGrid.PrintStateMatrix("ФИНАЛЬНОЕ СОСТОЯНИЕ");
    }

    // Создание робота
    private void CreateRobot(Node2D container)
    {
        // Создание объекта робота
        var robot = RobotPrefab.Instantiate<Robot>();
        container.AddChild(robot);
        
        // Ждем один кадр для полной инициализации
        CallDeferred(nameof(DeferredAddRobot), robot, new Vector2I(2, 2));
    }
    // Добавление робота на сетку
    private void DeferredAddRobot(Robot robot, Vector2I position)
    {
        if (LevelGrid.AddObjectToGrid(robot, position))
        {
            GD.Print($"✓ Робот создан в позиции {position}");
            // Принудительно обновляем визуальную позицию
            robot.UpdateWorldPositionImmediately();
        }
        else
        {
            GD.PrintErr($"✗ Не удалось создать робота в {position}!");
        }
    }

    // Функции добавления других объектов
    
    private void CreateBoxes(Node2D container)
    {
        var boxPositions = new Vector2I[] { 
            new Vector2I(4, 3), 
            new Vector2I(5, 4), 
            new Vector2I(3, 5) 
        };
        
        for (int i = 0; i < boxPositions.Length; i++)
        {
            var box = BoxPrefab.Instantiate<BoxObject>();
            container.AddChild(box);
            
            // Ждем один кадр для полной инициализации
            var pos = boxPositions[i];
            CallDeferred(nameof(DeferredAddBox), box, pos, i + 1);
        }
    }

    private void DeferredAddBox(BoxObject box, Vector2I position, int boxNumber)
    {
        if (LevelGrid.AddObjectToGrid(box, position))
        {
            GD.Print($"✓ Ящик {boxNumber} создан в позиции {position}");
            // Принудительно обновляем визуальную позицию
            box.UpdateWorldPositionImmediately();
        }
        else
        {
            GD.PrintErr($"✗ Не удалось создать ящик {boxNumber} в {position}");
        }
    }

    private void CreateObstacles(Node2D container)
    {
        var obstaclePositions = new Vector2I[] { 
            new Vector2I(1, 1), 
            new Vector2I(6, 2), 
            new Vector2I(3, 6) 
        };
        
        for (int i = 0; i < obstaclePositions.Length; i++)
        {
            var obstacle = ObstaclePrefab.Instantiate<ObstacleObject>();
            container.AddChild(obstacle);
            
            // Ждем один кадр для полной инициализации
            var pos = obstaclePositions[i];
            CallDeferred(nameof(DeferredAddObstacle), obstacle, pos, i + 1);
        }
    }

    private void DeferredAddObstacle(ObstacleObject obstacle, Vector2I position, int obstacleNumber)
    {
        if (LevelGrid.AddObjectToGrid(obstacle, position))
        {
            GD.Print($"✓ Препятствие {obstacleNumber} создано в позиции {position}");
            // Принудительно обновляем визуальную позицию
            obstacle.UpdateWorldPositionImmediately();
        }
        else
        {
            GD.PrintErr($"✗ Не удалось создать препятствие {obstacleNumber} в {position}");
        }
    }

    private void CreateWalls(Node2D container)
    {
        GD.Print("Создание нижней стены...");
        // Нижняя стена
        for (int x = 0; x < LevelGrid.GridWidth; x++)
        {
            var obstacle = ObstaclePrefab.Instantiate<ObstacleObject>();
            container.AddChild(obstacle);
            var pos = new Vector2I(x, LevelGrid.GridHeight - 1);
            CallDeferred(nameof(DeferredAddWall), obstacle, pos);
        }
        
        GD.Print("Создание правой стены...");
        // Правая стена
        for (int y = 0; y < LevelGrid.GridHeight - 1; y++)
        {
            var obstacle = ObstaclePrefab.Instantiate<ObstacleObject>();
            container.AddChild(obstacle);
            var pos = new Vector2I(LevelGrid.GridWidth - 1, y);
            CallDeferred(nameof(DeferredAddWall), obstacle, pos);
        }
        
        GD.Print("✓ Стены созданы по границам уровня");
    }

    private void DeferredAddWall(ObstacleObject obstacle, Vector2I position)
    {
        if (LevelGrid.AddObjectToGrid(obstacle, position))
        {
            obstacle.UpdateWorldPositionImmediately();
        }
    }
    
}