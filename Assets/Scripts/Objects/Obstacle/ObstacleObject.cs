using Godot;

public partial class ObstacleObject : GridObject
{
	public override void _Ready()
	{
		ObjectType = "OBSTACLE";
		IsSolid = true;
		CanBePushed = false;
		base._Ready();
	}
}
