using Godot;
using Godot.Collections;

public partial class GridPlaceable : Node2D
{

    public PlaceableDefinition def { get; private set; }
    [Export] public bool BlocksMovement { get; set; } = true;
    [Export] public float HoldDurationSeconds { get; set; } = 0.5f;
    private bool isMouseHovering = false;
    private bool isMousePressed = false;
    private float holdAccumulator = 0f;
    private bool holdAlreadyEmitted = false;
    public bool storage = true;
    private bool placing = false;
    public Vector2I AnchorCell { get; set; }
    public override void _Ready()
    {
        GetNode<SignalBus>("/root/SignalBus").Placing += () => placing = true;
        GetNode<SignalBus>("/root/SignalBus").StopPlacing += () => placing = false;
    }

    public override void _ExitTree()
    {
        GetNode<SignalBus>("/root/SignalBus").Placing -= () => placing = true;
        GetNode<SignalBus>("/root/SignalBus").StopPlacing -= () => placing = false;
    }

    public Array<Vector2I> GetOccupiedCells()
    {
        var occupied = new Array<Vector2I>();
        if (def.Footprint == null)
        {
            occupied.Add(AnchorCell);
            return occupied;
        }

        Array<Vector2I> offsets = def.Footprint.GetOffsets();
        foreach (Vector2I offset in offsets)
        {
            occupied.Add(AnchorCell + offset);
        }

        return occupied;
    }

    public override void _Input(InputEvent @event)
    {
        if(storage || placing) return;
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    // Check if mouse is over this object when button is pressed
                    if (IsMouseOver())
                    {
                        isMousePressed = true;
                        isMouseHovering = true;
                        holdAccumulator = 0f;
                        holdAlreadyEmitted = false;
                    }
                }
                else
                {
                    // Mouse button released
                    isMousePressed = false;
                    holdAccumulator = 0f;
                    holdAlreadyEmitted = false;
                }
            }
        }
        else if (@event is InputEventMouseMotion)
        {
            // Update hover state based on current mouse position
            isMouseHovering = IsMouseOver();
            
            // If button is pressed but mouse moved away, cancel the hold
            if (isMousePressed && !isMouseHovering)
            {
                holdAccumulator = 0f;
                holdAlreadyEmitted = false;
            }
        }
    }

    public override void _Process(double delta)
    {
        if(storage || placing) return;

        // Only accumulate hold time if mouse is pressed and hovering
        if (isMousePressed && isMouseHovering && !holdAlreadyEmitted)
        {
            holdAccumulator += (float)delta;
            
            // Check if hold duration has been reached
            if (holdAccumulator >= HoldDurationSeconds)
            {
                GetNode<SignalBus>("/root/SignalBus").PublishPlaceableRemovedFromActive(this);
                GetNode<SignalBus>("/root/SignalBus").PublishClearCells();
                holdAlreadyEmitted = true;
            }
        }
    }

    private bool IsMouseOver()
    {
        // Get the global mouse position
        Vector2 mousePos = GetGlobalMousePosition();
        
        // Get the node's global position
        Vector2 nodePos = GlobalPosition;
        if(def is UnitDefinition)
        {
            nodePos -= new Vector2(64, 64) / 2;
        }
        
        foreach (Vector2I cellOffset in def.Footprint.GetOffsets())
        {
            Vector2 cellWorldPos = nodePos + new Vector2(cellOffset.X * 64, cellOffset.Y * 64);
            Rect2 cellRect = new Rect2(cellWorldPos, new Vector2(64, 64));
            if (cellRect.HasPoint(mousePos))
            {
                return true;
            }
        }
        return false;
    }

    public void Initialize(PlaceableDefinition def)
    {
        this.def = def;
        AnchorCell = Vector2I.Zero;
        storage = false;
    }
}
