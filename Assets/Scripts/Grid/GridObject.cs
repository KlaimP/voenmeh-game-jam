using Godot;
using System.Threading.Tasks;

/* Базовый класс: Объект сетки
   (Родитель всех элементов на сетке) 
*/
public partial class GridObject : Node2D
{
	// Позиция на сетке
	[Export] public Vector2I GridPosition { get; set; }
	// Тип объекта
	[Export] public string ObjectType { get; protected set; } = "BASE";
	// Твёрдый / не твердый
	[Export] public bool IsSolid { get; protected set; } = false;
	// Может ли быть сдвинут
	[Export] public bool CanBePushed { get; protected set; } = false;

	// Сетка 
	protected Grid _grid; 
	// Анимация движения
	protected Tween _moveTween; 

	// Инициализация объекта
	public override void _Ready()
	{
		// Ищем Grid в родителях или на сцене
		_grid = GetParent() as Grid;
		if (_grid == null)
		{
			_grid = GetTree().CurrentScene.GetNode<Grid>("Grid");
		}
		
		if (_grid == null)
		{
			GD.PrintErr($"GridObject {ObjectType} не может найти Grid!");
			return;
		}

		// НЕ регистрируемся здесь - регистрация будет через Grid.AddObjectToGrid
		
		GD.Print($"{ObjectType} готов к размещению в {GridPosition}");
	}

	// Немедленное обновление позиции (без анимации)
	public void UpdateWorldPositionImmediately()
	{
		if (_grid != null)
		{
			GlobalPosition = _grid.GridToWorld(GridPosition);
			GD.Print($"{ObjectType} установлен в мировую позицию {GlobalPosition} (сетка: {GridPosition})");
		}
	}

	// Движение объекта с анимацией
	public virtual async Task MoveToGridPosition(Vector2I newPosition, float duration = 0.3f)
	{
		// Проверка сетки и возможности движения
		if (_grid == null) return;
		if (!CanMoveToPosition(newPosition)) return;

		Vector2I oldPosition = GridPosition;
		Vector2 targetWorldPos = _grid.GridToWorld(newPosition);

		GD.Print($"{ObjectType} начинает движение: {oldPosition} -> {newPosition}");

		// Анимация движения
		_moveTween = CreateTween();
		_moveTween.SetEase(Tween.EaseType.Out);
		_moveTween.SetTrans(Tween.TransitionType.Cubic);
		_moveTween.TweenProperty(this, "global_position", targetWorldPos, duration);
		
		await ToSignal(_moveTween, "finished");
		
		// Обновляем позицию в Grid
		GridPosition = newPosition;
		_grid.UpdateObjectPosition(this, oldPosition, newPosition);
		
		GD.Print($"{ObjectType} завершил движение в {newPosition}");
	}

	// Проверка возможности движения в указанную позицию
	public virtual bool CanMoveToPosition(Vector2I targetPosition)
	{
		// Проверка сетки и позиции
		if (_grid == null) return false;
		if (!_grid.IsInGridBounds(targetPosition)) return false;
		
		// Для твердых объектов проверяем, что целевая ячейка свободна
		if (IsSolid && _grid.HasSolidObjectAt(targetPosition)) return false;
			
		return true;
	}

	// Виртуальный метод для обработки наступления робота (для нетвёрдых объектов)
    public virtual void OnRobotEnter(Robot robot) { }

	// Виртуальный метод для анимации разрушения
    public virtual async Task PlayDestructionEffects()
    {
        // Базовая анимация - исчезновение с уменьшением и покраснением
        var tween = CreateTween();
        tween.TweenProperty(this, "scale", Vector2.Zero, 0.2f);
        tween.TweenProperty(this, "modulate", new Color(1, 0, 0, 0.5f), 0.2f);
        
        await ToSignal(tween, "finished");
    }

    // Виртуальный метод для обработки разрушения
    public virtual async Task OnDestroyed()
    {
        GD.Print($"{ObjectType} уничтожается в позиции {GridPosition}");
        await PlayDestructionEffects();
    }
}
