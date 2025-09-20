namespace Quest.Items;
public class MagicKey : Item
{
    public MagicKey(int amount) : base(amount)
    {
        MaxAmount = 1;
        Description = "A magical key.";
    }
}
