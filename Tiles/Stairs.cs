using Quest.Editor;
using Quest.Editor.Managers;
using Quest.World;
using System.IO;

namespace Quest.Tiles;

public class Stairs : Tile, IEditableTile, IHasLevelData
{
    public LevelPath DestLevel { get; set; }
    public ByteCoord Dest { get; set; }
    public Stairs(Point location, LevelPath destLevel, Point destPosition) : base(location, TileTypeID.Stairs)
    {
        Dest = new(destPosition);
        DestLevel = destLevel;
    }
    public override void OnPlayerEnter(GameManager gameManager, PlayerManager player)
    {
        if (DestLevel.IsNull()) return;

        Teleport(gameManager);
    }
    private void Teleport(GameManager gameManager)
    {
        LevelTransition.TransitionToLevel(gameManager, DestLevel, Dest);
    }
    public void Edit(EditorManager editorManager)
    {
        var (success, values) = PopupFactory.ShowInputForm("Stair Editor", [
        new("Level", null, placeholder: DestLevel.LevelName),
                new("Spawn X", PopupFactory.IsByte, placeholder: Dest.X),
                new("Spawn Y", PopupFactory.IsByte, placeholder: Dest.Y)]);
        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("Stair edit failed.");
            return;
        }
        if (values[0].Contains('\\') || values[0].Contains('/'))
        {
            Logger.Error("Invalid level format. Stairs can not go to other worlds.");
            return;
        }

        // Level
        DestLevel = new(editorManager.CurrentLevel.WorldName, values[0]);
        Dest = new(byte.Parse(values[1]), byte.Parse(values[2]));
    }
    public void WriteLevelData(BinaryWriter writer)
    {
        // Write destination
        writer.Write(DestLevel.LevelName ?? "");
        writer.Write(Dest);
    }
    public void ReadLevelData(BinaryReader reader, LevelPath levelPath)
    {
        DestLevel = new LevelPath(levelPath.WorldName, reader.ReadString());
        Dest = new(reader.ReadByte(), reader.ReadByte());
    }
}
