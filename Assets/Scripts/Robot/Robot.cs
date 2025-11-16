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


	// Звуки
	[Export] public AudioStream BoxTrapSound { get; set; } // Звук ящика на ловушке
	[Export] public AudioStream DamageSound { get; set; }  // ЗВУК ПОЛУЧЕНИЯ УРОНА


	private Sprite2D _sprite;
	private bool _isRotating = false;
	private bool _isMoving = false;
	// Возможные направления взгляда робота
	public enum RobotDirection
	{
		Up,    // Вверх
		Right, // Вправо
		Down,  // Вниз
		Left   // Влево
	}
	// Текущее направление
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

	public void EndGame()
	{
		GlobalSignals globals = GetNode("/root/GlobalSignals") as GlobalSignals;

		globals.EmitSignal(GlobalSignals.SignalName.EndGame);
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
	// Проверка финишной зоны (вызов метода зоны)
	private void CheckCurrentZone()
	{
		foreach (var finishZone in FinishZone.AllFinishZones)
		{
			if (finishZone.GridPosition == GridPosition)
			{
				if (!finishZone.HasRobot)
				{
					GD.Print($"Робот вошел в финишную зону в {GridPosition}");
					finishZone.OnRobotEnter(this);
				}
			}
			else
			{
				if (finishZone.HasRobot)
				{
					GD.Print($"Робот вышел из финишной зоны в {finishZone.GridPosition}");
					finishZone.OnRobotExit();
				}
			}
		}
	}

	// Переопределение MoveToGridPosition для отслеживания зон (РОБОТ)
	public override async Task MoveToGridPosition(Vector2I newPosition, float duration = 0.3f)
	{
		// Вызов базовой реализации
		await base.MoveToGridPosition(newPosition, duration);
		
		// Проверка финишной зоны
		CheckCurrentZone();
	}
	
	// Движение вперёд
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
			// Получение направления движения
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
			// Клетка с зоной (финиш или целевая) - просто двигаемся
			else if (targetObject is FinishZone || targetObject is BoxTargetZone)
			{
				await MoveToGridPosition(newPosition, MoveDuration);
				GD.Print($"✓ Шаг {step} выполнен (через зону)");
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
		
		// ВОСПРОИЗВЕДЕНИЕ ЗВУКА УРОНА - ДОБАВЬТЕ ЭТОТ БЛОК
		if (DamageSound != null)
		{
			SFXManager.Instance.PlaySound(DamageSound, -25.0f);
		}

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

	// Получение направления движения
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
	// Получение текущего направления (элемент перечисления)
	public RobotDirection GetCurrentDirection() => _currentDirection; 

	// Нормализация угла в диапазон [0, 2π)
	private float NormalizeAngle(float angle)
	{
		angle = angle % (2 * Mathf.Pi);
		if (angle < 0) angle += 2 * Mathf.Pi;
		return angle;
	}

	// Толкание ОДНОГО объекта
	private async Task PushSingleObject(Vector2I objectPosition, Vector2I direction)
	{
		GD.Print($"РОБОТ: начинаю толкать объект в {objectPosition}");
		
		// Определение объекта толкания и его возможности толкать
		GridObject objectToPush = _grid.GetObjectAt(objectPosition);
		if (objectToPush == null || !objectToPush.CanBePushed) return;
		
		// Определение новой позиции (куда будет перемещён объект) и есть ли возможность
		Vector2I newObjectPos = objectPosition + direction;
		if (!_grid.IsInGridBounds(newObjectPos)) return;
		
		// Объект за толкаемым объектом
		GridObject targetObject = _grid.GetObjectAt(newObjectPos);
		
		// Запоминаем старую позицию толкаемого объекта
		Vector2I oldBoxPosition = objectToPush.GridPosition;
		
		// Проверки
		if (targetObject != null)
		{
			// Ловушка за объектом
			if (targetObject is TrapObject)
			{
				GD.Print($"РОБОТ: объект {objectToPush.ObjectType} толкается на ловушку!");
				await DestroyObjectOnTrap(objectToPush, newObjectPos);
			}
			// Зоны за объектом
			else if (targetObject is BoxTargetZone || targetObject is FinishZone)
			{
				// Разрешаем толкать на обе зоны
				string zoneType = targetObject is BoxTargetZone ? "целевую" : "финишную";
				GD.Print($"РОБОТ: объект {objectToPush.ObjectType} толкается на {zoneType} зону!");
				
				await objectToPush.MoveToGridPosition(newObjectPos, MoveDuration);
				
				// Проверяем зоны только для BoxTargetZone (финишные зоны только для робота)
				if (objectToPush is BoxObject box) CheckBoxZone(box, newObjectPos, oldBoxPosition); 
			}
			else
			{
				GD.PrintErr("РОБОТ: объект нельзя толкнуть - целевая позиция занята!");
				return;
			}
		}
		// За толкающим объектом нет ничего
		else
		{
			GD.Print($"РОБОТ: толкаю {objectToPush.ObjectType} из {objectPosition} в {newObjectPos}");
			await objectToPush.MoveToGridPosition(newObjectPos, MoveDuration);
			
			// Проверяем зоны после движения
			if (objectToPush is BoxObject box) CheckBoxZone(box, newObjectPos, oldBoxPosition);
		}
		
		// Перемещаем робота на позицию прошлого объекта
		await MoveToGridPosition(objectPosition, MoveDuration);
		GD.Print($"РОБОТ: успешно завершил действие");
	}

	// Проверка целевых зон для ящиков
	private void CheckBoxZone(BoxObject box, Vector2I newPosition, Vector2I oldPosition)
	{
		if (box == null) return;
		
		foreach (var targetZone in BoxTargetZone.AllBoxTargetZones)
		{
			// Проверяем выход из старой позиции
			if (targetZone.GridPosition == oldPosition && targetZone.HasBox)
			{
				GD.Print($"Ящик вышел из целевой зоны в {oldPosition}");
				targetZone.OnBoxExit();
			}
			
			// Проверяем вход в новую позицию
			if (targetZone.GridPosition == newPosition && !targetZone.HasBox)
			{
				GD.Print($"Ящик вошел в целевую зону в {newPosition}");
				targetZone.OnBoxEnter(box);
			}
		}
	}

	// Проверка возможности толкания объекта
	private bool CanPushObject(Vector2I objectPosition, Vector2I direction)
	{
		// Координаты в пределах сетки
		if (!_grid.IsInGridBounds(objectPosition)) return false;
		
		// Получение объекта для толкания
		GridObject obj = _grid.GetObjectAt(objectPosition);
		if (obj == null || !obj.CanBePushed) return false;
		
		// Проверка позиции, куда будет вытолкнут объект
		Vector2I nextPos = objectPosition + direction;
		if (!_grid.IsInGridBounds(nextPos)) return false;
		
		// Объект за объектом толкания
		GridObject targetObj = _grid.GetObjectAt(nextPos);
		
		// Можно толкать если 
		return targetObj == null ||       // клетка пустая ИЛИ
			targetObj is TrapObject ||    // содержит ловушку ИЛИ
			targetObj is BoxTargetZone || // содержит целевую зону
			targetObj is FinishZone;      // содержит финишную зону
	}

	// Уничтожение объекта при толкании на ловушку
	// private async Task DestroyObjectOnTrap(GridObject objectToDestroy, Vector2I trapPosition)
	// {
	// 	GD.Print($"УНИЧТОЖЕНИЕ: объект {objectToDestroy.ObjectType} уничтожен ловушкой в {trapPosition}");
		
	// 	// Запоминаем позицию перед уничтожением для выхода из зоны
	// 	Vector2I destroyPosition = objectToDestroy.GridPosition;
		
	// 	// Визуальные эффекты уничтожения - ВЫЗЫВАЕМ МЕТОД ОБЪЕКТА
	// 	await objectToDestroy.OnDestroyed();
		
	// 	// Если это ящик - вызываем выход из зоны
	// 	if (objectToDestroy is BoxObject box) CheckBoxExitOnDestroy(box, destroyPosition); 
		
	// 	// Удаляем объект из сетки
	// 	_grid.RemoveObjectFromGrid(objectToDestroy.GridPosition);
		
	// 	// Уничтожаем объект
	// 	objectToDestroy.QueueFree();
	// }

	// Модифицируем метод DestroyObjectOnTrap
    private async Task DestroyObjectOnTrap(GridObject objectToDestroy, Vector2I trapPosition)
    {
        GD.Print($"УНИЧТОЖЕНИЕ: объект {objectToDestroy.ObjectType} уничтожен ловушкой в {trapPosition}");
        
        // ВОСПРОИЗВОДИМ ЗВУК ЛОВУШКИ
        if (BoxTrapSound != null)
        {
            SFXManager.Instance.PlaySound(BoxTrapSound, -20.0f); // -20.0f - регулируйте громкость
        }
        
        // Запоминаем позицию перед уничтожением для выхода из зоны
        Vector2I destroyPosition = objectToDestroy.GridPosition;
        
        // Визуальные эффекты уничтожения
        await objectToDestroy.OnDestroyed();
        
        // Если это ящик - вызываем выход из зоны
        if (objectToDestroy is BoxObject box) CheckBoxExitOnDestroy(box, destroyPosition); 
        
        // Удаляем объект из сетки
        _grid.RemoveObjectFromGrid(objectToDestroy.GridPosition);
        
        // Уничтожаем объект
        objectToDestroy.QueueFree();
    }

	// Проверка выхода из зоны при УНИЧТОЖЕНИИ ящика
	private void CheckBoxExitOnDestroy(BoxObject box, Vector2I position)
	{
		foreach (var targetZone in BoxTargetZone.AllBoxTargetZones)
		{
			if (targetZone.GridPosition == position && targetZone.HasBox)
			{
				GD.Print($"Уничтоженный ящик выходит из целевой зоны в {position}");
				targetZone.OnBoxExit();
			}
		}
	}


}
