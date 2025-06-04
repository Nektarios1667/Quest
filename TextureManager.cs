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
    public static class TextureManager
    {
        public enum TextureID {
            Null,
            // Characters
            BlueMage,
            GrayMage,
            WhiteMage,
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
        public static Dictionary<TextureID, Texture2D> Textures { get; private set; } = new();
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
