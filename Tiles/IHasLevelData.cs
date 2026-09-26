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
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        WriteLevelData(writer);
        writer.Close();

        return stream.ToArray();
    }
    public void SetLevelData(byte[] data, LevelPath levelPath)
    {
        if (data.Length <= 0) return;

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        ReadLevelData(reader, levelPath);
    }
    public string GetDataString();
}
