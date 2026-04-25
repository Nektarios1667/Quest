namespace Quest.Items;
public enum RecipeType
{
    Stove,
    Furnace,
}
public class Recipe
{
    public byte Fuel;
    public ItemRef Input;
    public ItemRef Output;
    public Recipe(ItemRef input, ItemRef output, byte fuel = 0)
    {
        Input = input;
        Output = output;
        Fuel = fuel;
    }
    public Recipe(ItemType inputType, byte inputAmount, ItemType outputType, byte outputAmount, byte fuel = 0)
    {
        Input = new(inputType, inputAmount);
        Output = new(outputType, outputAmount);
        Fuel = fuel;
    }
}

public static class RecipeRegistry
{
    public static readonly Dictionary<RecipeType, Dictionary<ItemTypeID, Recipe>> Recipes = [];
    static RecipeRegistry()
    {
        // --- Stove ---
        Recipes[RecipeType.Stove] = new() {
            {
            ItemTypeID.RawBeef, new Recipe(
                ItemTypes.RawBeef, 1,
                ItemTypes.CookedBeef, 1,
                fuel: 1
            )},
            {
            ItemTypeID.RawFish,new Recipe(
                ItemTypes.RawFish, 1,
                ItemTypes.CookedFish, 1,
                fuel: 1
            )},
        };

        // Furnace
        Recipes[RecipeType.Furnace] = new() {
            {
            ItemTypeID.RawCopper, new Recipe(
                ItemTypes.RawCopper, 1,
                ItemTypes.Copper, 1,
                fuel: 2
            )},
            {
            ItemTypeID.RawGold,new Recipe(
                ItemTypes.RawGold, 1,
                ItemTypes.Gold, 1,
                fuel: 2
            )},
            {
            ItemTypeID.RawIron,new Recipe(
                ItemTypes.RawIron, 1,
                ItemTypes.Iron, 1,
                fuel: 2
            )},
        };
    }

    public static Item? UseRecipe(Item ingredient, RecipeType type, Item? fuel)
    {
        // Get recipe
        if (Recipes[type].TryGetValue(ingredient.Type.TypeID, out var recipe))
        {
            // Check correct amount and consume
            if (ingredient.Type.TypeID == recipe.Input.Type.TypeID && ingredient.Amount >= recipe.Input.Amount &&
                (recipe.Fuel <= 0 || (fuel != null && fuel.Amount >= recipe.Fuel)))
            {
                ingredient.Amount -= recipe.Input.Amount;
                if (recipe.Fuel > 0)
                    fuel!.Amount -= recipe.Fuel;
                return new(recipe.Output);
            }
        }

        // Failed
        return null;
    }
    public static Item? CheckRecipe(Item ingredient, RecipeType type)
    {
        // Get recipe
        if (Recipes[type].TryGetValue(ingredient.Type.TypeID, out var recipe))
        {
            // Check correct amount and consume
            if (ingredient.Amount >= recipe.Input.Amount)
                return new(recipe.Output);
        }

        // Failed
        return null;
    }
}