using Godot;

/* Базовый класс: Ловушки
   Наследование: GridObject
   (Родитель всех ловушек на сетке) 
*/
public partial class TrapObject : GridObject
{    
	// Инициализация ловушки
    public override void _Ready()
    {
        base._Ready();
        // Ловушки не твердые - можно наступать
        IsSolid = false; 
        // Ловушки нельзя двигать
        CanBePushed = false;
    }

	// Функция, что на ловушку наступил робот
    public override void OnRobotEnter(Robot robot) => ActivateTrap(robot);

	// Переопределяемая функция активации ловушки
    protected virtual void ActivateTrap(Robot robot) { }
}