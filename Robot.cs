using Godot;
using System.Threading.Tasks;

public partial class Robot : GridObject
{
	[Export] public float MoveDuration { get; set; } = 0.3f;
	[Export] public float RotationDuration { get; set; } = 0.2f;

	private Sprite2D _sprite;
	
	private enum Direction { Up, Right, Down, Left }
	private Direction _currentDirection = Direction.Up;
	
	private bool _isRotating = false;
	private bool _isMoving = false;

	public override void _Ready()
	{
		ObjectType = "ROBOT";
		IsSolid = true;
		
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		base._Ready();
		
		SyncRotationToDirection();
		GD.Print("Робот готов к ручному управлению");
	}

	public override void _Process(double delta)
	{
		if (!_isMoving && !_isRotating)
		{
			HandleInput();
		}
	}

	private void HandleInput()
	{
		// Движение вперед
		if (Input.IsActionJustPressed("ui_up"))
		{
			_ = MoveForward();
		}
		
		// Поворот налево
		if (Input.IsActionJustPressed("ui_left"))
		{
			_ = TurnLeft();
		}
		
		// Поворот направо
		if (Input.IsActionJustPressed("ui_right"))
		{
			_ = TurnRight();
		}
		
		// Движение назад
		if (Input.IsActionJustPressed("ui_down"))
		{
			_ = MoveBackward();
		}
		
		// Быстрое движение (для теста)
		if (Input.IsActionJustPressed("ui_page_up"))
		{
			_ = MoveForwardFast();
		}
	}

	public async Task MoveForward()
	{
		if (_isMoving) return;
		_isMoving = true;
		
		Vector2I direction = GetForwardDirection();
		Vector2I newPosition = GridPosition + direction;
		
		if (CanMoveToPosition(newPosition))
		{
			// Проверяем, есть ли ящик в целевой позиции
			var objectsAtTarget = _grid.GetObjectsAt(newPosition);
			BoxObject boxToPush = null;
			
			foreach (var obj in objectsAtTarget)
			{
				if (obj is BoxObject box)
				{
					boxToPush = box;
					break;
				}
			}
			
			if (boxToPush != null)
			{
				// Толкаем ящик
				Vector2I boxTargetPosition = newPosition + direction;
				if (boxToPush.CanMoveToPosition(boxTargetPosition))
				{
					// Двигаем ящик и робота одновременно
					var boxTask = boxToPush.MoveToGridPosition(boxTargetPosition, MoveDuration);
					var robotTask = MoveToGridPosition(newPosition, MoveDuration);
					
					await Task.WhenAll(boxTask, robotTask);
					GD.Print("Робот толкнул ящик!");
				}
				else
				{
					GD.Print("Не могу толкнуть ящик - путь заблокирован");
				}
			}
			else
			{
				// Обычное движение
				await MoveToGridPosition(newPosition, MoveDuration);
			}
		}
		else
		{
			GD.Print("Движение вперед невозможно!");
		}
		
		_isMoving = false;
	}

	public async Task MoveForwardFast()
	{
		if (_isMoving) return;
		_isMoving = true;
		
		Vector2I direction = GetForwardDirection();
		Vector2I newPosition = GridPosition + direction;
		
		if (CanMoveToPosition(newPosition))
		{
			// Быстрое движение без анимации толкания ящиков
			await MoveToGridPosition(newPosition, MoveDuration * 0.5f);
		}
		
		_isMoving = false;
	}

	public async Task MoveBackward()
	{
		if (_isMoving) return;
		_isMoving = true;
		
		Vector2I direction = GetForwardDirection();
		Vector2I newPosition = GridPosition - direction;
		
		if (CanMoveToPosition(newPosition))
		{
			await MoveToGridPosition(newPosition, MoveDuration);
		}
		else
		{
			GD.Print("Движение назад невозможно!");
		}
		
		_isMoving = false;
	}

