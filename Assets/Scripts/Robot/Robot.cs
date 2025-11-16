using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

/* Робот */
public partial class Robot : GridObject
{
	// Спрайты робота
	[Export] private Texture2D _spriteUp;
	[Export] private Texture2D _spriteRight;
	[Export] private Texture2D _spriteDown;
	[Export] private Texture2D _spriteLeft;
	// Скорость движения
	[Export] public float MoveDuration { get; set; } = 0.3f;
	// Скорость поворота
	[Export] public float RotationDuration { get; set; } = 0.2f;


	private Sprite2D _sprite;
	private bool _isRotating = false;
	private bool _isMoving = false;
	// Направление взгляда робота
	public enum RobotDirection
	{
		Up,    // Вверх
		Right, // Вправо
		Down,  // Вниз
		Left   // Влево
	}
	private RobotDirection _currentDirection = RobotDirection.Up;



	// Инициализация робота
	public override void _Ready()
	{
		ObjectType = "ROBOT";
		IsSolid = true;      // Твёрдый
		CanBePushed = false; // Нельзя сдвинуть
		// Спрайт робота
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		base._Ready();
		// Устанавливаем начальный спрайт
   		SetDirection(RobotDirection.Up);
		GD.Print("=== РОБОТ ГОТОВ ===");
		_grid.PrintStateMatrix();
	}

	// Покадровое обновление
	public override void _Process(double delta)
	{
		if (!_isMoving && !_isRotating) HandleInput();
	}



	// Управление роботом (Debug)
	private void HandleInput()
	{
		if (Input.IsActionJustPressed("ui_up")) _ = MoveForward();
		if (Input.IsActionJustPressed("ui_left")) _ = TurnLeft();
		if (Input.IsActionJustPressed("ui_right")) _ = TurnRight();
		if (Input.IsActionJustPressed("ui_page_up")) _grid.PrintStateMatrix();
	}

	// ------------ КОМАНДЫ РОБОТА ------------ */
	// Движение вперёд на указанное количество шагов
	public async Task MoveForward(int steps = 1)
	{
		// Проверка движения
		if (_isMoving) return;
		// Движение ON
		_isMoving = true;
		
		GD.Print($"РОБОТ: начинаю движение на {steps} шагов");
		
		// Шаги через цикл
		for (int step = 1; step <= steps; step++)
		{
			// Получение направления движания
			Vector2I direction = GetForwardDirection();
			Vector2I newPosition = GridPosition + direction;
			
			GD.Print($"Шаг {step}/{steps}: попытка движения из {GridPosition} в {newPosition}");

			// Сначала проверяем что в целевой клетке
			GridObject targetObject = _grid.GetObjectAt(newPosition);
			
			// Свободная клетка - просто двигаемся
			if (targetObject == null)
			{
				await MoveToGridPosition(newPosition, MoveDuration);
				GD.Print($"✓ Шаг {step} выполнен");
			}
			// Клетка с ловушкой - двигаемся и активируем ловушку
			else if (targetObject is TrapObject)
			{
				await MoveToGridPosition(newPosition, MoveDuration);
				GD.Print($"✓ Шаг {step} выполнен (на ловушку)");
				targetObject.OnRobotEnter(this);
			}
			// Можно толкнуть объект
			else if (CanPushObject(newPosition, direction))
			{
				await PushSingleObject(newPosition, direction);
				GD.Print($"✓ Шаг {step} выполнен (с толканием объекта)");
			}
			// Шаг невозможен
			else
			{
				GD.Print($"❌ Шаг {step} невозможен! Движение прервано.");
				break;
			}
			
			// Небольшая пауза между шагами для лучшей анимации
			if (step < steps) await Task.Delay(50);
		}
		
		GD.Print($"РОБОТ: движение завершено (выполнено шагов: {steps})");
		// Движение OFF
		_isMoving = false;
	}

