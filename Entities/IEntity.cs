using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Entities;
public interface IEntity
{
    RectangleF Bounds { get; }
    ushort UID { get; }
}
