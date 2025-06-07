using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Gui
{
    public abstract class Widget
    {
        public bool Expired { get; protected set; } = false;
        public Xna.Vector2 Location { get; set; }
        public bool IsVisible { get; set; } = true;
        public Widget(Xna.Vector2 location)
        {
            Location = location;
        }
        public abstract void Draw(SpriteBatch batch);
        public virtual void Update(float deltaTime) {}
    }
}
