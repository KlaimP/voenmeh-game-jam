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
	// Движение вперёд
	public async Task MoveForward()
	{
		if (_isMoving) return;
		_isMoving = true;
		
		Vector2I direction = GetForwardDirection();
		Vector2I newPosition = GridPosition + direction;
		
		GD.Print($"РОБОТ: попытка движения из {GridPosition} в {newPosition}");

		if (_grid.IsCellEmpty(newPosition))
		{
			// Свободная клетка - просто двигаемся
			await MoveToGridPosition(newPosition, MoveDuration);
		}
		else if (CanPushObject(newPosition, direction))
		{
			// Можно толкнуть объект
			await PushSingleObject(newPosition, direction);
		}
		else
		{
			GD.Print("РОБОТ: движение невозможно!");
		}
		
		_isMoving = false;
	}

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

			if (_grid.IsCellEmpty(newPosition))
			{
				// Свободная клетка - просто двигаемся
				await MoveToGridPosition(newPosition, MoveDuration);
				GD.Print($"✓ Шаг {step} выполнен");
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

	/* // Движение назад (опционально, если надо)
	public async Task MoveBackward()
	{
		if (_isMoving) return;
		_isMoving = true;
		
		Vector2I direction = GetForwardDirection();
		Vector2I newPosition = GridPosition - direction;
		
		GD.Print($"РОБОТ: попытка движения назад в {newPosition}");
		
		if (_grid.IsCellEmpty(newPosition))
		{
			await MoveToGridPosition(newPosition, MoveDuration);
		}
		else
		{
			GD.Print("РОБОТ: движение назад невозможно!");
		}
		
		_isMoving = false;
	}*/

	// Получение направления движения
	private Vector2I GetForwardDirection()
	{
		float degrees = Mathf.RadToDeg(Rotation);
		degrees = (degrees % 360 + 360) % 360; // Нормализуем в [0, 360)
		
		if (degrees >= 315 || degrees < 45) return new Vector2I(0, -1);  // Вверх
		if (degrees >= 45 && degrees < 135) return new Vector2I(1, 0);   // Вправо
		if (degrees >= 135 && degrees < 225) return new Vector2I(0, 1);  // Вниз
		return new Vector2I(-1, 0);                                      // Влево
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
		
		if (_grid.HasSolidObjectAt(newObjectPos))
		{
			GD.PrintErr("РОБОТ: объект нельзя толкнуть - целевая позиция занята!");
			return;
		}
		
		GD.Print($"РОБОТ: толкаю {objectToPush.ObjectType} из {objectPosition} в {newObjectPos}");
		
		// Двигаем объект
		await objectToPush.MoveToGridPosition(newObjectPos, MoveDuration);
		
		// Двигаем робота на место объекта
		await MoveToGridPosition(objectPosition, MoveDuration);
		
		GD.Print($"РОБОТ: успешно толкнул {objectToPush.ObjectType}");
	}

	// Проверка возможности толкания объекта
	private bool CanPushObject(Vector2I objectPosition, Vector2I direction)
	{
		if (!_grid.IsInGridBounds(objectPosition)) return false;
		
		// Получаем объект в целевой позиции
		GridObject obj = _grid.GetObjectAt(objectPosition);
		
		// Проверяем, есть ли толкаемый объект
		if (obj == null || !obj.CanBePushed) return false;
		
		// Проверяем, свободна ли следующая позиция
		Vector2I nextPos = objectPosition + direction;
		return _grid.IsInGridBounds(nextPos) && _grid.IsCellEmpty(nextPos);
	}
}
