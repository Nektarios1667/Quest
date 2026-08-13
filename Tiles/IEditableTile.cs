using Quest.Editor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Tiles;

// Any tile that has custom values able to be edited in the level editor.
// For example, chests can be edited to select their loot table.
public interface IEditableTile
{
    void Edit(EditorManager editorManager);
}