	public async Task TurnLeft()
	{
		if (_isRotating) return;
		_isRotating = true;
		
		// Сначала меняем направление
		_currentDirection = _currentDirection switch
		{
			Direction.Up => Direction.Left,
			Direction.Left => Direction.Down,
			Direction.Down => Direction.Right,
			Direction.Right => Direction.Up,
			_ => Direction.Up
		};
		
		// Затем анимируем поворот к точному углу (в радианах)
		float targetRotation = DirectionToRotation(_currentDirection);
		
		// Нормализуем текущий rotation чтобы избежать больших поворотов
		float currentNormalized = NormalizeRotation(Rotation);
		
		// Выбираем кратчайший путь для анимации
		float shortestPathRotation = GetShortestPathRotation(currentNormalized, targetRotation);
		
		_moveTween = CreateTween();
		_moveTween.SetEase(Tween.EaseType.Out);
		_moveTween.SetTrans(Tween.TransitionType.Cubic);
		_moveTween.TweenProperty(this, "rotation", shortestPathRotation, RotationDuration);
		
		await ToSignal(_moveTween, "finished");
		
		// Гарантируем точное значение после анимации
		Rotation = targetRotation;
		_isRotating = false;
	}

	public async Task TurnRight()
	{
		if (_isRotating) return;
		_isRotating = true;
		
		// Сначала меняем направление
		_currentDirection = _currentDirection switch
		{
			Direction.Up => Direction.Right,
			Direction.Right => Direction.Down,
			Direction.Down => Direction.Left,
			Direction.Left => Direction.Up,
			_ => Direction.Up
		};
		
		// Затем анимируем поворот к точному углу (в радианах)
		float targetRotation = DirectionToRotation(_currentDirection);
		
		// Нормализуем текущий rotation чтобы избежать больших поворотов
		float currentNormalized = NormalizeRotation(Rotation);
		
		// Выбираем кратчайший путь для анимации
		float shortestPathRotation = GetShortestPathRotation(currentNormalized, targetRotation);
		
		_moveTween = CreateTween();
		_moveTween.SetEase(Tween.EaseType.Out);
		_moveTween.SetTrans(Tween.TransitionType.Cubic);
		_moveTween.TweenProperty(this, "rotation", shortestPathRotation, RotationDuration);
		
		await ToSignal(_moveTween, "finished");
		
		// Гарантируем точное значение после анимации
		Rotation = targetRotation;
		_isRotating = false;
	}

	private Vector2I GetForwardDirection()
	{
		return _currentDirection switch
		{
			Direction.Up => new Vector2I(0, -1),
			Direction.Right => new Vector2I(1, 0),
			Direction.Down => new Vector2I(0, 1),
			Direction.Left => new Vector2I(-1, 0),
			_ => new Vector2I(0, -1)
		};
	}

	private float DirectionToRotation(Direction direction)
	{
		return direction switch
		{
			Direction.Up => 0f,
			Direction.Right => Mathf.Pi / 2f,
			Direction.Down => Mathf.Pi,
			Direction.Left => Mathf.Pi * 3f / 2f,
			_ => 0f
		};
	}

	// Нормализует rotation в диапазон [0, 2π)
	private float NormalizeRotation(float rotation)
	{
		float twoPi = Mathf.Pi * 2f;
		rotation = rotation % twoPi;
		if (rotation < 0) rotation += twoPi;
		return rotation;
	}

	// Находит кратчайший путь для анимации между двумя углами
	private float GetShortestPathRotation(float from, float to)
	{
		float difference = to - from;
		float twoPi = Mathf.Pi * 2f;
		
		if (difference > Mathf.Pi)
		{
			return to - twoPi;
		}
		else if (difference < -Mathf.Pi)
		{
			return to + twoPi;
		}
		
		return to;
	}

	private void SyncRotationToDirection()
	{
		Rotation = DirectionToRotation(_currentDirection);
	}
}
