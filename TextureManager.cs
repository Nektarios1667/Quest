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
            if (Textures.TryGetValue(id, out var texture))
                return (texture, true);

            Logger.Error($"Texture with name '{id}' not found.");
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
            if (scale == default) scale = found ? Vector2.One : (tex.Bounds.Size / rect.Size).ToVector2();
            batch.Draw(tex, rect.Location.ToVector2(), source, color, rotation, origin, scale, effects, layerDepth);
        }
        public static void UnloadTexture(TextureID id)
        {
            if (!Textures.Remove(id))
                throw new KeyNotFoundException($"Texture with name '{id}' not found.");
        }


        private static Texture2D GenerateMissingTexture(GraphicsDevice gfx)
        {
            int size = 32;
            var tex = new Texture2D(gfx, size, size);
            Color[] data = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isMagenta = ((x + y) / 16) % 2 == 0;
                    data[y * size + x] = isMagenta ? Color.Magenta : Color.Black;
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}
