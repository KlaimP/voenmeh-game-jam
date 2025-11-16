using Godot;

/* Объект - Шипы
   Наследование: TrapObject <- GridObject
*/
public partial class ThornsTrap : TrapObject
{
	// Размер дамага
	[Export] public int Damage { get; set; } = 2;

	// Инициализация
	public override void _Ready()
	{
		ObjectType = "THORNS_TRAP";
		base._Ready();
	}

	// Переопределение функции удара робота
	protected override void ActivateTrap(Robot robot)
	{
		GD.Print($"🦴 Шипы активированы! Робот получает урон: {Damage}");
		robot.TakeDamage(Damage);
	}
}
