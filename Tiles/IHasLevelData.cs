using System.IO;

namespace Quest.Tiles;

// Used for tiles that have extra information attached for .qlv files.
// For example, signs store what text they contain.
public interface IHasLevelData
{
    public void WriteLevelData(BinaryWriter writer);
    public void ReadLevelData(BinaryReader reader, LevelPath levelPath);
}
