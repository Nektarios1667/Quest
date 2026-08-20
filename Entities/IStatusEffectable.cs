namespace Quest.Entities;

public interface IStatusEffectable
{
    public Dictionary<StatusEffect, float> StatusEffects { get; set; }
    public ushort UID { get; }
    public void Heal(GameManager gameManager, int amount);
    public void Hurt(GameManager gameManager, int amount);
}
