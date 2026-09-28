using Quest.Editor;
using Quest.Editor.Managers;
using ScottPlot.Interactivity;
using System.IO;

namespace Quest.Tiles;

public class DryWall : Tile, IEditableTile, IHasLevelData
{
    public Color Color { get; private set; } = Color.White;
    public DryWall(Point location) : base(location, TileTypeID.DryWall) { }
    public override void Draw(GameManager gameManager)
    {
        Point dest = CameraManager.TileToScreen(Location);
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: gameManager.LevelManager.TileTextureSource(this), scale: Constants.TileSizeScale, color: Color);
    }
    public void Edit(EditorManager editorManager)
    {
        var (success, values) = PopupFactory.ShowInputForm("DryWall Editor", [
            new("R", PopupFactory.IsByte),
            new("G", PopupFactory.IsByte),
            new("B", PopupFactory.IsByte)]);
        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("DryWall edit failed.");
            return;
        }

        Color color = new(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]));
        Color = color;
    }
    public void WriteLevelData(BinaryWriter writer)
    {
        writer.Write(Color);
    }
    public void ReadLevelData(BinaryReader reader, LevelPath levelPath)
    {
        Color = reader.ReadColor();
    }
    public string GetDataString()
    {
        return $"color: {Color.R},{Color.G},{Color.B}";
    }
}
