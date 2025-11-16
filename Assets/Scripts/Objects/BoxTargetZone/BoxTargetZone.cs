using Godot;
using System.Collections.Generic;

/* Объект - Целевые зоны ящиков
   Наследование: GridObject
*/
public partial class BoxTargetZone : GridObject
{
    // Все используемые целевые зоны ящиков
    public static List<BoxTargetZone> AllBoxTargetZones = new();
    // Флаг присутствия ящика
    public bool HasBox { get; private set; } = false;
    
    // Инициализация
    public override void _Ready()
    {
        ObjectType = "BOX_TARGET_ZONE";
        IsSolid = false;     // Нетвёрдая
        CanBePushed = false; // Нельзя двигать
        base._Ready();
        
        // Добавляем в статический список
        AllBoxTargetZones.Add(this);
        GD.Print($"Целевая зона для ящиков добавлена в {GridPosition}. Всего зон: {AllBoxTargetZones.Count}");
    }
    
    // Удаление из дерева
    public override void _ExitTree()
    {
        // Удаляем из списка при уничтожении
        AllBoxTargetZones.Remove(this);
        base._ExitTree();
    }
    

    
    // Фиксация входа ящика в зону
    public void OnBoxEnter(BoxObject box)
    {
        if (!HasBox)
        {
            HasBox = true;
            GD.Print($"🎯 Ящик зафиксирован в целевой зоне в {GridPosition}");
            PlayActivationEffect();
        }
    }

    // Фиксация выхода ящика из зоны
    public void OnBoxExit()
    {
        if (HasBox)
        {
            HasBox = false;
            GD.Print($"🎯 Ящик покинул целевую зону в {GridPosition}");
            PlayDeactivationEffect();
        }
    }
    
    // Анимация эффекта входа
    private void PlayActivationEffect()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(0, 1, 0, 1), 0.2f);
    }
    
    // Анимация эффекта выхода
    private void PlayDeactivationEffect()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.2f);
    }
}