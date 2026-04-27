using Godot;

public partial class HealthBar : Node2D
{
    private DefensiveAttributes defensiveAttributes;
    private float lastHealth = -1f;
    private float lastMaxHealth = -1f;

    public float BarWidth { get; set; } = 40f;
    public float BarHeight { get; set; } = 6f;
    public Color BackgroundColor { get; set; } = new Color(0.12f, 0.12f, 0.12f, 0.9f);
    public Color FillColor { get; set; } = new Color(0.16f, 0.8f, 0.3f, 0.95f);
    public Color BorderColor { get; set; } = new Color(0f, 0f, 0f, 1f);

    public void SetAttributes(DefensiveAttributes attributes)
    {
        defensiveAttributes = attributes;
        lastHealth = -1f;
        lastMaxHealth = -1f;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (defensiveAttributes == null)
        {
            return;
        }

        if (!Mathf.IsEqualApprox(defensiveAttributes.Health, lastHealth) ||
            !Mathf.IsEqualApprox(defensiveAttributes.MaxHealth, lastMaxHealth))
        {
            lastHealth = defensiveAttributes.Health;
            lastMaxHealth = defensiveAttributes.MaxHealth;
            BarWidth = 5 * Mathf.Sqrt(lastMaxHealth); // Example scaling, can be adjusted
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (defensiveAttributes == null || defensiveAttributes.MaxHealth <= 0f)
        {
            return;
        }

        float healthRatio = Mathf.Clamp(defensiveAttributes.Health / defensiveAttributes.MaxHealth, 0f, 1f);
        Rect2 barRect = new Rect2(-BarWidth * 0.5f, 0f, BarWidth, BarHeight);
        Rect2 fillRect = new Rect2(barRect.Position, new Vector2(BarWidth * healthRatio, BarHeight));

        DrawRect(barRect, BackgroundColor, true);
        if (fillRect.Size.X > 0f)
        {
            DrawRect(fillRect, FillColor, true);
        }
        DrawRect(barRect, BorderColor, false, 1f);
    }
}