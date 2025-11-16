using Godot;
using System.Collections.Generic;

public partial class BoxTargetZone : GridObject
{
    public static List<BoxTargetZone> AllBoxTargetZones = new();
    
    public bool HasBox { get; private set; } = false;
    
    public override void _Ready()
    {
        ObjectType = "BOX_TARGET_ZONE";
        IsSolid = false;
        CanBePushed = false;
        base._Ready();
        
        // Добавляем в статический список
        AllBoxTargetZones.Add(this);
        GD.Print($"Целевая зона для ящиков добавлена в {GridPosition}. Всего зон: {AllBoxTargetZones.Count}");
    }
    
    public override void _ExitTree()
    {
        // Удаляем из списка при уничтожении
        AllBoxTargetZones.Remove(this);
        base._ExitTree();
    }
    
    public void OnBoxEnter(BoxObject box)
    {
        if (!HasBox)
        {
            HasBox = true;
            GD.Print($"🎯 Ящик зафиксирован в целевой зоне в {GridPosition}");
            PlayActivationEffect();
        }
    }

    public void OnBoxExit()
    {
        if (HasBox)
        {
            HasBox = false;
            GD.Print($"🎯 Ящик покинул целевую зону в {GridPosition}");
            PlayDeactivationEffect();
        }
    }
    
    private void PlayActivationEffect()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(0, 1, 0, 1), 0.2f);
    }
    
    private void PlayDeactivationEffect()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.2f);
    }
}