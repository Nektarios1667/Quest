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

namespace Quest
{
    public class NPC
    {
        public bool IsTalking { get; set; }
        public Dialog DialogBox { get; private set; }
        public GameHandler Game { get; private set; }
        public Point Location { get; set; }
        public string Name { get; set; }
        public string Dialog { get; set; }
        public Texture2D Texture { get; set; }
        public Color TextureColor { get; set; }
        public float Scale { get; set; }
        public NPC(GameHandler game, Texture2D texture, Point location, string name, string dialog, Color textureColor = default, float scale = 1)
        {
            IsTalking = false;
            Game = game;
            Texture = texture;
            Location = location;
            Name = name;
            Dialog = dialog;
            TextureColor = textureColor == default ? Color.White : textureColor;
            Scale = scale;
            DialogBox = new Dialog(game.Gui, new(Constants.Middle.X - 600, Constants.Window.Y - 190), new(1200, 100), new(194, 125, 64), Color.Black, $"[{name}] {dialog}", Game.Window.PixelOperator, borderColor: new(36, 19, 4)) { IsVisible = false };
            game.Gui.Widgets.Add(DialogBox);
        }
        public void Draw()
        {
            Rectangle rect = new(new((int)(Game.Time * 3) % 4 * Game.MageSize.X, 0), Game.MageSize);
            Game.Batch.Draw(Texture, Location.ToVector2() * Constants.TileSize - Game.Camera + Constants.Middle, rect, TextureColor, 0f, Game.MageHalfSize.ToVector2(), scale:Scale, SpriteEffects.None, 0);
        }
        public void Update()
        {
            if (Vector2.DistanceSquared(Game.Camera / Constants.TileSize, Location.ToVector2()) <= 4)
            {
                if (!IsTalking)
                {
                    DialogBox.IsVisible = true;
                    DialogBox.Displayed = "";
                    IsTalking = true;
                    Logger.Log($"Taling to {Name}");
                }
            }
            else
            {
                DialogBox.IsVisible = false;
                IsTalking = false;
            }
        }
    }
}
