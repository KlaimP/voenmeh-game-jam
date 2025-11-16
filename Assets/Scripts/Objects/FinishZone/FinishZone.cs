using Godot;
using System.Collections.Generic;

public partial class FinishZone : GridObject
{
    public static List<FinishZone> AllFinishZones = new();
    
    public bool HasRobot { get; private set; } = false;
    
    public override void _Ready()
    {
        ObjectType = "FINISH_ZONE";
        IsSolid = false;
        CanBePushed = false;
        base._Ready();
        
        // Добавляем в статический список
        AllFinishZones.Add(this);
        GD.Print($"Финишная зона добавлена в {GridPosition}. Всего зон: {AllFinishZones.Count}");
    }
    
    public override void _ExitTree()
    {
        // Удаляем из списка при уничтожении
        AllFinishZones.Remove(this);
        base._ExitTree();
    }
    
    public void OnRobotEnter(Robot robot)
    {
        if (!HasRobot)
        {
            HasRobot = true;
            GD.Print($"🏁 Робот зафиксирован в финишной зоне в {GridPosition}");
            PlayActivationEffect();
        }
    }

    public void OnRobotExit()
    {
        if (HasRobot)
        {
            HasRobot = false;
            GD.Print($"🏁 Робот покинул финишную зону в {GridPosition}");
            PlayDeactivationEffect();
        }
    }
    
    private void PlayActivationEffect()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(0, 1, 1, 1), 0.2f);
    }
    
    private void PlayDeactivationEffect()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.2f);
    }
}