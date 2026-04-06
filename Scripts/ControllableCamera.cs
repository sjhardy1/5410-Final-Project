using Godot;
using System;

public partial class ControllableCamera : Camera2D
{
	[Export]
	public float[] zoomRange = {0.5f, 2f};
	private Rect2 dragArea;
	private bool dragging = false;
	private bool active = true;
	private Vector2 lastMousePos;
	public void Initialize(Rect2 dragArea)
	{
		this.dragArea = dragArea;
	}
	public void EnableControls()
	{
		active = true;
	}
	public void DisableControls()
	{
		active = false;
	}
	public override void _Input(InputEvent @event)
	{
		if (!active) return;
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseEvent.Pressed)
				{
					if (dragArea.HasPoint(mouseEvent.Position))
					{
						dragging = true;
						lastMousePos = mouseEvent.Position;
					}
				}
				else
				{
					dragging = false;
				}
			}
		}
		else if (@event is InputEventMouseMotion motionEvent && dragging)
		{
			Vector2 delta = motionEvent.Position - lastMousePos;
			GlobalPosition -= delta / Zoom; // Adjust for zoom level
			lastMousePos = motionEvent.Position;
		}
		if(@event is InputEventMouseButton zoomEvent)
		{
			if (zoomEvent.ButtonIndex == MouseButton.WheelDown)
			{
				Zoom = new Vector2(Mathf.Clamp(Zoom.X * 0.9f, zoomRange[0], zoomRange[1]), Mathf.Clamp(Zoom.Y * 0.9f, zoomRange[0], zoomRange[1]));
			}
			else if (zoomEvent.ButtonIndex == MouseButton.WheelUp)
			{
				Zoom = new Vector2(Mathf.Clamp(Zoom.X * 1.1f, zoomRange[0], zoomRange[1]), Mathf.Clamp(Zoom.Y * 1.1f, zoomRange[0], zoomRange[1]));
			}
		}
	}
}
