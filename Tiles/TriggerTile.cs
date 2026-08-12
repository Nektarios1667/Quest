using System;
using System.Collections.Generic;
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

public abstract class TriggerTile : Tile, IHasState
{
    public TileEffect EffectType { get; set; }
    public ByteCoord EffectCoord { get; set; }
    public LevelPath EffectLevel { get; set; }
    public bool Activated { get; protected set; } = false;
    // Optional
    public ItemRef? SpawnItem { get; set; } = null;
    public bool? LockPedestalItem { get; set; } = null;
    public ItemRef? PedestalItem { get; set; } = null;
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
}
