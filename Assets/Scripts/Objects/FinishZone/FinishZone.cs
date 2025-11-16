using Godot;
using System.Collections.Generic;

/* Объект - Финишная зона робота
   Наследование: GridObject
*/
public partial class FinishZone : GridObject
{
    // Все финишные зоны (для расширения программы)
    public static List<FinishZone> AllFinishZones = new();
    // Флаг присутствия в зоне робота
    public bool HasRobot { get; private set; } = false;
    
    // Инициализация
    public override void _Ready()
    {
        ObjectType = "FINISH_ZONE";
        IsSolid = false;     // Нетвёрдая
        CanBePushed = false; // Нельзя двигать
        base._Ready();
        
        // Добавляем в статический список
        AllFinishZones.Add(this);
        GD.Print($"Финишная зона добавлена в {GridPosition}. Всего зон: {AllFinishZones.Count}");
    }
    
    // Удаление из дерева
    public override void _ExitTree()
    {
        // Удаляем из списка при уничтожении
        AllFinishZones.Remove(this);
        base._ExitTree();
    }
    


    // Фиксация входа робота в зону
    public void OnRobotEnter(Robot robot)
    {
        if (!HasRobot)
        {
            HasRobot = true;
            GD.Print($"🏁 Робот зафиксирован в финишной зоне в {GridPosition}");
            PlayActivationEffect();
        }
    }

    // Фиксация выхода робота из зоны
    public void OnRobotExit()
    {
        if (HasRobot)
        {
            HasRobot = false;
            GD.Print($"🏁 Робот покинул финишную зону в {GridPosition}");
            PlayDeactivationEffect();
        }
    }
    
    // Анимация эффекта входа
    private void PlayActivationEffect()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(0, 1, 1, 1), 0.2f);
    }
    
    // Анимация эффекта выхода
    private void PlayDeactivationEffect()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.2f);
    }
}