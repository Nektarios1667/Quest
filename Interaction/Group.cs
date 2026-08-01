namespace Quest.Interaction;

public class Group : UIElement
{
    public List<string> Elements { get; private set; }
    private UserInterface UI { get; }
    public Group(UserInterface ui, List<string>? elements = null) : base(Point.Zero)
    {
        Elements = elements ?? [];
        UI = ui;
    }
    public override void Update(UserInterface ui, GameManager gameManager) { }
    public override void Draw(UserInterface ui) { }
    public override void Enable()
    {
        foreach (string element in Elements)
            UI.GetAllElements()[element].Enable();
    }
    public override void Disable()
    {
        foreach (string element in Elements)
            UI.GetAllElements()[element].Disable();
    }
    public override void ToggleEnable()
    {
        foreach (string element in Elements)
            UI.GetAllElements()[element].ToggleEnable();
    }
    public override void Show()
    {
        foreach (string element in Elements)
            UI.GetAllElements()[element].Show();
    }
    public override void Hide()
    {
        foreach (string element in Elements)
            UI.GetAllElements()[element].Hide();
    }
    public override void ToggleShow()
    {
        foreach (string element in Elements)
            UI.GetAllElements()[element].ToggleShow();
    }
}
