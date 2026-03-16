using Quest.Interaction;
using System.ComponentModel;

namespace Quest.Tiles;

public class DisplayCase : Tile
{
    public Interaction.Container Container { get; private set; }
    public DisplayCase(Point location) : base(location, TileTypeID.DisplayCase)
    {
        Container = new([null]);
    }
    public override void Draw(GameManager gameManager)
    {
        // Draw normal tile
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: gameManager.LevelManager.TileTextureSource(this), scale: Constants.TileSizeScale);

        // Draw displayed item
        if (Container.Items[0] != null)
            DrawTexture(gameManager.Batch, Container.Items[0]!.Texture, dest + Constants.TileHalfSize + new Point(0, (int)(Math.Sin(GameManager.GameTime) * 2)), scale: 3, origin: new Vector2(8, 8));
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.DisplayCaseUI.BindContainer(Container);
        player.OpenInterface(UserInterface.DisplayCaseUI);
        StateManager.OverlayState = OverlayState.Container;
    }
}

