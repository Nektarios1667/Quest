using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Content;
using Quest.World;
using System.Linq;
using System.Windows.Forms;

namespace Quest.Editor.Generator;

public partial class QLVViewer : Form
{
    private GameManager gameManager;
    private int tilesDisplayed = 200;
    private int biomesDisplayed = 200;
    private int decalsDisplayed = 200;
    private Level level;
    // Parent nodes
    private TreeNode levelNode;
    private TreeNode tilesNode;
    private TreeNode biomesNode;
    private TreeNode npcsNode;
    private TreeNode lootsNode;
    private TreeNode decalsNode;
    private TreeNode enemiesNode;
    private TreeNode scriptsNode;
    private TreeNode loadTiles;
    private TreeNode loadBiomes;
    private TreeNode loadDecals;

    public QLVViewer()
    {
        InitializeComponent();

        // Create mock context
        Window window = new();
        window.RunOneFrame();
        TextureManager.LoadTextures(window.Content);

        LevelManager levelManager = new();
        gameManager = new(null!, levelManager, null, null, null);

        SaveTree.AfterExpand += SaveTree_AfterExpand;
    }

    public void LoadLevel(string filepath)
    {
        // Read
        bool success = LevelFileManager.ReadLevel(gameManager, filepath, true);
        if (!success) return;

        level = gameManager.LevelManager.Levels[0];
        FileLabel.Text = filepath;

        SaveTree.Nodes.Clear();
        SaveTree.Update();

        SaveTree.BeginUpdate();

        // Top level
        levelNode = SaveTree.Nodes.Add("Level");
        tilesNode = SaveTree.Nodes.Add("Tiles");
        biomesNode = SaveTree.Nodes.Add("Biomes");
        npcsNode = SaveTree.Nodes.Add("NPCs");
        lootsNode = SaveTree.Nodes.Add("Loot");
        decalsNode = SaveTree.Nodes.Add("Decals");
        enemiesNode = SaveTree.Nodes.Add("Enemies");
        scriptsNode = SaveTree.Nodes.Add("Quill Scripts");

        // Level
        levelNode.Nodes.Add($"Tint: {level.Tint.R}, {level.Tint.G}, {level.Tint.B}");
        levelNode.Nodes.Add($"Spawn: {level.Spawn.X}, {level.Spawn.Y}");

        // Tile
        foreach (Tile tile in level.Tiles[0..tilesDisplayed])
        {
            TreeNode tileNode = tilesNode.Nodes.Add($"Tile {tile.UID}");
            tileNode.Nodes.Add($"Type: {tile.TypeID}");
            tileNode.Nodes.Add($"Pos: {tile.X}, {tile.Y}");
        }
        loadTiles = tilesNode.Nodes.Add("Load Tiles...");
        loadTiles.Nodes.Add("");

        // Biomes
        int b = 1;
        foreach (BiomeType biome in level.Biome[0..biomesDisplayed])
        {
            TreeNode biomeNode = biomesNode.Nodes.Add($"Biome {b}");
            biomeNode.Nodes.Add($"Type: {biome}");
            biomeNode.Nodes.Add($"Pos: {b % Constants.MapSize.X}, {b / Constants.MapSize.X}");

            b++;
        }
        loadBiomes = biomesNode.Nodes.Add("Load Biomes...");
        loadBiomes.Nodes.Add("");

        // NPCs
        foreach (NPC npc in level.NPCs.Values)
        {
            TreeNode npcNode = npcsNode.Nodes.Add($"NPC {npc.UID}");
            npcNode.Nodes.Add($"Name: {npc.Name}");
            npcNode.Nodes.Add($"Tex: {npc.Texture}");
            npcNode.Nodes.Add($"Scale: {npc.Scale}");
            npcNode.Nodes.Add($"Dialog: {npc.Dialog}");
            TreeNode npcShopOptionsNode = npcNode.Nodes.Add($"Shop Options [{npc.ShopOptions.Count}]");

            int o = 1;
            foreach (ShopOption option in npc.ShopOptions)
            {
                TreeNode optionNode = npcShopOptionsNode.Nodes.Add($"Option {o}");
                optionNode.Nodes.Add($"Item: {option.Item.Name}");
                optionNode.Nodes.Add($"Item Amount: {option.Item.Amount}");
                optionNode.Nodes.Add($"Cost: {option.Cost?.Name ?? "FREE"}");
                optionNode.Nodes.Add($"Cost Amount: {option.Cost?.Amount ?? 0}");
                o++;
            }
        }

        // Loot
        foreach (Loot loot in level.Loot)
        {
            TreeNode lootNode = lootsNode.Nodes.Add($"Loot {loot.UID}");
            lootNode.Nodes.Add($"Item: {loot.Item.Type}");
            lootNode.Nodes.Add($"Amount: {loot.Item.Amount}");
        }

        // Decals
        foreach (Decal decal in level.Decals.Values.ToArray()[0..decalsDisplayed])
        {
            TreeNode decalNode = decalsNode.Nodes.Add($"Decal {decal.UID}");
            decalNode.Nodes.Add($"Type: {decal.Type}");
            decalNode.Nodes.Add($"Pos: {decal.X}, {decal.Y}");
        }
        loadDecals = decalsNode.Nodes.Add("Load Decals...");
        loadDecals.Nodes.Add("");

        // Enemies
        foreach (Enemy enemy in level.Enemies.Values)
        {
            TreeNode enemyNode = enemiesNode.Nodes.Add($"Enemy {enemy.UID}");
            enemyNode.Nodes.Add($"Tex: {enemy.Texture}");
            enemyNode.Nodes.Add($"Scale: {enemy.Scale}");
            enemyNode.Nodes.Add($"X: {enemy.Position.X}");
            enemyNode.Nodes.Add($"Y: {enemy.Position.Y}");
            enemyNode.Nodes.Add($"Speed: {enemy.Speed}");
            enemyNode.Nodes.Add($"Health: {enemy.Health}");
            enemyNode.Nodes.Add($"Defense: {enemy.Defense}");
            enemyNode.Nodes.Add($"Damage: {enemy.Damage}");
            enemyNode.Nodes.Add($"Attack Speed: {enemy.AttackSpeed}");
            enemyNode.Nodes.Add($"Attack Range: {enemy.AttackRange}");
            enemyNode.Nodes.Add($"View Range: {enemy.ViewRange}");
            enemyNode.Nodes.Add($"Projectile Tex: {enemy.ProjectileTexture}");
            enemyNode.Nodes.Add($"Projectile Speed: {enemy.ProjectileSpeed}");
        }

        // Scripts
        foreach (QuillScript script in level.Scripts)
        {
            TreeNode scriptNode = scriptsNode.Nodes.Add($"Script {script.Name}");
            TreeNode scriptLinesNode = scriptNode.Nodes.Add($"Lines [{script.SourceCode.Split('\n').Length}]");

            int l = 1;
            foreach (string line in script.SourceCode.Split('\n'))
            {
                scriptLinesNode.Nodes.Add($"{l}: {line}");
                l++;
            }
        }

        SaveTree.EndUpdate();
    }

