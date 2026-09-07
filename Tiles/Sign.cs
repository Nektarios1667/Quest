using Quest.Editor;
using Quest.Editor.Managers;
using System.IO;
using System.Security.Policy;

namespace Quest.Tiles;

public class Sign : Tile, IHasDialog, IHasLevelData, IEditableTile
{
    public string GetFullDialog() => $"[Sign] {Text}";
    public string GetName() => "Sign";
    public string Text { get; set; } = string.Empty;
    private bool dialogOpen = false;
    public Sign(Point location, string text) : base(location, TileTypeID.Sign)
    {
        Text = text;
    }
    public override void Draw(GameManager gameManager)
    {
        base.Draw(gameManager);
        if (dialogOpen)
            NPC.DialogsNearby.Add((this, 0)); // Distance of 0 to give top priority
    }
    public override void OnPlayerEnter(GameManager gameManager, PlayerManager player)
    {
        dialogOpen = true;
    }
    public override void OnPlayerExit(GameManager gameManager, PlayerManager player)
    {
        dialogOpen = false;
    }
    public void WriteLevelData(BinaryWriter writer)
    {
        writer.Write(Text);
    }
    public void ReadLevelData(BinaryReader reader, LevelPath _)
    {
        Text = reader.ReadString();
    }
    public void Edit(EditorManager editorManager)
    {
        // Window
        var (success, values) = PopupFactory.ShowInputForm("Sign Editor", [
            new("Text", null),
        ]);

        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("Sign edit failed.");
            return;
        }

        Text = values[0];
    }
}


