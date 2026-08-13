using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Tiles;

// Used for tiles that need to save their current state to the .qsv save files.
// For example, Doors need to save whether they have been opemed or not.
public interface IHasLevelData
{
    public void WriteLevelData(BinaryWriter writer);
    public void ReadLevelData(BinaryReader reader, LevelPath levelPath);
}
