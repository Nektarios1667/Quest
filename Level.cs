using System.Collections.Generic;
using Quest.Tiles;
using Xna = Microsoft.Xna.Framework;
namespace Quest;


public class Level
{
    public Decal[] Decals { get; set; }
    public NPC[] NPCs { get; private set; }
    public string Name { get; private set; }
    public List<Loot> Loot { get; private set; }
    public Tile[] Tiles { get; private set; }
    public Xna.Point Spawn { get; private set; }
    public Level(string name, Tile[] tiles, Xna.Point spawn, NPC[] npcs, Loot[] loot, Decal[] decals)
    {
        // Initialize the level
        Name = name;
        Tiles = tiles;
        Spawn = spawn;
        NPCs = npcs;
        Loot = [.. loot];
        Decals = decals;
    }
}
