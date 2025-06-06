using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Content;

namespace Quest
{
    public class Metadata(Point size, Point tileMap, string type)
    {
        public Point Size { get; private set; } = size;
        public Point TileMap { get; private set; } = tileMap;
        public string Type { get; private set; } = type;
    }
    public static class TextureManager
    {
        public enum TextureID {
            Null,
            // Characters
            BlueMage,
            GrayMage,
            WhiteMage,
            PurpleWizard,
            WhiteWizard,
            // Gui
            CursorArrow,
            DialogBox,
            GuiBackground,
            Slot,
            Speech,
            // Items
            Pickaxe,
            Sword,
            Palantir,
            // Tiles
            Dirt,
            Flooring,
            Grass, 
            Sand,
            Sky,
            Stairs,
            StoneWall,
            Template,
            Water,
        }

        private static List<string> errors = [];
        public static Dictionary<TextureID, Texture2D> Textures { get; private set; } = [];
        public static Dictionary<TextureID, Metadata> Metadata { get; private set; } = [];
        
        private static Texture2D? MissingTexture { get; set; } = null;
        private static ContentManager? Content { get; set; }
        public static void LoadTextures(ContentManager content)
        {
            Content = content;
            MissingTexture = GenerateMissingTexture(content.GetGraphicsDevice());
            // Load all
            Textures[TextureID.BlueMage] = content.Load<Texture2D>($"Images/Characters/BlueMage");
            Textures[TextureID.GrayMage] = content.Load<Texture2D>($"Images/Characters/GrayMage");
            Textures[TextureID.WhiteMage] = content.Load<Texture2D>($"Images/Characters/WhiteMage");
            Textures[TextureID.PurpleWizard] = content.Load<Texture2D>($"Images/Characters/PurpleWizard");
            Textures[TextureID.WhiteWizard] = content.Load<Texture2D>($"Images/Characters/WhiteWizard");
            Textures[TextureID.CursorArrow] = content.Load<Texture2D>($"Images/Gui/CursorArrow");
            Textures[TextureID.DialogBox] = content.Load<Texture2D>($"Images/Gui/DialogBox");
            Textures[TextureID.GuiBackground] = content.Load<Texture2D>($"Images/Gui/GuiBackground");
            Textures[TextureID.Slot] = content.Load<Texture2D>($"Images/Gui/Slot");
            Textures[TextureID.Speech] = content.Load<Texture2D>($"Images/Gui/Speech");
            Textures[TextureID.Pickaxe] = content.Load<Texture2D>($"Images/Items/Pickaxe");
            Textures[TextureID.Sword] = content.Load<Texture2D>($"Images/Items/Sword");
            Textures[TextureID.Dirt] = content.Load<Texture2D>($"Images/Tiles/Dirt");
            Textures[TextureID.Flooring] = content.Load<Texture2D>($"Images/Tiles/Flooring");
            Textures[TextureID.Grass] = content.Load<Texture2D>($"Images/Tiles/Grass");
            Textures[TextureID.Sand] = content.Load<Texture2D>($"Images/Tiles/Sand");
            Textures[TextureID.Sky] = content.Load<Texture2D>($"Images/Tiles/Sky");
            Textures[TextureID.Stairs] = content.Load<Texture2D>($"Images/Tiles/Stairs");
            Textures[TextureID.StoneWall] = content.Load<Texture2D>($"Images/Tiles/StoneWall");
            Textures[TextureID.Template] = content.Load<Texture2D>($"Images/Tiles/Template");
            Textures[TextureID.Water] = content.Load<Texture2D>($"Images/Tiles/Water");
            Logger.Log("Textures loaded successfully.");

            // Metadata
            Metadata[TextureID.BlueMage] = new(Textures[TextureID.BlueMage].Bounds.Size, new(4, 5), "character");
            Metadata[TextureID.GrayMage] = new(Textures[TextureID.GrayMage].Bounds.Size, new(4, 5), "character");
            Metadata[TextureID.WhiteMage] = new(Textures[TextureID.WhiteMage].Bounds.Size, new(4, 5), "character");
            Metadata[TextureID.PurpleWizard] = new(Textures[TextureID.PurpleWizard].Bounds.Size, new(4, 1), "character");
            Metadata[TextureID.WhiteWizard] = new(Textures[TextureID.WhiteWizard].Bounds.Size, new(4, 1), "character");
            Metadata[TextureID.CursorArrow] = new(Textures[TextureID.CursorArrow].Bounds.Size, new(1, 1), "gui");
            Metadata[TextureID.DialogBox] = new(Textures[TextureID.DialogBox].Bounds.Size, new(1, 1), "gui");
            Metadata[TextureID.GuiBackground] = new(Textures[TextureID.GuiBackground].Bounds.Size, new(1, 1), "gui");
            Metadata[TextureID.Slot] = new(Textures[TextureID.Slot].Bounds.Size, new(1, 1), "gui");
            Metadata[TextureID.Speech] = new(Textures[TextureID.Speech].Bounds.Size, new(1, 4), "gui");
            Metadata[TextureID.Pickaxe] = new(Textures[TextureID.Pickaxe].Bounds.Size, new(1, 1), "item");
            Metadata[TextureID.Sword] = new(Textures[TextureID.Sword].Bounds.Size, new(1, 1), "item");
            Metadata[TextureID.Dirt] = new(Textures[TextureID.Dirt].Bounds.Size, new(4, 4), "tile");
            Metadata[TextureID.Flooring] = new(Textures[TextureID.Flooring].Bounds.Size, new(4, 4), "tile");
            Metadata[TextureID.Grass] = new(Textures[TextureID.Grass].Bounds.Size, new(4, 4), "tile");
            Metadata[TextureID.Sand] = new(Textures[TextureID.Sand].Bounds.Size, new(4, 4), "tile");
            Metadata[TextureID.Sky] = new(Textures[TextureID.Sky].Bounds.Size, new(4, 1), "tile");
            Metadata[TextureID.Stairs] = new(Textures[TextureID.Stairs].Bounds.Size, new(4, 1), "tile");
            Metadata[TextureID.StoneWall] = new(Textures[TextureID.StoneWall].Bounds.Size, new(4, 4), "tile");
            Metadata[TextureID.Template] = new(Textures[TextureID.Template].Bounds.Size, new(4, 4), "tile");
            Metadata[TextureID.Water] = new(Textures[TextureID.Water].Bounds.Size, new(4, 4), "tile");
            Logger.Log("Metadata loaded successfully.");
        }
        public static (Texture2D Texture, bool Found) GetTexture(TextureID id)
        {
            // Found
            if (Textures.TryGetValue(id, out var texture))
                return (texture, true);

            // Missing
            if (!errors.Contains($"getfail-{id}")) {
                Logger.Error($"Texture with name '{id}' not found.");
                errors.Add($"getfail-{id}");
            }
            if (MissingTexture == null)
            {
                if (Content == null)
                    Logger.Error("ContentManager is not initialized.", exit:true);

                Logger.Log("Generating missing texture.");
                MissingTexture = GenerateMissingTexture(Content.GetGraphicsDevice());
            }
            return (MissingTexture, false);
        }
        public static void DrawTexture(SpriteBatch batch, TextureID id, Rectangle rect, Rectangle? source = null, Color color = default, float rotation = 0f, Vector2 origin = default, Vector2 scale = default, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            (Texture2D tex, bool found) = GetTexture(id);
            if (color == default) color = Color.White;
            if (origin == default) origin = Vector2.Zero;
            // If scale is not set, default to 1, unless its missing then stretch to texture size
            if (scale == default && found) scale = Vector2.One;
            else if (!found) scale = (rect.Size / tex.Bounds.Size).ToVector2();

            batch.Draw(tex, rect.Location.ToVector2(), source, color, rotation, origin, scale, effects, layerDepth);
        }
        public static void UnloadTexture(TextureID id)
        {
            if (!Textures.Remove(id) && !errors.Contains($"unloadfail-{id}"))
            {
                Logger.Error($"Texture with name '{id}' not found.");
                errors.Add($"unloadfail-{id}");
            }
        }

        private static Texture2D GenerateMissingTexture(GraphicsDevice gfx)
        {
            var tex = new Texture2D(gfx, 2, 2);
            Color[] data = [Color.Magenta, Color.Black, Color.Black, Color.Magenta];
            tex.SetData(data);
            return tex;
        }
    }
}
