using Godot;

/* Базовый класс: Ловушки
   (Родитель всех ловушек на сетке) 
*/
public partial class TrapObject : GridObject
{    
	// Инициализация ловушки
    public override void _Ready()
    {
        base._Ready();
        IsSolid = false; // Ловушки не твердые - можно наступать
        CanBePushed = false;
    }

	// Функция, что на ловушку наступил робот
    public override void OnRobotEnter(Robot robot) => ActivateTrap(robot);

	// Переопределяемая функция активации ловушки
    protected virtual void ActivateTrap(Robot robot)
    {
        // Переопределяется в дочерних классах
    }
}