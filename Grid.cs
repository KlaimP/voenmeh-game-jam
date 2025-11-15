using Godot;
using System.Collections.Generic;

/* Сетка уровня */
public partial class Grid : Node2D
{
    // Ячеек в ширину
    [Export] public int GridWidth = 8;
    // Ячеек в высоту
    [Export] public int GridHeight = 8;
    // Размер ячеек (в px)
    [Export] public int CellSize = 64;
    [Export] public Color GridColor = new Color(0.3f, 0.3f, 0.3f);
    [Export] public Color BackgroundColor = new Color(0.1f, 0.1f, 0.1f);
    [Export] public bool ShowCellNumbers { get; set; } = true;

    // Словарь всех имеющихся объектов на сетке
    private Dictionary<Vector2I, GridObject> _gridObjects = new();
    // Состояние матрицы
    public int[,] StateMatrix { get; private set; }

    // Инициализация сетки
    public override void _Ready()
    {
        InitializeStateMatrix();
        QueueRedraw();
        GD.Print($"Grid инициализирован: {GridWidth}x{GridHeight}, CellSize: {CellSize}");
        PrintStateMatrix("После инициализации Grid");
    }

    // Отрисока сетки
    public override void _Draw()
    {
        // Рисуем фон
        DrawRect(new Rect2(0, 0, GridWidth * CellSize, GridHeight * CellSize), BackgroundColor);
        
        // Рисуем вертикальные линии
        for (int x = 0; x <= GridWidth; x++)
        {
            Vector2 start = new Vector2(x * CellSize, 0);
            Vector2 end = new Vector2(x * CellSize, GridHeight * CellSize);
            DrawLine(start, end, GridColor, 2);
        }
        
        // Рисуем горизонтальные линии
        for (int y = 0; y <= GridHeight; y++)
        {
            Vector2 start = new Vector2(0, y * CellSize);
            Vector2 end = new Vector2(GridWidth * CellSize, y * CellSize);
            DrawLine(start, end, GridColor, 2);
        }
        
        // Рисуем координаты ячеек
        if (ShowCellNumbers)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    string coordText = $"{x},{y}";
                    Vector2 position = new Vector2(x * CellSize + 5, y * CellSize + 15);
                    DrawString(ThemeDB.FallbackFont, position, coordText, HorizontalAlignment.Left, -1, 12);
                }
            }
        }
    }

    /* ------------ Функции для объектов сетки ------------ */
    // Добавление объектов на сетку и их регистрация в словаре
    public bool AddObjectToGrid(GridObject gridObject, Vector2I position)
    {
        // Проверка позиции в диапазоне сетке
        if (!IsInGridBounds(position)) 
        {
            GD.PrintErr($"Позиция {position} вне границ сетки!");
            return false;
        }
        
        // Проверка позиции и в ней объекта
        if (HasSolidObjectAt(position)) 
        {
            GD.PrintErr($"Позиция {position} уже занята твердым объектом {_gridObjects[position].ObjectType}!");
            return false;
        }

        // Устанавливаем позицию объекта
        gridObject.GridPosition = position;
        // Регистрируем объект в словаре
        _gridObjects[position] = gridObject;
        // Обновляем матрицу состояния
        UpdateStateMatrix();
        
        // Немедленно обновляем визуальную позицию объекта
        gridObject.GlobalPosition = GridToWorld(position);
        
        GD.Print($"✓ Успешно добавлен {gridObject.ObjectType} в позицию {position}");
        return true;
    }
    
    // Обновление позиции объекта
    public void UpdateObjectPosition(GridObject obj, Vector2I oldPosition, Vector2I newPosition)
    {
        GD.Print($"Обновление позиции {obj.ObjectType}: {oldPosition} -> {newPosition}");

        // Удаляем из старой позиции
        if (_gridObjects.ContainsKey(oldPosition) && _gridObjects[oldPosition] == obj)
        {
            _gridObjects.Remove(oldPosition);
            GD.Print($"Удален из {oldPosition}");
        }

        // Добавляем в новую позицию
        if (IsInGridBounds(newPosition))
        {
            _gridObjects[newPosition] = obj;
            GD.Print($"Добавлен в {newPosition}");
        }
        
        UpdateStateMatrix();
    }
 
    // Получение объекта по позиции
    public GridObject GetObjectAt(Vector2I position)
    {
        if (_gridObjects.ContainsKey(position)) return _gridObjects[position];
        return null;
    }





    /* ------------ Функции матрицы состояний ------------ */
    // Очистка матрицы состояний
    private void ClearStateMatrix()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                StateMatrix[x, y] = 0;
            }
        }
    }
    
    // Инициализация матрицы сетки
    private void InitializeStateMatrix()
    {
        StateMatrix = new int[GridWidth, GridHeight];
        ClearStateMatrix();
    }

    // Обновление матрицы состояний
    private void UpdateStateMatrix()
    {
        ClearStateMatrix();
        
        // Заполнение матрицы объектами
        foreach (var kvp in _gridObjects)
        {
            var position = kvp.Key;
            var obj = kvp.Value;
            
            if      (obj is Robot) StateMatrix[position.X, position.Y] = 1;
            else if (obj is BoxObject) StateMatrix[position.X, position.Y] = 2;
            else if (obj is ObstacleObject) StateMatrix[position.X, position.Y] = 3;
        }
    }

    // Логирование матрицы состояний
    public void PrintStateMatrix(string context = "")
    {
        string contextText = string.IsNullOrEmpty(context) ? "" : $" ({context})";
        GD.Print($"=== МАТРИЦА СОСТОЯНИЯ ГРИДА{contextText} ===");
        
        // Компактный вывод с номерами столбцов
        string header = "   ";
        for (int x = 0; x < GridWidth; x++)
        {
            header += $"{x} ";
        }
        GD.Print(header);
        
        for (int y = 0; y < GridHeight; y++)
        {
            string row = $"{y} [";
            for (int x = 0; x < GridWidth; x++)
            {
                switch (StateMatrix[x, y])
                {
                    case 0: row += "."; break;
                    case 1: row += "R"; break;
                    case 2: row += "B"; break;
                    case 3: row += "X"; break;
                    default: row += "?"; break;
                }
                if (x < GridWidth - 1) row += " ";
            }
            row += "]";
            GD.Print(row);
        }
        
        GD.Print("LEGEND: R=Robot, B=Box, X=Obstacle, .=Empty");
        GD.Print($"Objects: {_gridObjects.Count}, Grid: {GridWidth}x{GridHeight}");
        GD.Print("==============================");
    }





    /* ------------ Проверки по сетке и словарю ------------ */
    // Проверка ячейки на пустоту
    public bool IsCellEmpty(Vector2I position)
    {
        if (!IsInGridBounds(position)) return false;
        return !_gridObjects.ContainsKey(position) || !_gridObjects[position].IsSolid;
    }
    // Проверка существования объекта по позиции
    public bool HasSolidObjectAt(Vector2I position) => _gridObjects.ContainsKey(position) && _gridObjects[position].IsSolid;
    // Получение сеточных координат из глобальных
    public Vector2I WorldToGrid(Vector2 worldPosition)
    {
        int gridX = (int)(worldPosition.X / CellSize);
        int gridY = (int)(worldPosition.Y / CellSize);
        return new Vector2I(gridX, gridY);
    }
    // Получения глобальных координат из сеточных
    public Vector2 GridToWorld(Vector2I gridPosition)
    {
        float worldX = gridPosition.X * CellSize + CellSize / 2f;
        float worldY = gridPosition.Y * CellSize + CellSize / 2f;
        return new Vector2(worldX, worldY);
    }
    // Проверка нахождения позиции в сетке
    public bool IsInGridBounds(Vector2I gridPosition)
    {
        return gridPosition.X >= 0 && gridPosition.X < GridWidth && 
               gridPosition.Y >= 0 && gridPosition.Y < GridHeight;
    }
}