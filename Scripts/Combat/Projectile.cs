using Godot;
using System;

public partial class Projectile : Node2D
{
	private Vector2 targetGlobalPosition;
	private float speed = 400f;
	private float liveTime = 0.25f;
	private PackedScene hitEffectScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ZIndex = 100;
		Timer timer = new Timer();
		timer.WaitTime = liveTime;
		timer.Autostart = true;
		AddChild(timer);
		timer.Timeout += OnTimeout;
	}
	public void Initialize(Vector2 targetGlobalPosition, float speed, float liveTime = 0.25f, PackedScene hitEffectScene = null)
	{
		this.targetGlobalPosition = targetGlobalPosition;
		this.speed = speed;
		this.liveTime = liveTime;
		this.hitEffectScene = hitEffectScene;
	}
	public void OnTimeout()
	{
		if (hitEffectScene != null)
		{
			AnimatedSprite2D hitEffectInstance = hitEffectScene.Instantiate<AnimatedSprite2D>();
			GetParent().AddChild(hitEffectInstance);
			hitEffectInstance.GlobalPosition = targetGlobalPosition;
			hitEffectInstance.ZIndex = 100;
			hitEffectInstance.AnimationLooped += () =>
			{
				hitEffectInstance.QueueFree();
			};
		}
		QueueFree();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = (targetGlobalPosition - GlobalPosition).Normalized();
		Rotation = direction.Angle();
		GlobalPosition += direction * speed * (float)delta;
	}
}
