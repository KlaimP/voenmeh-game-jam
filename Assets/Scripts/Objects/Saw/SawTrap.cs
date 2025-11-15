using Godot;

public partial class SawTrap : TrapObject
{
    [Export] public float RotationSpeed { get; set; } = 2.0f;
    [Export] public int Damage { get; set; } = 1;
    
    private Tween _rotationTween;

    public override void _Ready()
    {
        ObjectType = "SAW_TRAP";
        base._Ready();
        StartRotationAnimation();
    }

    private void StartRotationAnimation()
    {
        _rotationTween = CreateTween();
        _rotationTween.SetLoops();
        _rotationTween.TweenProperty(this, "rotation", Mathf.Pi * 2, RotationSpeed);
        _rotationTween.SetTrans(Tween.TransitionType.Linear);
    }

    protected override void ActivateTrap(Robot robot)
    {
        GD.Print($"🔪 Пила активирована! Робот получает урон: {Damage}");
        robot.TakeDamage(Damage);
        
        // Можно добавить визуальные эффекты
        PlayActivationEffects();
    }

    private void PlayActivationEffects()
    {
        // Мигание или другие эффекты при активации
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1, 0.5f, 0.5f, 1), 0.1f);
        tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1), 0.1f);
    }
}