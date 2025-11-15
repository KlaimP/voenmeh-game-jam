using Godot;
using System.Collections.Generic;

public partial class Grid : Node2D
{
	[Export] public int GridWidth = 8;
	[Export] public int GridHeight = 8;
	[Export] public int CellSize = 64;
	[Export] public Color GridColor = new Color(0.3f, 0.3f, 0.3f);
	[Export] public Color BackgroundColor = new Color(0.1f, 0.1f, 0.1f);
	
	[Export] public bool ShowCellNumbers { get; set; } = true;

	private Dictionary<Vector2I, List<GridObject>> _gridObjects = new();

	public override void _Ready()
	{
		QueueRedraw();
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(0, 0, GridWidth * CellSize, GridHeight * CellSize), BackgroundColor);
		
		for (int x = 0; x <= GridWidth; x++)
		{
			Vector2 start = new Vector2(x * CellSize, 0);
			Vector2 end = new Vector2(x * CellSize, GridHeight * CellSize);
			DrawLine(start, end, GridColor, 2);
		}
		
		for (int y = 0; y <= GridHeight; y++)
		{
			Vector2 start = new Vector2(0, y * CellSize);
			Vector2 end = new Vector2(GridWidth * CellSize, y * CellSize);
			DrawLine(start, end, GridColor, 2);
		}
		
		if (ShowCellNumbers)
		{
			DrawCellCoordinates();
		}
	}
	
	private void DrawCellCoordinates()
	{
		for (int x = 0; x < GridWidth; x++)
		{
			for (int y = 0; y < GridHeight; y++)
			{
				string coordText = $"{x+1},{y+1}";
				Vector2 position = new Vector2(x * CellSize + 5, y * CellSize + 15);
				DrawString(ThemeDB.FallbackFont, position, coordText, HorizontalAlignment.Left, -1, 12);
			}
		}
	}

	public bool AddObjectToGrid(GridObject gridObject, Vector2I position)
	{
		if (!IsInGridBounds(position)) 
			return false;

		if (!_gridObjects.ContainsKey(position))
		{
			_gridObjects[position] = new List<GridObject>();
		}

		if (HasSolidObjectAt(position) && gridObject.IsSolid)
		{
			return false;
		}

		_gridObjects[position].Add(gridObject);
		gridObject.SetGridPosition(position);
		
		return true;
	}

	public void MoveObject(GridObject obj, Vector2I newPosition)
	{
		if (!IsInGridBounds(newPosition)) 
			return;

		if (_gridObjects.ContainsKey(obj.GridPosition))
		{
			_gridObjects[obj.GridPosition].Remove(obj);
		}

		if (!_gridObjects.ContainsKey(newPosition))
		{
			_gridObjects[newPosition] = new List<GridObject>();
		}

		_gridObjects[newPosition].Add(obj);
		obj.SetGridPosition(newPosition);
	}

	public List<GridObject> GetObjectsAt(Vector2I position)
	{
		if (_gridObjects.ContainsKey(position))
			return new List<GridObject>(_gridObjects[position]);
		
		return new List<GridObject>();
	}

	public bool HasSolidObjectAt(Vector2I position)
	{
		var objects = GetObjectsAt(position);
		foreach (var obj in objects)
		{
			if (obj.IsSolid)
				return true;
		}
		return false;
	}

	public Vector2I WorldToGrid(Vector2 worldPosition)
	{
		int gridX = (int)(worldPosition.X / CellSize);
		int gridY = (int)(worldPosition.Y / CellSize);
		return new Vector2I(gridX, gridY);
	}

	public Vector2 GridToWorld(Vector2I gridPosition)
	{
		float worldX = gridPosition.X * CellSize + CellSize / 2f;
		float worldY = gridPosition.Y * CellSize + CellSize / 2f;
		return new Vector2(worldX, worldY);
	}

	public bool IsInGridBounds(Vector2I gridPosition)
	{
		return gridPosition.X >= 0 && gridPosition.X < GridWidth && 
			   gridPosition.Y >= 0 && gridPosition.Y < GridHeight;
	}

	public void UpdateGrid()
	{
		QueueRedraw();
	}
}
