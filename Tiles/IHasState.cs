using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Tiles;

// IHasState is used for tiles that need to save their current state to the .qsv save files
public interface IHasState
{
    public TileTypeID TypeID {  get; }
    public ByteCoord Location { get; }
    public void WriteState(BinaryWriter writer, GameManager gameManager);
    public void ReadState(BinaryReader reader, GameManager gameManager);
}
