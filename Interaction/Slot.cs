namespace Quest.Interaction;

public class Slot : UIElement
{
    public event Action? OnClicked;
    public event Action? OnDropped;
    public event Action? OnHovered;
    public event Action? OnItemChange;
    public Rectangle Bounds { get; protected set; }
    public ButtonState State { get; protected set; } = ButtonState.Normal;
    public Item? Item { get; protected set; }
    public Color Color { get; set; } = Color.White;
    public Color? Marked { get; protected set; }
    // Consts/statics
    protected static readonly Point slotOffset = new(14, 14);
    protected const float itemScale = 3;
    public static readonly Point SlotSize = TextureManager.Metadata[TextureID.Slot].Size;
    public Slot(Point location) : base(location)
    {
        Bounds = new Rectangle(Location, SlotSize);
    }
    public override void Update(UserInterface ui)
    {
        // Clicking
        if (Bounds.Contains(InputManager.MousePosition))
        {
            if (InputManager.LMouseClicked || InputManager.RMouseClicked)
            {
                State = ButtonState.Pressed;
                OnClicked?.Invoke();
            }
            else if (InputManager.LMouseDown || InputManager.RMouseDown)
            {
                State = ButtonState.Pressed;
            }
            else
            {
                OnHovered?.Invoke();
                State = ButtonState.Hovered;
            }
        }
        else
            State = ButtonState.Normal;

        // Dropping
        if ((State == ButtonState.Hovered || State == ButtonState.Pressed) && InputManager.BindDown(InputAction.DropItem))
            OnDropped?.Invoke();

    }
    public void Mark(Color color)
    {
        Marked = color;
    }
    public override void Draw(UserInterface ui)
    {
        // Draw inventory slots
        DrawTexture(ui.Batch, TextureID.Slot, Location, color: Marked ?? Color);
        Marked = null;
        // Respond to hover/press states
        if (State == ButtonState.Hovered)
            ui.Batch.FillRectangle(Bounds, Color.SlateGray * 0.3f);

        // Draw inventory items
        if (Item == null) return;
        DrawTexture(ui.Batch, Item.Texture, Location + slotOffset, scale: itemScale);

        // Amount text
        if (Item.Amount <= 1) return; // Don't draw amount text for single items
        Vector2 textDest = Location.ToVector2() + SlotSize.ToVector2() - new Vector2(PixelOperatorCharSize.X * Item.Amount.ToString().Length + 6, 36);
        ui.Batch.DrawString(PixelOperatorBold, $"{Item.Amount}", textDest, Color.White);
    }
    public virtual bool SetItem(Item? item)
    {
        bool changed = Item != item;
        Item = item;
        if (changed)
            OnItemChange?.Invoke();
        return true;
    }
    public virtual bool CanAccept(Item? item) => true;
}
