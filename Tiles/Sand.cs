﻿namespace Quest.Tiles;

public class Sand : Tile
{
    public Sand(Point location) : base(location)
    {
        IsWalkable = true;
    }
}