    private void SelectButton_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog dialog = new OpenFileDialog())
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string[] parts = dialog.FileName.Split(System.IO.Path.DirectorySeparatorChar);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(parts[^1]);

                string result = System.IO.Path.Combine(parts[^3], fileName);

                LoadLevel(new(result));
            }
        }
    }
    private void SaveTree_AfterExpand(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Text == "Load Tiles...")
        {
            for (int i = 0; i < 200; i++)
            {
                Tile tile = level.Tiles[tilesDisplayed];
                TreeNode tileNode = tilesNode.Nodes.Insert(tilesDisplayed, $"Tile {tile.UID}");
                tileNode.Nodes.Add($"Type: {tile.TypeID}");
                tileNode.Nodes.Add($"Pos: {tile.X}, {tile.Y}");

                tilesDisplayed += 1;
            }
            loadTiles.Collapse();
        } else if (e.Node?.Text == "Load Biomes...")
        {
            for (int i = 0; i < 200; i++)
            {
                int b = biomesDisplayed;

                TreeNode biomeNode = biomesNode.Nodes.Insert(tilesDisplayed, $"Biome {b}");
                biomeNode.Nodes.Add($"Type: {level.Biome[b]}");
                biomeNode.Nodes.Add($"Pos: {b % Constants.MapSize.X}, {b / Constants.MapSize.X}");

                biomesDisplayed += 1;
            }

            loadBiomes.Collapse();
        }
        else if (e.Node?.Text == "Load Decals...")
        {
            for (int i = 0; i < 200; i++)
            {
                int d = decalsDisplayed;

                TreeNode decalNode = decalsNode.Nodes.Insert(decalsDisplayed, $"Decal {level.Decals.Values.ToArray()[d].UID}");
                decalNode.Nodes.Add($"Type: {level.Decals.Values.ToArray()[d].Type}");

                decalsDisplayed += 1;
            }

            loadDecals.Collapse();
        }
    }

}
