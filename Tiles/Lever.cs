using System.IO;

namespace Quest.Tiles;

public class Lever : TriggerTile
{
    public Lever(Point location, string levelName, TileEffect effectType, ByteCoord effectCoord, LevelPath effectLevel) : base(TileTypeID.Lever, location, levelName, effectType, effectCoord, effectLevel)
    {

    }
    public override void Draw(GameManager gameManager)
    {
        // Draw tile
        Point dest = CameraManager.TileToScreen(Location);
        Rectangle source = GetAnimationSource(Type.Texture, 0, row: Activated ? 1 : 0);
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: source, scale: Constants.TileSizeScale);
    }
    public override void OnPlayerEnter(GameManager gameManager, PlayerManager player)
    {
        if (TimerManager.IsCompleteOrMissing($"LeverCooldown_{UID}"))
        {
            Activate(gameManager);
            TimerManager.SetTimer($"LeverCooldown_{UID}", 1, null);
        }
    }
    public override void Activate(GameManager gameManager)
    {
        Activated = !Activated;

        // Get tile
        Tile? tile = gameManager.LevelManager.GetTile(EffectLevel, EffectCoord.ToPoint());
        if (tile == null) return;

        RunAction(gameManager, tile);

        // Toggle
        if (EffectType == TileEffect.ToggleCloseDoor)
            EffectType = TileEffect.ToggleOpenDoor;
        else if (EffectType == TileEffect.ToggleOpenDoor)
            EffectType = TileEffect.ToggleCloseDoor;
    }
    public override void WriteState(BinaryWriter writer, GameManager gameManager)
    {
        writer.Write(Activated);
        writer.Write(TimerManager.TryGetTimer($"LeverCooldown_{UID}")?.Left ?? 0f);
    }
    public override void ReadState(BinaryReader reader, GameManager gameManager)
    {
        Activated = reader.ReadBoolean();
        TimerManager.SetTimer($"LeverCooldown_{UID}", reader.ReadSingle(), null);
    }
}
