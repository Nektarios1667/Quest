using Quest.Editor;
using Quest.Editor.Managers;
using ScottPlot.Interactivity;
using SharpDX.MediaFoundation.DirectX;
using System.IO;

namespace Quest.Tiles;

public class TimedPressurePlate : TriggerTile
{
    Timer? Timer { get; set; }
    float Time { get; set; }
    public TimedPressurePlate(Point location, string levelName, TileEffect effectType, ByteCoord effectCoord, LevelPath effectLevel, float time) : base(TileTypeID.TimedPressurePlate, location, levelName, effectType, effectCoord, effectLevel)
    {
        Time = time;
    }
    public override void Draw(GameManager gameManager)
    {
        // Draw tile
        Point dest = CameraManager.TileToScreen(Location);
        int row = Timer != null ? (int)(Timer.Progress * 4) + 1 : 0;
        Rectangle source = GetAnimationSource(Type.Texture, 0, row: row);
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: source, scale: Constants.TileSizeScale);
    }
    public override void OnPlayerEnter(GameManager gameManager, PlayerManager player)
    {
        Timer = TimerManager.NewTimer($"TimedPressurePlate_{UID}", Time, () => RunAction(gameManager));
    }
    public override void WriteState(BinaryWriter writer, GameManager gameManager)
    {
        writer.Write(Activated);
        writer.Write(Timer?.Left ?? 0);
    }
    public override void ReadState(BinaryReader reader, GameManager gameManager)
    {
        Activated = reader.ReadBoolean();
        float timeLeft = reader.ReadSingle();
        if (Activated)
            TimerManager.SetTimer($"TimedPressurePlate_{UID}", timeLeft, () => RunAction(gameManager));
    }
    public override void RunAction(GameManager gameManager)
    {
        base.RunAction(gameManager);
        Timer = null;
    }
    public override void Edit(EditorManager editorManager)
    {
        base.Edit(editorManager);

        // Set time length
        var (success, values) = PopupFactory.ShowInputForm("Timed Pressure Plate Editor", [
            new("Timer Length", PopupFactory.IsPositiveFloat, placeholder: Time)]);

        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("Timed pressure plate edit failed.");
            return;
        }

        Time = float.Parse(values[0]);
    }
    public override void WriteLevelData(BinaryWriter writer)
    {
        base.WriteLevelData(writer);
        writer.Write(Time);
    }
    public override void ReadLevelData(BinaryReader reader, LevelPath levelPath)
    {
        base.ReadLevelData(reader, levelPath);
        Time = reader.ReadSingle();
    }
}
