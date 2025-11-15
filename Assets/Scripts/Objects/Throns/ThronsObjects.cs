using Godot;

public partial class ThronsObject : GridObject
{
    public override void _Ready()
    {
        ObjectType = "THRONS";
        IsSolid = false; // Можно наступать
        CanBePushed = false;
        base._Ready();
    }
}