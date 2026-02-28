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
    public event Action<Slot>? SlotClicked;
    private List<string> SlotElements { get; set; } = [];
    private Dictionary<string, UIElement> Elements { get; set; }
    public SpriteBatch Batch { get; private set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public UserInterface(SpriteBatch batch, Dictionary<string, UIElement>? elements = null)
    {
        Elements = elements ?? [];
        Batch = batch;
    }
    public void Update()
    {
        if (!IsEnabled || !IsVisible) return;

        foreach (var element in Elements.Values)
        {
            if (element.IsVisible && element.IsEnabled)
                element.Update(this);
        }
    }
    public void Draw()
    {
        if (!IsVisible) return;

        foreach (var element in Elements.Values)
        {
            if (element.IsVisible)
                element.Draw(this);
        }
    }
    public void BindSlots(Item?[] items)
    {
        for (int i = 0; i < Math.Min(SlotElements.Count, items.Length); i++)
            if (Elements[SlotElements[i]] is Slot slot)
                slot.SetItem(items[i]);
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
            slot.OnClicked += () => SlotClicked?.Invoke(slot);
        }

        return true;
    }
    public void RemoveElement(string name)
    {
        Elements.Remove(name);
        SlotElements.Remove(name);
    }
    public Dictionary<string, UIElement> GetElements() => Elements;
    public string[] GetSlotElements() => [.. SlotElements];
}
