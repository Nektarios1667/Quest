using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quest.Gui;
using System.Security.Policy;
using static Quest.TextureManager;
using MonoGame.Extended;
using SharpDX.Direct3D11;

namespace Quest
{
    public class NPC
    {
        public bool HasSpoken { get; set; }
        public bool IsTalking => DialogBox.IsVisible && DialogBox.IsSpeaking;
        public Dialog DialogBox { get; private set; }
        public IGameManager Game { get; private set; }
        public Point Location { get; set; }
        public string Name { get; set; }
        public string Dialog { get; set; }
        public TextureID Texture { get; set; }
        public Color TextureColor { get; set; }
        public float Scale { get; set; }
        public bool Important { get; set; }
        // Private
        private Point textureSize { get; set; }
        private Point tilemap { get; set; }
        private Point tilesize { get; set; }
        private Point speechsize { get; set; }

        public NPC(IGameManager game, TextureID texture, Point location, string name, string dialog, Color textureColor = default, float scale = 1, bool important = true)
        {
            HasSpoken = false;
            Game = game;
            Important = important;
            Texture = texture;

            // Private
            textureSize = TextureManager.Metadata[Texture].Size;
            tilemap = TextureManager.Metadata[Texture].TileMap;
            tilesize = (textureSize / tilemap);
            speechsize = TextureManager.Metadata[TextureID.Speech].Size;

            Location = location;
            Name = name;
            Dialog = dialog;
            TextureColor = textureColor == default ? Color.White : textureColor;
            Scale = scale;
            DialogBox = new Dialog(game.Gui, new(Constants.Middle.X - 600, Constants.Window.Y - 190), new(1200, 100), new(194, 125, 64), Color.Black, $"[{name}] {dialog}", Game.PixelOperator, borderColor: new(36, 19, 4)) { IsVisible = false };
            game.Gui.Widgets.Add(DialogBox);
        }
        public void Draw()
        {
            // Npc
            Rectangle source = new(new((int)(Game.Time * 2) % tilemap.X * tilesize.X, 0), tilesize);
            Vector2 origin = new(tilesize.X / 2, tilesize.Y);
            Rectangle rect = new(((Location.ToVector2() + Constants.HalfVec) * Constants.TileSize - Game.Camera + Constants.Middle).ToPoint(), tilesize);
            DrawTexture(Game.Batch, Texture, rect, color: TextureColor, scale: new(Scale), source:source, origin:origin);
            // Debug
            if (Constants.DRAW_HITBOXES)
                Game.Batch.FillRectangle(new((rect.Location - tilesize).ToVector2(), source.Size.ToVector2() * Scale), Constants.DebugPinkTint);
            // Speech bubble
            // DrawTexture(Game.Batch, TextureID.Speech, new(rect.X - (int)(speechsize.X*Scale*2), rect.Y - (int)(tilesize.Y*Scale) - (int)(speechsize.Y/4 * Scale) - (int)(Game.Time % 2)*2, 40, 20), source: new(0, speechsize.Y/4 * ((HasSpoken ? 2 : 0) + (Important ? 1 : 0)), speechsize.X, speechsize.Y/4), scale:new(Scale*1.5f));
        }
        public void Update()
        {
            if (DialogBox.HasSpoken) HasSpoken = true;
            // Speaking
            if (!Game.Inventory.Opened && Vector2.DistanceSquared(Game.Camera / Constants.TileSize, Location.ToVector2()) <= 4)
            {
                if (!IsTalking)
                    DialogBox.IsVisible = true;
            }
            // Hiding if away
            else
            {
                DialogBox.IsVisible = false;
                DialogBox.Displayed = "";
            }
        }
    }
}
