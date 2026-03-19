using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Migs.MPath.Core;
using Migs.MPath;
using Migs.MPath.Core.Data;
using SharpDX.XAudio2;
using Migs.MPath.Core.Interfaces;
using System.Reflection.Metadata.Ecma335;
using System.Configuration;
using SharpDX.X3DAudio;

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
                SetNode(level.Tiles[y * Constants.MapSize.X + x], x - startX, y - startY);

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
        var result = Pathfinder.GetPath(PathAgent, new(fromX, fromY), new(toX, toY));
        return result.Path?.ToArray();
    }
    private static void PrintGrid(int fromX, int fromY, int toX, int toY, Coordinate[]? path, bool success)
    {
        Console.WriteLine("_______________________");
        Console.WriteLine(success ? "Success" : "Failed");
        for (int x = 0; x < Grid.GetLength(1); x++)
        {
            for (int y = 0; y < Grid.GetLength(0); y++)
            {
                if (fromX == y && fromY == x)
                    Console.Write("F ");
                else if (toX == y && toY == x)
                    Console.Write("T ");
                else if (path != null && path.Contains(new(y, x)))
                    Console.Write("X ");
                else
                    Console.Write(Grid[y, x].IsWalkable ? ". " : "# ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("_______________________");
    }
}
