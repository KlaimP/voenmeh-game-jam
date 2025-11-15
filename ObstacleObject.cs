using Godot;

public partial class ObstacleObject : GridObject
{
	private Sprite2D _sprite;

	public override void _Ready()
	{
		ObjectType = "OBSTACLE";
		IsSolid = true;
		CanBePushed = false;
		
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		base._Ready();
	}
}
