using System.Linq;

namespace Quest.Utilities;
public class Node(Point pos)
{
    public Point Position = pos;
    public float GCost, HCost;
    public Node Parent;

    public float FCost => GCost + HCost;

    public override bool Equals(object? obj) => obj is Node other && other.Position == Position;
    public override int GetHashCode() => Position.X * 73856093 ^ Position.Y * 19349663;
}

public static class Pathfinder
{
    private static GameManager gameManager { get; set; } = null!;
    public static void Init(GameManager gameManager)
    {
        Pathfinder.gameManager = gameManager;
    }
    public static Point[]? FindTilePathAStar(Point from, Point to)
    {
        // Setup
        List<Node> openNodes = [new(from)];
        HashSet<Node> closedNodes = [];
        Dictionary<Point, Node?> openMap = [];

        // Keep getting best path
        while (openNodes.Count > 0)
        {
            // Find the node with the lowest FCost
            Node node = openNodes.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();
            
            // Found
            if (node.Position == to)
                return RetracePath(node);

            // Move to closed
            openNodes.Remove(node);
            closedNodes.Add(node);

            // Get neighbors
            foreach (Node neighbor in GetNeighbors(node.Position))
            {
                if (closedNodes.Contains(neighbor)) continue; // Skip if already evaluated

                // Check for better costs
                float tentativeG = node.GCost + (neighbor.Position.DistanceSquared(node.Position) == 1 ? 1 : Constants.SQRT2);
                Node? openNode = openMap.GetValueOrDefault(neighbor.Position);
                if (openNode == null || tentativeG < openNode.GCost)
                {
                    // Update costs
                    neighbor.GCost = tentativeG;
                    neighbor.HCost = Math.Abs(neighbor.Position.X - to.X) + Math.Abs(neighbor.Position.Y - to.Y);
                    neighbor.Parent = node;
                    // Add to open list if not already there
                    if (openNode == null)
                    {
                        openNodes.Add(neighbor);
                        openMap[neighbor.Position] = neighbor;
                    }
                }
            }
        }

        return null;
    }
    private static Point[] RetracePath(Node node)
    {
        List<Point> path = [];
        while (node != null)
        {
            path.Add(node.Position);
            node = node.Parent;
        }
        path.Reverse();
        return [.. path[1..]];
    }
    private static List<Node> GetNeighbors(Point point)
    {
        // Get neighboring points
        Span<Point> neighborOffsets = Constants.AllNeighborTiles;
        List<Node> neighbors = [];
        // Check each neighbor point
        for (int i = 0; i < neighborOffsets.Length; i++)
        {
            Point neighbor = point + neighborOffsets[i];
            if (neighbor.X < 0 || neighbor.X >= Constants.MapSize.X || neighbor.Y < 0 || neighbor.Y >= Constants.MapSize.Y) continue; // Skip out of bounds
            if (gameManager.LevelManager.Level.Tiles[LevelManager.Flatten(neighbor)].IsWalkable)
                neighbors.Add(new Node(neighbor));
        }
        return neighbors;
    } 
}

