namespace Quest.Items;
public class Orange : Item
{
    public Orange(int amount) : base(amount)
    {
        MaxAmount = 5;
        Description = "A fresh juicy orange.";
    }
}
