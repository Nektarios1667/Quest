﻿using Xna = Microsoft.Xna.Framework;

namespace Quest.Gui;

public abstract class Widget
{
    public bool Expired { get; protected set; } = false;
    public Xna.Point Location { get; set; }
    public bool IsVisible { get; set; } = true;
    public Widget(Xna.Point location)
    {
        Location = location;
    }
    public abstract void Draw(SpriteBatch batch);
    public virtual void Update(float deltaTime) { }
}
