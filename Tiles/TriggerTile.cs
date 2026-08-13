using Quest.Editor;
using Quest.Editor.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Tiles;

public enum TileEffect : byte
{
    None,
    OpenDoor,
    SpawnItem,
}

public abstract class TriggerTile : Tile, IHasState, IEditableTile, IHasLevelData
{
    public TileEffect EffectType { get; set; }
    public ByteCoord EffectCoord { get; set; }
    public LevelPath EffectLevel { get; set; }
    public bool Activated { get; protected set; } = false;
    // Optional
    public ItemRef? SpawnItem { get; set; } = null;
    public TriggerTile(TileTypeID type, Point location, string levelName, TileEffect effectType, ByteCoord effectCoord, LevelPath effectLevel) : base(location, type)
    {
        EffectType = effectType;
        EffectCoord = effectCoord;
        EffectLevel = effectLevel;
    }
    public virtual void RunAction(GameManager gameManager, PlayerManager player)
    {
        if (Activated) return;
        Activated = true;

        // Get tile
        Tile? tile = gameManager.LevelManager.GetTile(EffectLevel, EffectCoord.ToPoint());
        if (tile == null) return;

        // --- Action ---
        // Open
        if (EffectType == TileEffect.OpenDoor)
        {
            if (tile is Door door) door.Open(gameManager);
        }
        // Spawn Item
        else if (EffectType == TileEffect.SpawnItem)
        {
            if (SpawnItem != null)
                gameManager.LevelManager.Level.Loot.Add(new(SpawnItem, (tile.Location + Constants.TileHalfSize) * Constants.TileSize));
        }
    }
    public abstract void WriteState(BinaryWriter writer, GameManager gameManager);
    public abstract void ReadState(BinaryReader reader, GameManager gameManager);
    public virtual void Edit(EditorManager editorManager)
    {
        // Input fields
        List<InputField> fields = [
            new("Tile Effect", null, dropdownOptions: Enum.GetNames<TileEffect>(), placeholder: EffectType),
                new("Effected Tile Coord X", PopupFactory.IsByte, placeholder: EffectCoord.X),
                new("Effected Tile Coord Y", PopupFactory.IsByte, placeholder: EffectCoord.Y),
                new("Effected Tile Level", null, placeholder: EffectLevel.LevelName)
        ];


        // Window
        var (success, values) = PopupFactory.ShowInputForm("Pressure Plate Editor", fields.ToArray());
        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("Pressure plate edit failed.");
            return;
        }

        // Shared fields
        EffectType = Enum.Parse<TileEffect>(values[0]);
        EffectCoord = new(byte.Parse(values[1]), byte.Parse(values[2]));
        EffectLevel = new(EffectLevel.WorldName, values[3]);
    }
    public void WriteLevelData(BinaryWriter writer)
    {
        writer.Write((byte)EffectType);
        writer.Write(EffectCoord);
        writer.Write(EffectLevel.LevelName);
    }
    public void ReadLevelData(BinaryReader reader, LevelPath levelPath)
    {
        EffectType = (TileEffect)reader.ReadByte();
        EffectCoord = reader.ReadByteCoord();
        EffectLevel = new(levelPath.WorldName, reader.ReadString());
    }
}
