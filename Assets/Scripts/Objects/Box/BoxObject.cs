using Godot;

/* Объект - Ящик
   Наследование: GridObject
*/
public partial class BoxObject : GridObject
{
    // Инициализация
    public override void _Ready()
    {
        ObjectType = "BOX";
        IsSolid = true;     // Твёрдый
        CanBePushed = true; // Можно двигать
        base._Ready();
    }
}