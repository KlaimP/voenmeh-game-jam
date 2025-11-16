using Godot;

/* Объект - Лазер
   Наследование: TrapObject <- GridObject
*/
public partial class LaserTrap : TrapObject
{
    // Текстуры лазера (назначь в префабе)
    [Export] public Texture2D EndpointTexture { get; set; }  // Для начала и конца
    [Export] public Texture2D BeamTexture { get; set; }      // Для луча
    
    // Тип сегмента лазера
    public enum LaserSegmentType
    {
        Start,    // Начальный сегмент
        End,      // Конечный сегмент  
        Beam      // Промежуточный луч
    }
    
    [Export] public LaserSegmentType SegmentType { get; set; } = LaserSegmentType.Beam;
	// Размер дамага
	[Export] public int Damage { get; set; } = 3;
    private Sprite2D _sprite;

    // Инициализация
    public override void _Ready()
    {
        ObjectType = "LASER_TRAP";
        IsSolid = false;
        CanBePushed = false;
        
        // Получаем спрайт
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        
        // Устанавливаем текстуру в зависимости от типа сегмента
        if (SegmentType == LaserSegmentType.Start || SegmentType == LaserSegmentType.End)
        {
            if (_sprite != null && EndpointTexture != null)
            {
                _sprite.Texture = EndpointTexture;
            }
        }
        else
        {
            if (_sprite != null && BeamTexture != null)
            {
                _sprite.Texture = BeamTexture;
            }
        }
        
        base._Ready();
        
        GD.Print($"Лазер создан: тип={SegmentType}, позиция={GridPosition}");
    }

	public void UpdateLaserTexture()
	{
		if (_sprite == null)
			_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
			
		if (_sprite != null)
		{
			if (SegmentType == LaserSegmentType.Start || SegmentType == LaserSegmentType.End)
			{
				if (EndpointTexture != null)
					_sprite.Texture = EndpointTexture;
			}
			else
			{
				if (BeamTexture != null)
					_sprite.Texture = BeamTexture;
			}
		}
	}

    // Переопределение функции активации - КОНЕЦ ИГРЫ
    protected override void ActivateTrap(Robot robot)
    {
        GD.Print($"🔴💀 ЛАЗЕР АКТИВИРОВАН! РОБОТ УНИЧТОЖЕН!");
		robot.TakeDamage(Damage);
    }

}