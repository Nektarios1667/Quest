using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Tiles;
public interface IContainer
{
    public Interaction.Container Container { get; }
    public ByteCoord Location { get; }
}
