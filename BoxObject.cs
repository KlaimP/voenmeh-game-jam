using Godot;
using System.Threading.Tasks;

public partial class BoxObject : GridObject
{
    private Sprite2D _sprite;

    public override void _Ready()
    {
        ObjectType = "BOX";
        IsSolid = true;
        CanBePushed = true;
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        base._Ready();
    }

    public async Task<bool> TryPush(Vector2I direction)
    {
        Vector2I newPosition = GridPosition + direction;
        
        if (CanMoveToPosition(newPosition))
        {
            await MoveToGridPosition(newPosition, 0.2f);
            return true;
        }
        
        return false;
    }
}