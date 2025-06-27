global using System;
global using System.Collections.Generic;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Input;
global using MonoGame.Extended;
global using Quest.Entities;
global using Quest.Items;
global using Quest.Managers;
global using Quest.Tiles;
global using Quest.Utilities;
global using Quest.Decals;
global using static Quest.Managers.TextureManager;

#if EDITOR
    using var game = new Quest.Editor.Window();
    game.Run();
#else
using var game = new Quest.Window();
game.Run();
#endif