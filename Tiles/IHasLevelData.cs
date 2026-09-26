using System.IO;

namespace Quest.Tiles;

// Used for tiles that have extra information attached for .qlv files.
// For example, signs store what text they contain.
public interface IHasLevelData
{
    public void WriteLevelData(BinaryWriter writer);
    public void ReadLevelData(BinaryReader reader, LevelPath levelPath);
    public byte[] GetLevelData()
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        WriteLevelData(writer);
        writer.Close();

        return stream.ToArray();
    }
    public void SetLevelData(byte[] data, LevelPath levelPath)
    {
        var stream = new MemoryStream(data);
        var reader = new BinaryReader(stream);

        ReadLevelData(reader, levelPath);
    }
}
