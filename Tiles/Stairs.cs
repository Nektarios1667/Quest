using System;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class Stairs : Tile
{
    public string DestLevel { get; set; }
    public Point DestPosition { get; set; }
    public Stairs(Xna.Point location, string destLevel, Xna.Point destPosition) : base(location)
    {
        IsWalkable = true;
        DestLevel = destLevel;
        DestPosition = destPosition;
    }
    public override void OnPlayerEnter(IGameManager game)
    {
        // Load another level
        Console.WriteLine($"[System] Teleporting to level '{DestLevel}' @ {DestPosition.X}, {DestPosition.Y}");
        game.LoadLevel(DestLevel);
        game.CameraDest = (DestPosition * Constants.TileSize).ToVector2() + new Vector2(Constants.TileSize.X / 2, 0);
        game.Camera = game.CameraDest;
    }
}
