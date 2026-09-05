using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.World;

public class LevelTransition
{
    public Rectangle Area { get; private set; }
    public LevelPath DestinationLevel { get; private set; }
    public Point DestinationPosition { get; private set; }
    public bool IsRelativeX { get; private set; }
    public bool IsRelativeY { get; private set; }
    public LevelTransition(Rectangle area, LevelPath destinationLevel, Point destinationPosition, bool isRelativeX = false, bool isRelativeY = false)
    {
        Area = area;
        DestinationLevel = destinationLevel;
        DestinationPosition = destinationPosition;
        IsRelativeX = isRelativeX;
        IsRelativeY = isRelativeY;
    }
    private static bool IsTransitioning = false;
    public static void TransitionToLevel(GameManager gameManager, LevelPath destLevel, Point dest, bool isRelativeX = false, bool isRelativeY = false)
    {
        if (IsTransitioning) return;

        // Read the level
        bool read = LevelFileManager.ReadLevel(gameManager, destLevel.ToString(), reload: false);
        if (!read)
        {
            Logger.Error($"Failed to teleport to level '{destLevel.LevelName}' - read failed");
            return;
        }

        IsTransitioning = true;
        TimerManager.SetTimer("ScreenFadeOut", 1.5f, null);
        Point finalDest = new(dest.X + (isRelativeX ? CameraManager.TileCoord.X : 0), dest.Y + (isRelativeY ? CameraManager.TileCoord.Y : 0));
        TimerManager.SetTimer("TransitionToLevel", 1.5f, () => RunTransitionLevel(gameManager, destLevel, finalDest.ToByteCoord()));

        Logger.System($"Teleporting to level '{destLevel.LevelName}' @ {dest}");
    }
    private static void RunTransitionLevel(GameManager gameManager, LevelPath destLevel, ByteCoord dest)
    {
        IsTransitioning = false;

        // Load the level
        bool loaded = gameManager.LevelManager.LoadLevel(gameManager, destLevel.ToString());
        if (!loaded)
        {
            Logger.Error($"Failed to teleport to level '{destLevel.LevelName}' - load failed");
            return;
        }

        // Move camera to dest
        CameraManager.CameraDest = (dest * Constants.TileSize).ToVector2() + new Vector2(Constants.TileSize.X / 2, 0);
        CameraManager.Camera = CameraManager.CameraDest;
        CameraManager.Update(gameManager, 0f); // Force update to avoid visual glitches

        TimerManager.SetTimer("ScreenFadeIn", 1.5f, null);
        Logger.System($"Teleported to level '{destLevel.LevelName}' @ {dest}");
    }
    public override string ToString()
    {
        return $"{Area.Location.X}, {Area.Location.Y}, {Area.Width}, {Area.Height} | " +
            $"{DestinationLevel.LevelName} @" +
            $" {(IsRelativeX && DestinationPosition.X >= 0 ? "+" : "") + DestinationPosition.X}, {(IsRelativeY && DestinationPosition.Y >= 0 ? "+" : "") + DestinationPosition.Y}";
    }
}
