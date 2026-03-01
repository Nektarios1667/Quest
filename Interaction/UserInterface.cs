using Quest.Managers;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;

public partial class UserInterface
{
    public event Action<int ,UserInterface>? OnSlotClick;
    public event Action<int, UserInterface>? OnSlotDrop;
    public event Action<int, UserInterface>? OnSlotHover;
    private List<string> SlotElements { get; set; } = [];
    public Container? BoundContainer { get; private set; }
    private Dictionary<string, UIElement> Elements { get; set; }
    public SpriteBatch Batch { get; private set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public UserInterface(SpriteBatch batch, Dictionary<string, UIElement>? elements = null)
    {
        Elements = elements ?? [];
        Batch = batch;
    }
    public void Update(string? tag = null)
    {
        if (!IsEnabled || !IsVisible) return;

        foreach (var element in Elements.Values)
        {
            if (element.IsVisible && element.IsEnabled && (tag == null || element.Tags.Contains(tag)))
                element.Update(this);
        }
        Reload();
    }
    public void Draw(string? tag = null)
    {
        if (!IsVisible) return;

        foreach (var element in Elements.Values)
        {
            if (element.IsVisible && (tag == null || element.Tags.Contains(tag)))
                element.Draw(this);
        }
    }
    public void BindContainer(Container cont) => BoundContainer = cont;
    public void UnbindContainer() => BoundContainer = null;
    public void Reload()
    {
        if (BoundContainer == null) return;

        for (int i = 0; i < Math.Min(SlotElements.Count, BoundContainer.Items.Length); i++)
        {
            if (Elements[SlotElements[i]] is Slot slot)
                slot.SetItem(BoundContainer.Items[i]);
        }
    }
    public bool AddElement(string name, UIElement element)
    {
        // Check
        if (Elements.ContainsKey(name)) return false;

        // Add
        Elements[name] = element;
        if (element is Slot slot)
        {
            SlotElements.Add(name);
            slot.OnClicked += () => OnSlotClick?.Invoke(SlotElements.IndexOf(name), this);
            slot.OnDropped += () => OnSlotDrop?.Invoke(SlotElements.IndexOf(name), this);
            slot.OnHovered += () => OnSlotHover?.Invoke(SlotElements.IndexOf(name), this);
        }

        return true;
    }
    public void RemoveElement(string name)
    {
        Elements.Remove(name);
        SlotElements.Remove(name);
    }
    public Dictionary<string, UIElement> GetElements(string? tag = null) => tag == null ? Elements : Elements.Where(kv => kv.Value.Tags.Contains(tag)).ToDictionary(kv => kv.Key, kv => kv.Value);
    public Slot GetSlot(string name) => Elements[name] as Slot ?? throw new Exception($"Element '{name}' is not a Slot.");
    public Slot GetSlot(int idx) => GetSlot(SlotElements[idx]);

    // Visibilty/Enablement
    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
    public void ToggleEnable() => IsEnabled = !IsEnabled;
    public void Show() => IsVisible = true;
    public void Hide() => IsVisible = false;
    public void ToggleVisible() => IsVisible = !IsVisible;

}
