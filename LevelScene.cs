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
        
        /* Генерация уровня здесь */
        GD.Print("=== НАЧАЛО ИНИЦИАЛИЗАЦИИ УРОВНЯ ===");
        GD.Print("Создание робота...");
        CreateRobot(objectsContainer);
        GD.Print("Создание ящиков...");
        CreateBoxes(objectsContainer);
        GD.Print("Создание препятствий...");
        CreateObstacles(objectsContainer);
        GD.Print("=== УРОВЕНЬ СОЗДАН ===");
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
    private void DeferredAddRobot(Robot robot, Vector2I position)
    {
        if (LevelGrid.AddObjectToGrid(robot, position))
        {
            GD.Print($"✓ Робот создан в позиции {position}");
            robot.UpdateWorldPositionImmediately();
        }
        else
        {
            GD.PrintErr($"✗ Не удалось создать робота в {position}!");
        }
    }

    /* ------------ Функции добавления объектов сцены ------------ */
    // Создание ящиков
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
            box.UpdateWorldPositionImmediately();
        }
        else
        {
            GD.PrintErr($"✗ Не удалось создать ящик {boxNumber} в {position}");
        }
    }

    // Создание препятствий (стенки)
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
            obstacle.UpdateWorldPositionImmediately();
        }
        else
        {
            GD.PrintErr($"✗ Не удалось создать препятствие {obstacleNumber} в {position}");
        }
    }
}