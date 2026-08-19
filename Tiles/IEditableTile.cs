using Quest.Editor.Managers;

namespace Quest.Tiles;

// Any tile that has custom values able to be edited in the level editor.
// For example, chests can be edited to select their loot table.
public interface IEditableTile
{
    void Edit(EditorManager editorManager);
}
