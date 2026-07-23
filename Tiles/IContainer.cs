namespace Quest.Tiles;

public interface IContainer
{
    public Interaction.Container Container { get; }
    public Point Location { get; }
}
