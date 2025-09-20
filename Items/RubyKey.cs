namespace Quest.Items;
public class RubyKey : Item
{
    public RubyKey(int amount) : base(amount)
    {
        MaxAmount = 1;
        Name = "RubyKey";
        Description = "A fancy ruby key.";
    }
}
