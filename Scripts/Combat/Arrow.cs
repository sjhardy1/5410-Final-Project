using Godot;
using System;

public partial class Arrow : Sprite2D
{
	private Vector2 targetGlobalPosition;
	private float speed = 400f;
	private float liveTime = 0.25f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Timer timer = new Timer();
		timer.WaitTime = liveTime;
		timer.Autostart = true;
		AddChild(timer);
		timer.Timeout += () => QueueFree();
	}
	public void Initialize(Vector2 targetGlobalPosition, float speed, float liveTime = 0.25f)
	{
		this.targetGlobalPosition = targetGlobalPosition;
		this.speed = speed;
		this.liveTime = liveTime;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = (targetGlobalPosition - GlobalPosition).Normalized();
		Rotation = direction.Angle();
		GlobalPosition += direction * speed * (float)delta;
	}
}
