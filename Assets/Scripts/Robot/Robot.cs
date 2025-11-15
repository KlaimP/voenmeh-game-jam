using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

/* Робот */
public partial class Robot : GridObject
{
	[Export] public float MoveDuration { get; set; } = 0.3f;
	[Export] public float RotationDuration { get; set; } = 0.2f;

	private Sprite2D _sprite;
	private bool _isRotating = false;
	private bool _isMoving = false;

	// Инициализация параметров робота
	public override void _Ready()
	{
		ObjectType = "ROBOT";
		IsSolid = true;
		CanBePushed = false;
		
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		base._Ready();
		
		GD.Print("=== РОБОТ ГОТОВ ===");
		_grid.PrintStateMatrix();
	}

	public override void _Process(double delta)
	{
		if (!_isMoving && !_isRotating)
			HandleInput();
	}

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
		if (_isMoving) return;
		_isMoving = true;
		
		GD.Print($"РОБОТ: начинаю движение на {steps} шагов");
		
		for (int step = 1; step <= steps; step++)
		{
			Vector2I direction = GetForwardDirection();
			Vector2I newPosition = GridPosition + direction;
			
			GD.Print($"Шаг {step}/{steps}: попытка движения из {GridPosition} в {newPosition}");

			// Сначала проверяем что в целевой клетке
			GridObject targetObject = _grid.GetObjectAt(newPosition);
			
			if (targetObject == null)
			{
				// Свободная клетка - просто двигаемся
				await MoveToGridPosition(newPosition, MoveDuration);
				GD.Print($"✓ Шаг {step} выполнен");
			}
			else if (targetObject is TrapObject)
			{
				// Клетка с ловушкой - двигаемся и активируем ловушку
				await MoveToGridPosition(newPosition, MoveDuration);
				GD.Print($"✓ Шаг {step} выполнен (на ловушку)");
				targetObject.OnRobotEnter(this);
			}
			else if (CanPushObject(newPosition, direction))
			{
				// Можно толкнуть объект
				await PushSingleObject(newPosition, direction);
				GD.Print($"✓ Шаг {step} выполнен (с толканием объекта)");
			}
			else
			{
				GD.Print($"❌ Шаг {step} невозможен! Движение прервано.");
				break;
			}
			
			// Небольшая пауза между шагами для лучшей анимации
			if (step < steps)
				await Task.Delay(50);
		}
		
		GD.Print($"РОБОТ: движение завершено (выполнено шагов: {steps})");
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
		
		float targetRotation = Rotation - Mathf.Pi / 2f;
		
		_moveTween = CreateTween();
		_moveTween.SetEase(Tween.EaseType.Out);
		_moveTween.SetTrans(Tween.TransitionType.Cubic);
		_moveTween.TweenProperty(this, "rotation", targetRotation, RotationDuration);
		
		await ToSignal(_moveTween, "finished");
		
		_isRotating = false;
		GD.Print($"РОБОТ: повернул налево. Угол: {Mathf.RadToDeg(Rotation)}°");
	}

	// Поворот направо
	public async Task TurnRight()
	{
		if (_isRotating) return;
		_isRotating = true;
		
		float targetRotation = Rotation + Mathf.Pi / 2f;
		
		_moveTween = CreateTween();
		_moveTween.SetEase(Tween.EaseType.Out);
		_moveTween.SetTrans(Tween.TransitionType.Cubic);
		_moveTween.TweenProperty(this, "rotation", targetRotation, RotationDuration);
		
		await ToSignal(_moveTween, "finished");
		
		_isRotating = false;
		GD.Print($"РОБОТ: повернул направо. Угол: {Mathf.RadToDeg(Rotation)}°");
	}

	// Получение направления движения (с нормализацией)
	private Vector2I GetForwardDirection()
	{
		// Нормализуем угол только при получении направления
		float normalizedRotation = NormalizeAngle(Rotation);
		float degrees = Mathf.RadToDeg(normalizedRotation);
		
		if (degrees >= 315 || degrees < 45) return new Vector2I(0, -1);  // Вверх
		if (degrees >= 45 && degrees < 135) return new Vector2I(1, 0);   // Вправо
		if (degrees >= 135 && degrees < 225) return new Vector2I(0, 1);  // Вниз
		return new Vector2I(-1, 0);                                      // Влево
	}

	// Нормализация угла в диапазон [0, 2π)
	private float NormalizeAngle(float angle)
	{
		angle = angle % (2 * Mathf.Pi);
		if (angle < 0)
			angle += 2 * Mathf.Pi;
		return angle;
	}

	// Толкание одного объекта
	private async Task PushSingleObject(Vector2I objectPosition, Vector2I direction)
	{
		GD.Print($"РОБОТ: начинаю толкать объект в {objectPosition}");
		
		// Получаем объект в целевой позиции
		GridObject objectToPush = _grid.GetObjectAt(objectPosition);
		
		if (objectToPush == null)
		{
			GD.PrintErr("РОБОТ: не найден объект для толкания!");
			return;
		}
		
		if (!objectToPush.CanBePushed)
		{
			GD.PrintErr($"РОБОТ: объект {objectToPush.ObjectType} нельзя толкать!");
			return;
		}
		
		// Вычисляем новую позицию для объекта
		Vector2I newObjectPos = objectPosition + direction;
		
		// Проверяем, можно ли толкнуть объект
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