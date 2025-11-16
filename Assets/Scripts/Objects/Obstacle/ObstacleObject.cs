using Godot;

/* Объект - Стенка
   Наследование: GridObject
*/
public partial class ObstacleObject : GridObject
{
	// Инициализация
	public override void _Ready()
	{
		ObjectType = "OBSTACLE";
		IsSolid = true;      // Твёрдая
		CanBePushed = false; // Нельзя двигать
		base._Ready();
	}
}
