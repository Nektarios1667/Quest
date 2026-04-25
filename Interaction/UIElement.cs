namespace Quest.Interaction;
public abstract class UIElement
{
    public List<string> Tags { get; set; } = [];
    public Point Location { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public UIElement(Point location)
    {
        Location = location;
    }
    public abstract void Update(UserInterface ui);
    public abstract void Draw(UserInterface ui);
    public virtual void Tag(string name) => Tags.Add(name);
    public virtual void Untag(string name) => Tags.Remove(name);
    public virtual void Hide() => IsVisible = false;
    public virtual void Show() => IsVisible = true;
    public virtual void ToggleShow() => IsVisible = !IsVisible;
    public virtual void Enable() => IsEnabled = true;
    public virtual void Disable() => IsEnabled = false;
    public virtual void ToggleEnable() => IsEnabled = !IsEnabled;

}
