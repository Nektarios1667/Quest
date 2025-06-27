namespace Quest.Items;
public class Item
{
    public string DisplayName => $"{Amount} {StringTools.FillCamelSpaces(Name)}";
    public PlayerManager PlayerManager { get; protected set; }
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public int Amount { get; set; }
    public int MaxAmount { get; protected set; }
    public TextureID Texture { get; protected set; }
    public int UID { get; protected set; }
    public Item(PlayerManager playerManager, int amount)
    {
        PlayerManager = playerManager;
        Amount = amount;
        MaxAmount = Constants.MaxStack;
        Name = GetType().Name;
        Description = "Inventory item";
        Texture = (TextureID)Enum.Parse(typeof(TextureID), GetType().Name);
        UID = UIDManager.NewUID("Items");
    }
    public virtual void PrimaryUse() { }
    public virtual void SecondaryUse() { }

    public static Item ItemFromName(PlayerManager player, string name, int amount)
    {
        string fullTypeName = $"Quest.Items.{name}"; // Replace with your actual namespace
        var type = Type.GetType(fullTypeName);

        if (type == null || !typeof(Item).IsAssignableFrom(type))
            throw new ArgumentException($"Invalid item name '{name}'");

        var created = (Item?)Activator.CreateInstance(type, player, amount);
        return created ?? throw new InvalidOperationException($"Failed to create item '{name}'");
    }
}
