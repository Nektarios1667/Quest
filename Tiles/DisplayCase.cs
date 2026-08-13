using Quest.Editor;
using Quest.Editor.Managers;
using Quest.Interaction;
using System.ComponentModel;
using System.IO;

namespace Quest.Tiles;

public class DisplayCase : Tile, IContainer, IHasLevelData
{
    public Interaction.Container Container { get; private set; }
    public DisplayCase(Point location, string levelName) : base(location, TileTypeID.DisplayCase)
    {
        Container = new([null]);
        SaveManager.SaveContainer(this, levelName);
    }
    public override void Draw(GameManager gameManager)
    {
        // Draw normal tile
        Point dest = CameraManager.TileToScreen(Location);
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: gameManager.LevelManager.TileTextureSource(this), scale: Constants.TileSizeScale);

        // Draw displayed item
        if (Container.Items[0] != null)
            DrawTexture(gameManager.Batch, Container.Items[0]!.Texture, dest + Constants.TileHalfSize + new Point(0, (int)(Math.Sin(GameManager.GameTime) * 2)), scale: new(3), origin: new Vector2(8, 8));
    }
    public override void OnPlayerCollide(GameManager gameManager,PlayerManager player)
    {
        UserInterface.DisplayCaseUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.DisplayCaseUI);
        gameManager.StateManager.OverlayState = OverlayState.Container;
    }
    public void Edit(EditorManager editorManager)
    {
        var (success, values) = PopupFactory.ShowInputForm("Lamp Editor", [
        new("Item", null, EditorManager.ItemsOptionsWNone, placeholder: Container.Items[0] == null ? "NONE" : Container.Items[0]!.Name),
                new("Amount", PopupFactory.IsNonZeroByte, placeholder: Container.Items[0]?.Amount)]);
        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("Display case edit failed.");
            return;
        }
        Container.Items[0] = new(ItemTypes.All[(byte)Enum.Parse(typeof(ItemTypeID), values[0])], byte.Parse(values[1]));
    }
    public void WriteLevelData(BinaryWriter writer)
    {
        SaveManager.WriteItemData(writer, Container.Items[0]);
    }
    public void ReadLevelData(BinaryReader reader, LevelPath levelPath)
    {
        Item? item = SaveManager.ReadItemData(reader);
        Container.Items[0] = item;
    }
}

