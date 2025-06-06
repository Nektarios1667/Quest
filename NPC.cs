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
        public bool HasTalked { get; set; }
        public bool IsTalking { get; set; }
        public Dialog DialogBox { get; private set; }
        public IGameManager Game { get; private set; }
        public Point Location { get; set; }
        public string Name { get; set; }
        public string Dialog { get; set; }
        public TextureID Texture { get; set; }
        public Color TextureColor { get; set; }
        public float Scale { get; set; }
        // Private
        private Point textureSize { get; set; }
        private Point tilemap { get; set; }
        private Point tilesize { get; set; }

        public NPC(IGameManager game, TextureID texture, Point location, string name, string dialog, Color textureColor = default, float scale = 1)
        {
            IsTalking = false;
            HasTalked = false;
            Game = game;
            Texture = texture;

            // Private
            textureSize = TextureManager.Metadata[Texture].Size;
            tilemap = TextureManager.Metadata[Texture].TileMap;
            tilesize = (textureSize / tilemap);

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
            Game.Batch.FillRectangle(new((rect.Location - tilesize).ToVector2(), source.Size.ToVector2() * Scale), Constants.DebugPinkTint);
        }
        public void Update()
        {
            // State
            if (DialogBox.Displayed == Dialog) HasTalked = true;

            // Speaking
            if (!Game.Inventory.Opened && Vector2.DistanceSquared(Game.Camera / Constants.TileSize, Location.ToVector2()) <= 4)
            {
                if (!IsTalking)
                {
                    DialogBox.IsVisible = true;
                    DialogBox.Displayed = "";
                    IsTalking = true;
                }
            }
            // Hiding if away
            else
            {
                DialogBox.IsVisible = false;
                IsTalking = false;
            }
        }
    }
}
