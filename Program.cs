﻿global using System;
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
global using Quest.Enemies;

global using static Quest.Managers.TextureManager;

using System.Linq;

static class Program
{
    static void Main(string[] args)
    {
        if (args.Contains("--level-editor"))
        {
            using var levelEditorApp = new Quest.Editor.LevelEditor();
            levelEditorApp.Run();
        }
        else
        {
            using var gameApp = new Quest.Window();
            gameApp.Run();
        }
    }
}