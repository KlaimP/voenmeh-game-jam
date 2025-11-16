using Godot;
using System;

public partial class ZoomCamera2D : Camera2D
{
	[Export] public MouseButton PanButton = MouseButton.Middle;
	[Export] public bool PanWithSpace = true;
	[Export] public float PanSpeed = 1.0f;
	[Export] public SplitContainer Split;
	private bool dragging = false;
	private Vector2 dragMouseWorld;
	private Vector2 dragCamStart;
	private Vector2 targetPosition;

	public override void _Ready()
	{
		targetPosition = GlobalPosition;
	}
	private bool IsMouseBeyondLimit()
	{
		float screenX = GetViewport().GetMousePosition().X;
		return screenX > Split.SplitOffset;
	}
	public override void _PhysicsProcess(double delta)
	{
		if (!IsMouseBeyondLimit())
		{
			if (Input.IsActionJustPressed("camera_zoom_in"))
			{
				if (Zoom.X * 1.1f >= 2.0f)
				{
					return;
				}
				Zoom = new Vector2(Zoom.X * 1.1f, Zoom.Y * 1.1f);
			}
			if (Input.IsActionJustPressed("camera_zoom_out"))
			{
				if (Zoom.X * 0.9f <= 0.5f)
				{
					return;
				}
				Zoom = new Vector2(Zoom.X * 0.9f, Zoom.Y * 0.9f);
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb2)
		{
			if (mb2.ButtonIndex == PanButton && mb2.Pressed)
			{
				dragging = true;
				dragMouseWorld = GetGlobalMousePosition();
				dragCamStart = GlobalPosition;
			}
			else if (mb2.ButtonIndex == PanButton && !mb2.Pressed)
			{
				dragging = false;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (dragging && !IsMouseBeyondLimit())
		{
			var cur = GetGlobalMousePosition();
			var diff = (cur - dragMouseWorld) * PanSpeed;
			targetPosition = dragCamStart - diff;
		}

		targetPosition.X = Mathf.Clamp(targetPosition.X, LimitLeft, LimitRight);
		targetPosition.Y = Mathf.Clamp(targetPosition.Y, LimitTop, LimitBottom);

		// Плавная интерполяция через Mathf.Lerp по компонентам
		float t = Mathf.Clamp(10f * (float)delta, 0f, 1f);
		GlobalPosition = new Vector2(
			Mathf.Lerp(GlobalPosition.X, targetPosition.X, t),
			Mathf.Lerp(GlobalPosition.Y, targetPosition.Y, t)
		);
	}

}
