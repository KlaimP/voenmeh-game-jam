using Godot;

public partial class ThornsTrap : TrapObject
{
    [Export] public int Damage { get; set; } = 2;
 
    public override void _Ready()
    {
        ObjectType = "THORNS_TRAP";
        base._Ready();
    }

    protected override void ActivateTrap(Robot robot)
    {
        GD.Print($"🦴 Шипы активированы! Робот получает урон: {Damage}");
        robot.TakeDamage(Damage);
    }
}