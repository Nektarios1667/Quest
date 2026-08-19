using System.IO;

namespace Quest.Tiles;

// Used for tiles that need to save their current state to the .qsv save files.
// For example, Doors need to save whether they have been opemed or not.
public interface IHasState
{
    public ushort UID { get; }
    public TileTypeID TypeID { get; }
    public ByteCoord Location { get; }
    public void WriteState(BinaryWriter writer, GameManager gameManager);
    public void ReadState(BinaryReader reader, GameManager gameManager);
}
