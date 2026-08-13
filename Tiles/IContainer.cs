namespace Quest.Tiles;

// Any tile that stores items.
// For example, crates.
public interface IContainer
{
    public Interaction.Container Container { get; }
    public ByteCoord Location { get; }
}
