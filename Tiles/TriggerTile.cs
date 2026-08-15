using Quest.Editor;
using Quest.Editor.Managers;
using ScottPlot.Interactivity;
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
    SpawnEnemy,
    RunCommand,
}

public abstract class TriggerTile : Tile, IHasState, IEditableTile, IHasLevelData
{
    public TileEffect EffectType { get; set; }
    public ByteCoord EffectCoord { get; set; }
    public LevelPath EffectLevel { get; set; }
    public bool Activated { get; protected set; } = false;
    // Optional
    public ItemRef? SpawnItem { get; set; } = null;
    public Enemy? SpawnEnemy { get; set; } = null;
    public string? Command { get; set; } = null;
    public TriggerTile(TileTypeID type, Point location, string levelName, TileEffect effectType, ByteCoord effectCoord, LevelPath effectLevel) : base(location, type)
    {
        EffectType = effectType;
        EffectCoord = effectCoord;
        EffectLevel = effectLevel;
    }
    public virtual void RunAction(GameManager gameManager)
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
        // Spawn enemy
        else if (EffectType == TileEffect.SpawnEnemy)
        {
            if (SpawnEnemy != null)
            {
                SpawnEnemy.Reset();
                gameManager.LevelManager.Level.Enemies[SpawnEnemy.UID] = SpawnEnemy;
            }
        }
        // Run command
        else if (EffectType == TileEffect.RunCommand)
        {
            if (Command != null)
            {
                CommandManager.Execute(Command);
            }
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
            new("Effected Tile Level", null, placeholder: EffectLevel.LevelName),
            new("[Optional] Spawn Item Type", null, EditorManager.ItemsOptionsWNone, placeholder: SpawnItem == null ? "NONE" : SpawnItem.Type),
            new("[Optional] Spawn Item Amount", PopupFactory.IsByte, placeholder: SpawnItem == null ? "0" : SpawnItem.Type),
            //new("[Optional] Spawn Enemy", null, placeholder: SpawnEnemy),
            new("[Optional] Command", null, placeholder: SpawnEnemy),
        ];


        // Window
        var (success, values) = PopupFactory.ShowInputForm("Trigger Tile Editor", fields.ToArray());
        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("Trigger tile edit failed.");
            return;
        }

        // Shared fields
        EffectType = Enum.Parse<TileEffect>(values[0]);
        EffectCoord = new(byte.Parse(values[1]), byte.Parse(values[2]));
        EffectLevel = new(EffectLevel.WorldName, values[3]);
        // Optional
        if (values[4] != "" && values[4] != "NONE" && values[5] != "" && values[5] != "0") SpawnItem = new(ItemTypes.All[(byte)Enum.Parse(typeof(ItemTypeID), values[4])], byte.Parse(values[5]));
        else SpawnItem = null;

        //if (values[6] != "") // TODO
        
        if (values[6] != "") Command = values[6];
        else Command = null;
    }
    public virtual void WriteLevelData(BinaryWriter writer)
    {
        writer.Write((byte)EffectType);
        writer.Write(EffectCoord);
        writer.Write(EffectLevel.LevelName);
    }
    public virtual void ReadLevelData(BinaryReader reader, LevelPath levelPath)
    {
        EffectType = (TileEffect)reader.ReadByte();
        EffectCoord = reader.ReadByteCoord();
        EffectLevel = new(levelPath.WorldName, reader.ReadString());
    }
}
