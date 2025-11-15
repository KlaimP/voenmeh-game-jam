using Godot;
using System;
using System.Threading.Tasks;

public partial class GridObject : Node2D
{
    [Export] public Vector2I GridPosition { get; protected set; }
    [Export] public string ObjectType { get; protected set; } = "BASE";
    [Export] public bool IsSolid { get; protected set; } = false;
    [Export] public bool CanBePushed { get; protected set; } = false; // Новое свойство
    
    [Signal] public delegate void ObjectMovedEventHandler(Vector2I fromPosition, Vector2I toPosition);

    protected Grid _grid;
    protected Tween _moveTween;

    public override void _Ready()
    {
        InitializeGridReference();
        UpdateWorldPosition();
    }

    protected virtual void InitializeGridReference()
    {
        _grid = GetParent() as Grid;
        if (_grid == null)
        {
            _grid = GetTree().CurrentScene.GetNode<Grid>("Grid");
        }
        
        if (_grid == null)
        {
            Node parent = GetParent();
            while (parent != null && _grid == null)
            {
                _grid = parent as Grid;
                parent = parent.GetParent();
            }
        }
    }

    public virtual void SetGridPosition(Vector2I newPosition)
    {
        if (!IsValidGridPosition(newPosition)) 
            return;

        Vector2I oldPosition = GridPosition;
        GridPosition = newPosition;
        
        UpdateWorldPosition();
        EmitSignal(SignalName.ObjectMoved, oldPosition, newPosition);
    }

    public virtual async Task MoveToGridPosition(Vector2I newPosition, float duration = 0.3f)
    {
        if (!CanMoveToPosition(newPosition)) 
            return;

        Vector2I oldPosition = GridPosition;
        Vector2 targetWorldPos = _grid.GridToWorld(newPosition);

        _moveTween = CreateTween();
        _moveTween.SetEase(Tween.EaseType.Out);
        _moveTween.SetTrans(Tween.TransitionType.Cubic);
        _moveTween.TweenProperty(this, "global_position", targetWorldPos, duration);
        
        await ToSignal(_moveTween, "finished");
        
        GridPosition = newPosition;
        
        if (_grid != null)
        {
            _grid.MoveObject(this, newPosition);
        }
        
        EmitSignal(SignalName.ObjectMoved, oldPosition, newPosition);
    }

    protected virtual void UpdateWorldPosition()
    {
        if (_grid != null)
        {
            GlobalPosition = _grid.GridToWorld(GridPosition);
        }
    }

    protected virtual bool IsValidGridPosition(Vector2I position)
    {
        return _grid != null && _grid.IsInGridBounds(position);
    }

    public virtual bool CanMoveToPosition(Vector2I targetPosition)
    {
        if (!IsValidGridPosition(targetPosition)) 
            return false;

        if (_grid != null)
        {
            var objectsAtTarget = _grid.GetObjectsAt(targetPosition);
            foreach (var obj in objectsAtTarget)
            {
                if (obj.IsSolid && obj != this)
                {
                    // Если это ящик, проверяем можно ли его толкнуть дальше
                    if (obj is BoxObject box && box.CanBePushed)
                    {
                        Vector2I pushDirection = targetPosition - GridPosition;
                        Vector2I boxTargetPosition = targetPosition + pushDirection;
                        return box.CanMoveToPosition(boxTargetPosition);
                    }
                    return false;
                }
            }
        }

        return true;
    }
}