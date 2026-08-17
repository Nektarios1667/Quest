using System.IO;
using System.Windows.Forms;

namespace Quest.Tiles;

public class PressurePlate : TriggerTile
{
    public PressurePlate(Point location, string levelName, TileEffect effectType, ByteCoord effectCoord, LevelPath effectLevel) : base(TileTypeID.PressurePlate, location, levelName, effectType, effectCoord, effectLevel)
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
        Activate(gameManager);
    }
    public override void WriteState(BinaryWriter writer, GameManager gameManager)
    {
        // If a pressure plate is even written to the save, then it means it's activated
    }
    public override void ReadState(BinaryReader reader, GameManager gameManager)
    {
        // If a pressure plate is even written to the save, then it means it's activated
        Activated = true;
    }
}
