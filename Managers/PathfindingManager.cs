using Migs.MPath.Core;
using Migs.MPath.Core.Data;
using Migs.MPath.Core.Interfaces;
using Quest.World;
using System.Linq;

namespace Quest.Managers;

public static class PathfindingManager
{
    public class Agent(int size) : IAgent { public int Size { get; set; } = size; }
    public static Pathfinder Pathfinder { get; private set; }
    public static Cell[,] Grid { get; private set; }
    public static Agent PathAgent { get; private set; }
    static PathfindingManager()
    {
        var settings = new PathfinderSettings();
        settings.IsDiagonalMovementEnabled = false;
        settings.IsMovementBetweenCornersEnabled = false;
        settings.IsCellWeightEnabled = true;

        Grid = new Cell[Constants.NativeResolutionTiles.X, Constants.NativeResolutionTiles.Y];

        Pathfinder = new(Grid, settings);
        Pathfinder.EnablePathCaching();

        PathAgent = new(1);
    }
    public static void SetGrid(Level level, Point start, Point size) => SetGrid(level, start.X, start.Y, size.X, size.Y);
    public static void SetGrid(Level level, int startX, int startY, int width, int height)
    {
        DebugManager.StartBenchmark($"PathfindingGrid");

        for (int y = startY; y < startY + height; y++)
            for (int x = startX; x < startX + width; x++)
                SetNode(level.Tiles[y * LevelManager.MapSize.X + x], x - startX, y - startY);

        Pathfinder.InvalidateCache();
        DebugManager.EndBenchmark($"PathfindingGrid");
    }
    private static void SetNode(Tile tile, int x, int y)
    {
        // Set properties - a weight of 1000 or more is assumed to be unwalkable by a non-player
        Grid[x, y].IsWalkable = tile.IsWalkable && tile.Weight < 1000;
        Grid[x, y].Coordinate = new(x, y);
        Grid[x, y].Weight = tile.Weight;
    }
    public static Coordinate[]? GetPath(Point from, Point to) => GetPath(from.X, from.Y, to.X, to.Y);
    public static Coordinate[]? GetPath(int fromX, int fromY, int toX, int toY)
    {
        if (fromX < 0 || fromY < 0 || toX >= Grid.GetLength(0) || toY >= Grid.GetLength(1))
        {
            Logger.Error("Pathfinding calculation is off grid.");
            return null;

        }

        var result = Pathfinder.GetPath(PathAgent, new(fromX, fromY), new(toX, toY));
        return result.Path?.ToArray();
    }
}
