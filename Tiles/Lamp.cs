
using Quest.Editor;
using Quest.Editor.Managers;
using System.IO;

namespace Quest.Tiles;

public class Lamp : Tile, IDynamicTile, IEditableTile, IHasLevelData
{
    public byte LightRadius { get; set; }
    public Lamp(Point location, byte lightRadius = 10) : base(location, TileTypeID.Lamp)
    {
        LightRadius = lightRadius;
    }
    public override void Draw(GameManager gameManager)
    {
        base.Draw(gameManager);

        Color tintColor = Color.Lerp(Color.Transparent, Color.Yellow, Math.Clamp(LightRadius / 15f, 0.3f, 0.75f));
        gameManager.Batch.FillRectangle(new(CameraManager.TileToScreen(Location).ToVector2(), Constants.TileSize), tintColor);

        LightingManager.SetLight($"LampTile_{X}_{Y}", Location.ToPoint(), LightRadius, singleFrame: true);
    }
    public void Edit(EditorManager editorManager)
    {
        var (success, values) = PopupFactory.ShowInputForm("Lamp Editor", [new("Light Radius", PopupFactory.IsByte, placeholder: LightRadius)]);
        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("Lamp edit failed.");
            return;
        }
        LightRadius = byte.Parse(values[0]);
    }
    public void WriteLevelData(BinaryWriter writer)
    {
        writer.Write(LightRadius);
    }
    public void ReadLevelData(BinaryReader reader, LevelPath levelPath)
    {
        LightRadius = reader.ReadByte();
    }
}
