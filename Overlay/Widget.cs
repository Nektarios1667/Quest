namespace Quest.Gui;
public enum HorizontalAlignment
{
    Left,
    Center,
    Right,
}
public enum VerticalAlignment
{
    Top,
    Bottom,
}
public abstract class Widget
{
    public bool Expired { get; protected set; } = false;
    public Point Position { get; set; }
    public bool IsVisible { get; set; } = true;
    public Widget(Point location)
    {
        Position = location;
    }
    public abstract void Draw(SpriteBatch batch);
    public virtual void Update(float deltaTime) { }
}
