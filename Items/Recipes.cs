using System.Linq;

namespace Quest.Items;
public enum RecipeType
{
    Stove,
    Furnace,
    Crafter,
}
public class Recipe
{
    public byte Fuel;
    public ItemRef[] Inputs;
    public ItemRef Output;
    public Recipe(IEnumerable<ItemRef> input, ItemRef output, byte fuel = 0)
    {
        Inputs = [.. input];
        Output = output;
        Fuel = fuel;
    }
}

public class RecipeKey : IEquatable<RecipeKey>
{
    public readonly byte[] Items;
    public readonly int Type;
    public RecipeKey(RecipeType type, IEnumerable<ItemRef> inputs)
    {
        Type = (int)type;
        Items = [.. inputs.Select(i => (byte)i.Type.TypeID).OrderBy(id => id)];
    }
    public bool Equals(RecipeKey? other)
    {
        if (other == null || Type != other.Type || Items.Length != other.Items.Length)
            return false;
        for (int i = 0; i < Items.Length; i++)
            if (Items[i] != other.Items[i])
                return false;
        return true;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int h = Type;
            for (int i = 0; i < Items.Length; i++)
                h = (h * 31) ^ Items[i];
            return h;
        }
    }
}

public static class RecipeRegistry
{
    public static readonly Dictionary<RecipeKey, Recipe> Recipes = [];
    public static void RegisterRecipe(RecipeType type, IEnumerable<ItemRef> inputs, ItemRef output, byte fuel = 0)
    {
        var key = new RecipeKey(type, inputs);
        Recipes[key] = new Recipe(inputs, output, fuel);
    }
    static RecipeRegistry()
    {
        // --- Stove ---
        RegisterRecipe(RecipeType.Stove, [new(ItemTypes.RawBeef, 1)], new(ItemTypes.CookedBeef, 1), fuel: 1);
        RegisterRecipe(RecipeType.Stove, [new(ItemTypes.RawFish, 1)], new(ItemTypes.CookedFish, 1), fuel: 1);
        // --- Furnace ---
        RegisterRecipe(RecipeType.Furnace, [new(ItemTypes.RawCopper, 1)], new(ItemTypes.Copper, 1), fuel: 2);
        RegisterRecipe(RecipeType.Furnace, [new(ItemTypes.RawGold, 1)], new(ItemTypes.Gold, 1), fuel: 2);
        RegisterRecipe(RecipeType.Furnace, [new(ItemTypes.RawIron, 1)], new(ItemTypes.Iron, 1), fuel: 2);
        // --- Crafter ---
        RegisterRecipe(RecipeType.Crafter, [new(ItemTypes.Iron, 4), new(ItemTypes.Coal, 1)], new(ItemTypes.Lantern, 1));
    }

    public static Item? UseRecipe(Item[] inputs, Item? fuel, RecipeType type)
    {
        RecipeKey inputsRecipe = new RecipeKey(type, inputs.Select(i => i.GetItemRef()));
        if (Recipes.TryGetValue(inputsRecipe, out var matchingRecipe))
        {
            // Check to make sure all the ingredients are present in the required amounts
            foreach (var input in matchingRecipe.Inputs)
            {
                var matchingItem = inputs.FirstOrDefault(i => i.Type.TypeID == input.Type.TypeID);
                if (matchingItem == null || matchingItem.Amount < input.Amount)
                    return null; // Missing ingredient or not enough amount
            }

            // Check fuel amount if required
            if (matchingRecipe.Fuel > 0)
            {
                if (fuel == null || fuel.Amount < matchingRecipe.Fuel)
                    return null; // Not enough fuel
            }

            // Consume items
            foreach (var input in matchingRecipe.Inputs)
            {
                var matchingItem = inputs.First(i => i.Type.TypeID == input.Type.TypeID);
                matchingItem.Consume(input.Amount);
            }

            // Consume fuel
            if (matchingRecipe.Fuel > 0)
                fuel!.Consume(matchingRecipe.Fuel);
            
            // If we reach here, all ingredients are present in the required amounts
            return Item.Create(matchingRecipe.Output.Type, matchingRecipe.Output.Amount);
        }

        // Failed
        return null;
    }
}