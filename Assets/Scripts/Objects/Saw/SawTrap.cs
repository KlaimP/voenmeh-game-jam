using Godot;

/* Объект - Пила
   Наследование: TrapObject <- GridObject
*/
public partial class SawTrap : TrapObject
{
	// Размер дамага
	[Export] public int Damage { get; set; } = 1;

	// Инициализация
	public override void _Ready()
	{
		ObjectType = "SAW_TRAP";
		base._Ready();
	}

	// Переопределение функции удара робота
	protected override void ActivateTrap(Robot robot)
	{
		GD.Print($"🔪 Пила активирована! Робот получает урон: {Damage}");
		robot.TakeDamage(Damage);
	}
}
