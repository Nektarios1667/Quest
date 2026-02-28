namespace Quest.Gui;


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
    public Point Offset = new(0, 0);
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
                notif.Color = notif.BaseColor * (1 - (notif.Timer * 4 / 3 / notif.Duration - .25f));
            else
                notif.Color = notif.BaseColor;
        }
    }
    public override void Draw(SpriteBatch batch)
    {
        if (Notifications.Count == 0 || !IsVisible) return;
        for (int n = 0; n < Notifications.Count; n++)
        {
            Notification notif = Notifications[n];
            Point textSize = Font.MeasureString(notif.Text).ToPoint() / Constants.TwoPoint;
            Point dest = Location + Offset - new Point(textSize.X / 2, (textSize.Y + 5) * n / 2);
            batch.DrawString(Font, notif.Text, dest.ToVector2(), notif.Color, 0f, Vector2.Zero, .5f, SpriteEffects.None, 0f);
        }
    }
    public void AddNotification(string text, Color? color = null, float duration = 4f)
    {
        // Remove repeats
        if (Notifications.Count > 0 && Notifications[0].Text == text && Notifications[0].Color == color)
        {
            Notifications[0].Timer = 0f; // Reset timer
            Notifications[0].Duration = duration; // Reset duration
            return;
        }

        Notifications.Insert(0, new Notification(text, color ?? Color, duration));
        // Remove oldest if too many
        if (Notifications.Count > Height)
            Notifications.RemoveAt(Notifications.Count - 1);
    }
}
