using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Quest.Gui
{

    public class NotificationArea(Point location, int height, SpriteFont font, Color? color = null) : Widget(location)
    {
        private class Notification(string text, Color color, float duration = 2f)
        {
            public string Text { get; set; } = text;
            public float Duration { get; set; } = duration;
            public float Timer { get; set; } = 0f;
            public Color Color { get; set; } = color;
            public Color BaseColor { get; set; } = color;
        }
        public SpriteFont Font { get; set; } = font;
        private List<Notification> Notifications { get; set; } = [];
        public int Height { get; set; } = height;
        public Color Color { get; set; } = color ?? Color.Black;
        public Point Offset { get; set; } = new(0, 0);

        public override void Update(float deltaTime)
        {
            if (!IsVisible) return;
            for (int n = 0; n < Notifications.Count; n++)
            {
                // Start from newest
                Notification notif = Notifications[n];
                // Time
                notif.Timer += deltaTime;
                if (notif.Timer >= notif.Duration)
                {
                    Notifications.RemoveAt(n);
                    continue;
                }

                // Fade away
                if (notif.Timer / notif.Duration >= .25f)
                    notif.Color = notif.BaseColor * (1 - ((notif.Timer * 4 / 3) / notif.Duration - .25f));
            }
        }
        public override void Draw(SpriteBatch batch)
        {
            if (Notifications.Count == 0 || !IsVisible) return;
            for (int n = 0; n < Notifications.Count; n++)
            {
                Notification notif = Notifications[n];
                Point dest = Location + Offset - new Point(0, (int)Font.MeasureString(notif.Text).Y * n / 2);
                batch.DrawString(Font, notif.Text, dest.ToVector2(), notif.Color, 0f, Vector2.Zero, .5f, SpriteEffects.None, 0f);
            }
        }
        public void AddNotification(string text, Color? color = null, float duration = 4f)
        {
            Notifications.Insert(0, new Notification(text, color ?? Color, duration));
            // Remove oldest if too many
            if (Notifications.Count > Height)
                Notifications.RemoveAt(Notifications.Count - 1);
        }
    }
}
