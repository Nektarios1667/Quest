using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Quest.Gui
{
    public class GuiHandler
    {
        public List<Widget> Widgets { get; set; }
        public Texture2D DialogBox { get; private set; }
        public GuiHandler()
        {
            // Initialize the GUI handler
            Widgets = [];
            DialogBox = null;
        }
        public void Update(float deltaTime)
        {
            // Update all widgets
            foreach (Widget widget in Widgets)
            {
                widget.Update(deltaTime);
            }
        }
        public void Draw(SpriteBatch batch)
        {
            // Draw all widgets
            foreach (Widget widget in Widgets)
            {
                if (widget.IsVisible)
                {
                    widget.Draw(batch);
                }
            }
        }
        public void LoadContent(ContentManager content)
        {
            DialogBox = content.Load<Texture2D>("Images/Gui/DialogBox");
        }
    }
}
