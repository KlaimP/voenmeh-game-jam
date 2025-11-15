using Godot;

public partial class BoxObject : GridObject
{
    public override void _Ready()
    {
        ObjectType = "BOX";
        IsSolid = true;
        CanBePushed = true;
        base._Ready();
    }
}