    // Функция получения урона
    public void TakeDamage(int damage)
    {
        GD.Print($"💥 РОБОТ ПОЛУЧИЛ УРОН: {damage}");
        
        // Визуальный эффект получения урона
        PlayDamageEffect();
        
        // Здесь можно добавить логику здоровья:
        // - Уменьшение HP
        // - Проверка на смерть
        // - Воспроизведение звука
        // - Анимация мигания
    }
	// Анимация получения урона
    private void PlayDamageEffect()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1, 0.3f, 0.3f, 1), 0.1f);
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.1f);
    }

	// Поворот налево
	public async Task TurnLeft()
	{
		if (_isRotating) return;
		_isRotating = true;
		
		// Определяем новое направление
		RobotDirection newDirection = _currentDirection switch
		{
			RobotDirection.Up => RobotDirection.Left,
			RobotDirection.Right => RobotDirection.Up,
			RobotDirection.Down => RobotDirection.Right,
			RobotDirection.Left => RobotDirection.Down,
			_ => RobotDirection.Up
		};
		
		// Анимация смены спрайта
		await AnimateSpriteChange(newDirection);
		
		_isRotating = false;
		GD.Print($"РОБОТ: повернул налево. Направление: {newDirection}");
	}

	// Поворот направо
	public async Task TurnRight()
	{
		if (_isRotating) return;
		_isRotating = true;
		
		// Определяем новое направление
		RobotDirection newDirection = _currentDirection switch
		{
			RobotDirection.Up => RobotDirection.Right,
			RobotDirection.Right => RobotDirection.Down,
			RobotDirection.Down => RobotDirection.Left,
			RobotDirection.Left => RobotDirection.Up,
			_ => RobotDirection.Up
		};
		
		// Анимация смены спрайта
		await AnimateSpriteChange(newDirection);
		
		_isRotating = false;
		GD.Print($"РОБОТ: повернул направо. Направление: {newDirection}");
	}
	// Анимация поворота
	private async Task AnimateSpriteChange(RobotDirection newDirection)
	{
		// Анимация уменьшения
		var tweenOut = CreateTween();
		tweenOut.TweenProperty(_sprite, "scale", new Vector2(0.8f, 0.8f), RotationDuration / 2);
		await ToSignal(tweenOut, "finished");
		
		// Меняем спрайт
		SetDirection(newDirection);
		
		// Анимация возврата к нормальному размеру
		var tweenIn = CreateTween();
		tweenIn.TweenProperty(_sprite, "scale", Vector2.One, RotationDuration / 2);
		await ToSignal(tweenIn, "finished");
	}

	// Метод обновления спрайта робота
	private void UpdateRobotSprite()
	{
		if (_sprite == null) return;
		
		// Определяем направление по углу
		float degrees = Mathf.RadToDeg(NormalizeAngle(Rotation));
		
		if (degrees >= 315 || degrees < 45) SetDirection(RobotDirection.Up);
		else if (degrees >= 45 && degrees < 135) SetDirection(RobotDirection.Right);
		else if (degrees >= 135 && degrees < 225) SetDirection(RobotDirection.Down);
		else SetDirection(RobotDirection.Left);
	}
	// Установка направлений
	private void SetDirection(RobotDirection direction)
	{
		_currentDirection = direction;
		
		switch (direction)
		{
			case RobotDirection.Up: _sprite.Texture = _spriteUp; break;
			case RobotDirection.Right: _sprite.Texture = _spriteRight; break;
			case RobotDirection.Down: _sprite.Texture = _spriteDown; break;
			case RobotDirection.Left: _sprite.Texture = _spriteLeft; break;
		}
	}

	// Получение направления движения (с нормализацией)
	private Vector2I GetForwardDirection()
	{
		switch (_currentDirection)
		{
			case RobotDirection.Up: return new Vector2I(0, -1);
			case RobotDirection.Right: return new Vector2I(1, 0);
			case RobotDirection.Down: return new Vector2I(0, 1);
			case RobotDirection.Left: return new Vector2I(-1, 0);
			default: return new Vector2I(0, -1);
		}
	}
	public RobotDirection GetCurrentDirection() => _currentDirection; 

	// Нормализация угла в диапазон [0, 2π)
	private float NormalizeAngle(float angle)
	{
		angle = angle % (2 * Mathf.Pi);
		if (angle < 0) angle += 2 * Mathf.Pi;
		return angle;
	}

	// Толкание одного объекта
	private async Task PushSingleObject(Vector2I objectPosition, Vector2I direction)
	{
		GD.Print($"РОБОТ: начинаю толкать объект в {objectPosition}");
		
		// Получаем объект в целевой позиции
		GridObject objectToPush = _grid.GetObjectAt(objectPosition);
		// Проверка объекта
		if (objectToPush == null)
		{
			GD.PrintErr("РОБОТ: не найден объект для толкания!");
			return;
		}
		// Можно ли толкнуть объект
		if (!objectToPush.CanBePushed)
		{
			GD.PrintErr($"РОБОТ: объект {objectToPush.ObjectType} нельзя толкать!");
			return;
		}
		
		// Вычисляем новую позицию для объекта
		Vector2I newObjectPos = objectPosition + direction;
		
		// Проверяем, можно ли толкнуть объект в координаты
		if (!_grid.IsInGridBounds(newObjectPos))
		{
			GD.PrintErr("РОБОТ: объект нельзя толкнуть - выход за границы сетки!");
			return;
		}
		
		// Проверяем, что в целевой позиции
		GridObject targetObject = _grid.GetObjectAt(newObjectPos);
		if (targetObject != null)
		{
			if (targetObject is TrapObject)
			{
				// Толкаем на ловушку - объект уничтожается
				GD.Print($"РОБОТ: объект {objectToPush.ObjectType} толкается на ловушку!");
				await DestroyObjectOnTrap(objectToPush, newObjectPos);
			}
			else
			{
				GD.PrintErr("РОБОТ: объект нельзя толкнуть - целевая позиция занята!");
				return;
			}
		}
		else
		{
			// Обычное толкание на свободную клетку
			GD.Print($"РОБОТ: толкаю {objectToPush.ObjectType} из {objectPosition} в {newObjectPos}");
			await objectToPush.MoveToGridPosition(newObjectPos, MoveDuration);
		}
		
		// Двигаем робота на место объекта
		await MoveToGridPosition(objectPosition, MoveDuration);
		
		GD.Print($"РОБОТ: успешно завершил действие");
	}

	// Уничтожение объекта при толкании на ловушку
	private async Task DestroyObjectOnTrap(GridObject objectToDestroy, Vector2I trapPosition)
	{
		GD.Print($"УНИЧТОЖЕНИЕ: объект {objectToDestroy.ObjectType} уничтожен ловушкой в {trapPosition}");
		
		// Визуальные эффекты уничтожения
		await PlayDestructionEffects(objectToDestroy);
		
		// Удаляем объект из сетки
		_grid.RemoveObjectFromGrid(objectToDestroy.GridPosition);
		
		// Уничтожаем объект
		objectToDestroy.QueueFree();
	}

	// Визуальные эффекты уничтожения
	private async Task PlayDestructionEffects(GridObject obj)
	{
		// Анимация исчезновения
		var tween = CreateTween();
		tween.TweenProperty(obj, "scale", Vector2.Zero, 0.2f);
		tween.TweenProperty(obj, "modulate", new Color(1, 0, 0, 0.5f), 0.2f);
		
		await ToSignal(tween, "finished");
	}

	// Проверка возможности толкания объекта
	private bool CanPushObject(Vector2I objectPosition, Vector2I direction)
	{
		if (!_grid.IsInGridBounds(objectPosition)) return false;
		
		// Получаем объект в целевой позиции
		GridObject obj = _grid.GetObjectAt(objectPosition);
		
		// Проверяем, есть ли толкаемый объект
		if (obj == null || !obj.CanBePushed) return false;
		
		// Проверяем следующую позицию
		Vector2I nextPos = objectPosition + direction;
		if (!_grid.IsInGridBounds(nextPos)) return false;
		
		// Можно толкать если:
		// 1. Клетка пустая ИЛИ
		// 2. В клетке ловушка (объект уничтожится)
		GridObject targetObj = _grid.GetObjectAt(nextPos);
		return targetObj == null || targetObj is TrapObject;
	}
}